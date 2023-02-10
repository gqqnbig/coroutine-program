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

			var falseG = new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((PaperVariable)"x", (PaperVariable)"y")));
			var trueG = new GeneratorType(terminatorTrue, new SequenceType(new SequenceType((PaperVariable)"x", (PaperVariable)"x")));
			var rec = new GeneratorType(new SequenceType(falseG, trueG, new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n")))),
				receive: new SequenceType(new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n"))));

			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("base", new GeneratorType(terminatorFalse, new SequenceType(new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0))))));
			coroutines.Add(new Generator("recursion1", true, rec.Clone()));
			coroutines.Add(new Generator("recursion2", true, rec.Clone()));

			return coroutines;
		}

		[Fact]
		public void TrueTest()
		{
			List<Generator> coroutines = GetRules();
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"String", new ListType((ConcreteType)"String", (PaperInt)3))), ConcreteType.Void)));

			try
			{
				var result = Solver.Solve(coroutines);
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
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"Path", new ListType((ConcreteType)"String", (PaperInt)3))), ConcreteType.Void)));

			try
			{
				var result = Solver.Solve(coroutines);
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
