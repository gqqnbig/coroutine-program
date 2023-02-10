using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;
using Xunit;

namespace GeneratorCalculationTests
{
	public class PatternMatchingTests
	{
		private List<Generator> GetRules()
		{
			var terminatorTrue = new ListType((ConcreteType)"T", PaperStar.Instance);
			//new SequenceType(new SequenceType((ConcreteType)"T", (ConcreteType)"T", (ConcreteType)"T"));
			var terminatorFalse = new ListType((ConcreteType)"F", PaperStar.Instance);
			//new SequenceType(new SequenceType((ConcreteType)"F", (ConcreteType)"F", (ConcreteType)"F"));


			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("true", new GeneratorType(terminatorTrue, new SequenceType(new SequenceType((PaperVariable)"x", (PaperVariable)"x")))));
			coroutines.Add(new Generator("false", true, new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((PaperVariable)"x", (PaperVariable)"y")))));
			coroutines.Add(new Generator("recursion1", true, new GeneratorType(new SequenceType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), (PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n"))),
				receive: new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n")))));
			coroutines.Add(new Generator("recursion2", true, new GeneratorType(new SequenceType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), (PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n"))),
				receive: new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n")))));
			coroutines.Add(new Generator("base", new GeneratorType(terminatorFalse, new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0)))));

			return coroutines;
		}

		[Fact]
		public void TrueTest()
		{
			List<Generator> coroutines = GetRules();
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType((ConcreteType)"String", new ListType((ConcreteType)"String", (PaperInt)3)), ConcreteType.Void)));

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
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType((ConcreteType)"Path", new ListType((ConcreteType)"String", (PaperInt)3)), ConcreteType.Void)));

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
