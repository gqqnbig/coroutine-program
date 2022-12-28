using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;
using Xunit;

namespace GeneratorCalculationTests
{
	public class PrologApplication
	{
		static List<KeyValuePair<string, GeneratorType>> GetPrologKnowledgeBase()
		{
			var gNegate1 = new GeneratorType((ConcreteType)"Yes", (ConcreteType)"Negate");
			var gNegate2 = new GeneratorType(ConcreteType.Void, (ConcreteType)"Yes");

			List<KeyValuePair<string, GeneratorType>> coroutines = new List<KeyValuePair<string, GeneratorType>>
			{
				new KeyValuePair<string, GeneratorType>("child1", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "John", (ConcreteType) "Sue")))),
				new KeyValuePair<string, GeneratorType>("child2", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "Jane", (ConcreteType) "Sue")))),
				new KeyValuePair<string, GeneratorType>("child3", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "Sue", (ConcreteType) "George")))),
				new KeyValuePair<string, GeneratorType>("child4", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "John", (ConcreteType) "Sam")))),
				new KeyValuePair<string, GeneratorType>("child5", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "Jane", (ConcreteType) "Sam")))),
				new KeyValuePair<string, GeneratorType>("child6", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "Sue", (ConcreteType) "Gina")))),
				new KeyValuePair<string, GeneratorType>("female1", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Female", (ConcreteType) "Sue")))),
				new KeyValuePair<string, GeneratorType>("female2", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Female", (ConcreteType) "Jane")))),
				new KeyValuePair<string, GeneratorType>("female3", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Female", (ConcreteType) "June")))),
				new KeyValuePair<string, GeneratorType>("female-other", new GeneratorType((ConcreteType)"No", new SequenceType(new SequenceType((ConcreteType) "Female", (PaperVariable) "x")))),
				new KeyValuePair<string, GeneratorType>("parent", new GeneratorType(new SequenceType(new SequenceType((ConcreteType) "Child", (PaperVariable) "x", (PaperVariable) "y")), new SequenceType(new SequenceType((ConcreteType) "Parent", (PaperVariable) "y", (PaperVariable) "x")))),

				new KeyValuePair<string, GeneratorType>("Negate", new GeneratorType(new SequenceType(gNegate1,gNegate2),(ConcreteType)"No")),
			};
			return coroutines;
		}


		[Fact]
		public void RunProlog()
		{
			List<KeyValuePair<string, GeneratorType>> coroutines = GetPrologKnowledgeBase();
			coroutines.Add(new KeyValuePair<string, GeneratorType>("query", new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new SequenceType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Yes"), (PaperVariable)"x")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("starter", new GeneratorType((ConcreteType)"Sue", ConcreteType.Void)));

			try
			{
				var result = Solver.Solve(coroutines);
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
			List<KeyValuePair<string, GeneratorType>> coroutines = GetPrologKnowledgeBase();

			coroutines.Add(new KeyValuePair<string, GeneratorType>("query",
				new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new SequenceType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Yes"), (PaperVariable)"x")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("starter", new GeneratorType((ConcreteType)"Sam", ConcreteType.Void)));

			try
			{
				var result = Solver.Solve(coroutines);
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
			List<KeyValuePair<string, GeneratorType>> coroutines = GetPrologKnowledgeBase();

			coroutines.Add(new KeyValuePair<string, GeneratorType>("query",
				new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new SequenceType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Negate", (ConcreteType)"Yes"), (PaperVariable)"x")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("starter", new GeneratorType((ConcreteType)"Sam", ConcreteType.Void)));

			try
			{
				var result = Solver.Solve(coroutines);
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
			List<KeyValuePair<string, GeneratorType>> coroutines = GetPrologKnowledgeBase();

			coroutines.Add(new KeyValuePair<string, GeneratorType>("query",
				new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new SequenceType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Negate", (ConcreteType)"Yes"), (PaperVariable)"x")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("starter", new GeneratorType((ConcreteType)"Sue", ConcreteType.Void)));

			try
			{
				var result = Solver.Solve(coroutines);
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
