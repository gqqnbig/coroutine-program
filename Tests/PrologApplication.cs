using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;
using Xunit;
using System.Linq;

namespace GeneratorCalculationTests
{
	public class PrologApplication
	{
		static List<Generator> GetPrologKnowledgeBase()
		{
			var gNegate1 = new CoroutineInstanceType((ConcreteType)"Negate", (ConcreteType)"Yes");
			var gNegate2 = new CoroutineInstanceType((ConcreteType)"Yes", ConcreteType.Void);

			var femaleOtherCondition = new Z3Condition(
				s =>
				{
					var Sue = s.ConcreteSort.Consts.First(c => c.ToString().Equals("Sue"));
					var Jane = s.ConcreteSort.Consts.First(c => c.ToString().Equals("Jane"));
					var June = s.ConcreteSort.Consts.First(c => c.ToString().Equals("June"));
					return s.z3Ctx.MkAnd(
						s.z3Ctx.MkNot(s.z3Ctx.MkEq(s.z3Ctx.MkConst("x", s.ConcreteSort), Sue)),
						s.z3Ctx.MkNot(s.z3Ctx.MkEq(s.z3Ctx.MkConst("x", s.ConcreteSort), Jane)),
						s.z3Ctx.MkNot(s.z3Ctx.MkEq(s.z3Ctx.MkConst("x", s.ConcreteSort), June)));
				});

			var coroutines = new List<Generator>
			{
				new Generator("child1", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Child",(ConcreteType) "John",(ConcreteType) "Sue")), ConcreteType.Void)),
				new Generator("child2", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Child",(ConcreteType) "Jane",(ConcreteType) "Sue")), ConcreteType.Void)),
				new Generator("child3", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Child",(ConcreteType) "Sue",(ConcreteType) "George")), ConcreteType.Void)),
				new Generator("child4", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Child",(ConcreteType) "John",(ConcreteType) "Sam")), ConcreteType.Void)),
				new Generator("child5", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Child",(ConcreteType) "Jane",(ConcreteType) "Sam")), ConcreteType.Void)),
				new Generator("child6", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Child",(ConcreteType) "Sue",(ConcreteType) "Gina")), ConcreteType.Void)),
				new Generator("female1", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Female",(ConcreteType) "Sue")), ConcreteType.Void)),
				new Generator("female2", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Female",(ConcreteType) "Jane")), ConcreteType.Void)),
				new Generator("female3", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Female",(ConcreteType) "June")), ConcreteType.Void)),
				new Generator("female-other", new CoroutineInstanceType(femaleOtherCondition,
					new SequenceType(new TupleType((ConcreteType) "Female", (PaperVariable) "x")),(ConcreteType)"No")),
				new Generator("parent", new CoroutineInstanceType(new SequenceType(new TupleType((ConcreteType) "Parent",(PaperVariable) "y",(PaperVariable) "x")), new SequenceType(new TupleType((ConcreteType) "Child",(PaperVariable) "x",(PaperVariable) "y")))),

				new Generator("Negate", new CoroutineInstanceType((ConcreteType) "No", new SequenceType(gNegate1, gNegate2))),
			};
			return coroutines;
		}


		[Fact]
		public void RunProlog()
		{
			Solver solver = new Solver();
			var coroutines = GetPrologKnowledgeBase();
			coroutines.Add(new Generator("query", new CoroutineInstanceType((PaperVariable)"x", new SequenceType(new TupleType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new TupleType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Yes"))));
			coroutines.Add(new Generator("starter", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Sue")));

			try
			{
				var result = solver.SolveWithBindings(coroutines);
			}
			catch (DeadLockException e)
			{
				Assert.Single(e.YieldsToOutside);
				Assert.Equal((ConcreteType)"Yes", e.YieldsToOutside[0]);
			}
		}

		[Fact]
		public void RunPrologNoMatch()
		{
			var coroutines = GetPrologKnowledgeBase();

			coroutines.Add(new Generator("query", new CoroutineInstanceType((PaperVariable)"x", new SequenceType(new TupleType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new TupleType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Yes"))));
			coroutines.Add(new Generator("starter", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Sam")));

			try
			{
				var result = new Solver().SolveWithBindings(coroutines);
			}
			catch (DeadLockException e)
			{
				if (e.YieldsToOutside.Count == 0)
					return;
				//Assert.True(e.YieldsToOutside.Count > 0, "When x = Sam, the answer should be No.");
				Assert.NotEqual((ConcreteType)"Yes", e.YieldsToOutside[0]);
			}
		}

		[Fact]
		public void RunNegateMatch()
		{
			var coroutines = GetPrologKnowledgeBase();

			coroutines.Add(new Generator("query", new CoroutineInstanceType((PaperVariable)"x", new SequenceType(new TupleType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new TupleType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Negate", (ConcreteType)"Yes"))));
			coroutines.Add(new Generator("starter", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Sam")));

			try
			{
				var result = new Solver().SolveWithBindings(coroutines);
			}
			catch (DeadLockException e)
			{
				Assert.Single(e.YieldsToOutside);
				Assert.Equal((ConcreteType)"Yes", e.YieldsToOutside[0]);
			}
		}


		[Fact]
		public void RunNegateNoMatch()
		{
			var coroutines = GetPrologKnowledgeBase();

			coroutines.Add(new Generator("query", new CoroutineInstanceType((PaperVariable)"x", new SequenceType(new TupleType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new TupleType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Negate", (ConcreteType)"Yes"))));
			coroutines.Add(new Generator("starter", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Sue")));

			try
			{
				var result = new Solver().SolveWithBindings(coroutines);
			}
			catch (DeadLockException e)
			{
				if (e.YieldsToOutside.Count == 0)
					return;
				Assert.NotEqual((ConcreteType)"Yes", e.YieldsToOutside[0]);
			}
		}
	}
}
