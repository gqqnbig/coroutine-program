using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;
using Xunit;

namespace GeneratorCalculation.Tests
{
	public class PatternMatchingTests
	{
		private static List<Generator> GetRules()
		{
			var terminatorTrue = new ListType((ConcreteType)"T", PaperStar.Instance);
			var terminatorFalse = new ListType((ConcreteType)"F", PaperStar.Instance);

			var falseG = new GeneratorType(new Dictionary<SequenceType, List<SequenceType>>
				{
					[new SequenceType((PaperVariable)"x")] = new List<SequenceType> { new SequenceType((PaperVariable)"y") }
				},
				new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"y")), ConcreteType.Void);
			var trueG = new GeneratorType(terminatorTrue, new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"x")));
			var rec = new GeneratorType(
				new Dictionary<SequenceType, List<SequenceType>>
				{
					//Use a "D"ummy type because (PaperInt)0 is not a PaperType.
					[new SequenceType(new ListType((ConcreteType)"D", (PaperVariable)"n"))] = new List<SequenceType> { new SequenceType(new ListType((ConcreteType)"D", (PaperInt)0)) }
				},
				receive: new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n"))),
				yield: new SequenceType(falseG, trueG, new TupleType((PaperVariable)"x", (PaperVariable)"y"), new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n")))));

			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("base", new GeneratorType(terminatorFalse, new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0))))));
			coroutines.Add(new Generator("recursion1", true, rec.Clone()));
			coroutines.Add(new Generator("recursion2", true, rec.Clone()));

			return coroutines;
		}

		private static List<Generator> GetSelfCleaningRules()
		{
			var terminatorTrue = new ListType((ConcreteType)"T", PaperStar.Instance);
			var terminatorFalse = new ListType((ConcreteType)"F", PaperStar.Instance);
			var falseG = new GeneratorType(new Dictionary<SequenceType, List<SequenceType>>
				{
					[new SequenceType((PaperVariable)"x")] = new List<SequenceType> { new SequenceType((PaperVariable)"y") }
				},
				new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"y")), ConcreteType.Void);
			var trueG = new GeneratorType(terminatorTrue, new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"x")));
			var rec = new GeneratorType(
				new Dictionary<SequenceType, List<SequenceType>>
				{
					//Use a "D"ummy type because (PaperInt)0 is not a PaperType.
					[new SequenceType(new ListType((ConcreteType)"D", (PaperVariable)"n"))] = new List<SequenceType> { new SequenceType(new ListType((ConcreteType)"D", (PaperInt)0)) }
				},
				receive: new SequenceType(new ListType(falseG, PaperStar.Instance), new ListType(trueG, PaperStar.Instance), new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n"))), 
				yield: new SequenceType(falseG, trueG, new TupleType((PaperVariable)"x", (PaperVariable)"y"), new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n")))));

			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("base", new GeneratorType(terminatorFalse, new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0))))));
			coroutines.Add(new Generator("recursion1", true, rec.Clone()));
			coroutines.Add(new Generator("recursion2", true, rec.Clone()));

			return coroutines;
		}

		[Fact]
		public void TrueTest()
		{
			List<Generator> coroutines = GetRules();
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType(new TupleType((ConcreteType)"String", new ListType((ConcreteType)"String", (PaperInt)3))), ConcreteType.Void)));

			try
			{
				var result = new Solver().Solve(coroutines);
			}
			catch (DeadLockException e)
			{
				Assert.True(e.YieldsToOutside.Count > 0);
				Assert.True(e.YieldsToOutside[0] is ListType);
				Assert.Equal((ConcreteType)"T", ((ListType)e.YieldsToOutside[0]).Type);
			}
		}


		[Fact]
		public void FalseTest()
		{
			List<Generator> coroutines = GetRules();
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType(new TupleType((ConcreteType)"Path", new ListType((ConcreteType)"String", (PaperInt)3))), ConcreteType.Void)));

			try
			{
				var result = new Solver().Solve(coroutines);
			}
			catch (DeadLockException e)
			{
				Assert.True(e.YieldsToOutside.Count > 0);
				Assert.IsType<ListType>(e.YieldsToOutside[0]);
				Assert.Equal((ConcreteType)"F", ((ListType)e.YieldsToOutside[0]).Type);
			}
		}


		[Fact]
		public void SelfCleaningTrueTest()
		{
			List<Generator> coroutines = GetSelfCleaningRules();
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType(new TupleType((ConcreteType)"String", new ListType((ConcreteType)"String", (PaperInt)3))), ConcreteType.Void)));

			try
			{
				var result = new Solver().Solve(coroutines);
			}
			catch (DeadLockException e)
			{
				Assert.True(e.YieldsToOutside.Count > 0);
				Assert.True(e.YieldsToOutside[0] is ListType);
				Assert.Equal((ConcreteType)"T", ((ListType)e.YieldsToOutside[0]).Type);
			}
		}


		[Fact]
		public void SelfCleaningFalseTest()
		{
			List<Generator> coroutines = GetSelfCleaningRules();
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType(new TupleType((ConcreteType)"Path", new ListType((ConcreteType)"String", (PaperInt)3))), ConcreteType.Void)));

			try
			{
				var result = new Solver().Solve(coroutines);
			}
			catch (DeadLockException e)
			{
				Assert.True(e.YieldsToOutside.Count > 0);
				Assert.IsType<ListType>(e.YieldsToOutside[0]);
				Assert.Equal((ConcreteType)"F", ((ListType)e.YieldsToOutside[0]).Type);
			}
		}
	}
}
