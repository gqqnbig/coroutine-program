using System;
using System.Collections.Generic;
using Xunit;
using GeneratorCalculation;

namespace GeneratorCalculationTests
{
	public class SolverTests
	{
		[Fact]
		public void ContinueYieldAfterReceive()
		{
			var coroutines = new List<Generator>
			{
				new Generator("",new GeneratorType(new SequenceType((ConcreteType)"A",(ConcreteType)"B"),ConcreteType.Void)),
				new Generator("",new GeneratorType((ConcreteType)"C",(ConcreteType)"A"))
			};

			var result = new Solver().SolveWithBindings(coroutines);

			Assert.Equal(new SequenceType((ConcreteType)"C", (ConcreteType)"B"), result.Yield);
		}

		[Fact]
		public void ReceiveStartPosition()
		{
			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("a", new GeneratorType((ConcreteType)"T", (ConcreteType)"S")));
			coroutines.Add(new Generator("b", new GeneratorType((ConcreteType)"S", ConcreteType.Void)));
			coroutines.Add(new Generator("c", new GeneratorType((ConcreteType)"U", (ConcreteType)"S")));

			var result = new Solver().SolveWithBindings(coroutines);
		}

		[Fact]
		public void RunInfiniteLoop()
		{
			//Console.WriteLine("hi");
			var back = Console.Out;
			Console.SetOut(System.IO.TextWriter.Null);

			//Console.WriteLine("hello");
			try
			{

				List<Generator> list = new List<Generator>();

				list.Add(new Generator("a", true, new GeneratorType((ConcreteType)"X", ConcreteType.Void)));
				list.Add(new Generator("b", true, new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
				Assert.Throws<StepLimitExceededException>(() => new Solver().SolveWithBindings(list, steps: 100));
			}
			finally
			{
				Console.SetOut(back);
			}
		}

		[Fact]
		public void SolveSingle()
		{
			var list = new List<Generator>();
			list.Add(new Generator("", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			var g = new Solver().SolveWithBindings(list);

			Assert.Equal(ConcreteType.Void, g.Receive);
			Assert.Contains("Y", g.Yield.ToString());
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

			Assert.Throws<DeadLockException>(() => new Solver().SolveWithBindings(list));
		}

		[Fact]
		public void SingleRemainingNoLock()
		{
			var list = new List<Generator>();
			var g1 = new GeneratorType((ConcreteType)"A", (ConcreteType)"B");
			list.Add(new Generator("g1", g1));

			var g2 = new GeneratorType((ConcreteType)"C", ConcreteType.Void);
			list.Add(new Generator("g2", g2));


			var result = new GeneratorType(new SequenceType((ConcreteType)"C", (ConcreteType)"A"), (ConcreteType)"B");
			Assert.Equal(result, new Solver().SolveWithBindings(list));
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

			var result = new Solver().SolveWithBindings(coroutines);

			Console.WriteLine("Final result:");
			Console.WriteLine(result);
		}

		[Fact]
		public void UseVariable()
		{

			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("a", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new Generator("b", new GeneratorType((PaperVariable)"a", (PaperVariable)"a")));


			var result = new Solver().SolveWithBindings(coroutines);

			Assert.Contains("Y", result.Yield.ToString());
			Assert.Equal(ConcreteType.Void, result.Receive);
		}


		[Fact]
		public void PopReceive()
		{
			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("a", new GeneratorType((ConcreteType)"A", ConcreteType.Void)));
			coroutines.Add(new Generator("b", new GeneratorType(new SequenceType((ConcreteType)"B", (ConcreteType)"C"), (ConcreteType)"A")));

			var result = new Solver().SolveWithBindings(coroutines);

			Assert.Equal(new SequenceType((ConcreteType)"B", (ConcreteType)"C"), result.Yield);
			Assert.Equal(ConcreteType.Void, result.Receive);
		}

		[Fact]
		public void ReceiveCoroutine()
		{
			var coroutines = new List<Generator>();
			var g = new GeneratorType((ConcreteType)"A", ConcreteType.Void);
			coroutines.Add(new Generator("a", g));
			coroutines.Add(new Generator("b", new GeneratorType((ConcreteType)"B", new SequenceType(new ListType(g.Clone(), (PaperInt)1)))));

			var result = new Solver().SolveWithBindings(coroutines);

		}

	}
}
