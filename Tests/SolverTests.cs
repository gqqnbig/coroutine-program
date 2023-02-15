using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using GeneratorCalculation;

namespace GeneratorCalculationTests
{
	public class SolverTests
	{
		[Fact()]
		public void SolveSingle()
		{
			var list = new List<Generator>();
			list.Add(new Generator("", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			var result = Solver.Solve(list);

			Assert.Single(result.Flow);
			Assert.Contains("Y", result.Flow[0].Type.ToString());
		}

		[Fact]
		public void SolveDeadlock()
		{
			var list = new List<Generator>();
			var g1 = new GeneratorType((ConcreteType)"A", (ConcreteType)"B");
			list.Add(new Generator("g1", g1));

			var g2 = new GeneratorType((ConcreteType)"C", ConcreteType.Void);
			list.Add(new Generator("g2", g2));

			var g3 = new GeneratorType((ConcreteType)"D", (ConcreteType)"E");
			list.Add(new Generator("g3", g3));

			Assert.Throws<DeadLockException>(() => Solver.Solve(list));
		}

		[Fact]
		public void SingleRemainingNoLock()
		{
			var list = new List<Generator>();
			var g1 = new GeneratorType((ConcreteType)"A", (ConcreteType)"B");
			list.Add(new Generator("g1", g1));

			var g2 = new GeneratorType((ConcreteType)"C", ConcreteType.Void);
			list.Add(new Generator("g2", g2));


			var result = new GeneratorType(
				new DataFlow(Direction.Yielding, (ConcreteType)"C"),
				new DataFlow(Direction.Resuming, (ConcreteType)"B"),
				new DataFlow(Direction.Yielding, (ConcreteType)"A"));
			Assert.Equal(result, Solver.Solve(list));
		}

		[Fact]
		public void Interleave()
		{
			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("oc1", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new Generator("oc2", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new Generator("fr1", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), (ConcreteType)"Y")));
			coroutines.Add(new Generator("fr2", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), (ConcreteType)"Y")));


			GeneratorType interleave = new GeneratorType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")),
				new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")));
			coroutines.Add(new Generator("interleave", interleave));

			var result = Solver.Solve(coroutines);

			Console.WriteLine("Final result:");
			Console.WriteLine(result);
		}

		[Fact]
		public void UseVariable()
		{

			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("a", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new Generator("b", new GeneratorType((PaperVariable)"a", (PaperVariable)"a")));


			var result = Solver.Solve(coroutines);
			Assert.Single(result.Flow);
			Assert.Contains("Y", result.Flow[0].Type.ToString());
		}


		[Fact]
		public void PopReceive()
		{
			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("a", new GeneratorType((ConcreteType)"A", ConcreteType.Void)));
			coroutines.Add(new Generator("b", new GeneratorType(new SequenceType((ConcreteType)"B", (ConcreteType)"C"), (ConcreteType)"A")));

			var result = Solver.Solve(coroutines);

			Assert.Equal(new DataFlow(Direction.Yielding, (ConcreteType)"B"), result.Flow[0]);
			Assert.Equal(new DataFlow(Direction.Yielding, (ConcreteType)"C"), result.Flow[1]);
		}



	}
}
