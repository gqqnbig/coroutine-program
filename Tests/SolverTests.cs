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
			var list = new List<KeyValuePair<string, GeneratorType>>();
			var g = new GeneratorType((ConcreteType)"Y", ConcreteType.Void);
			list.Add(new KeyValuePair<string, GeneratorType>("x", g));

			Assert.Equal(g, Solver.Solve(list));
		}

		[Fact]
		public void SolveDeadlock()
		{
			var list = new List<KeyValuePair<string, GeneratorType>>();
			var g1 = new GeneratorType((ConcreteType)"A", (ConcreteType)"B");
			list.Add(new KeyValuePair<string, GeneratorType>("g1", g1));

			var g2 = new GeneratorType((ConcreteType)"C", ConcreteType.Void);
			list.Add(new KeyValuePair<string, GeneratorType>("g2", g2));

			var g3 = new GeneratorType((ConcreteType)"D", (ConcreteType)"E");
			list.Add(new KeyValuePair<string, GeneratorType>("g3", g3));

			Assert.Throws<DeadLockException>(() => Solver.Solve(list));
		}

		[Fact]
		public void SingleRemainingNoLock()
		{
			var list = new List<KeyValuePair<string, GeneratorType>>();
			var g1 = new GeneratorType((ConcreteType)"A", (ConcreteType)"B");
			list.Add(new KeyValuePair<string, GeneratorType>("g1", g1));

			var g2 = new GeneratorType((ConcreteType)"C", ConcreteType.Void);
			list.Add(new KeyValuePair<string, GeneratorType>("g2", g2));


			var result = new GeneratorType(new SequenceType((ConcreteType)"C", (ConcreteType)"A"), (ConcreteType)"B");
			Assert.Equal(result, Solver.Solve(list));
		}

		[Fact]
		public void Interleave()
		{
			List<KeyValuePair<string, GeneratorType>> coroutines = new List<KeyValuePair<string, GeneratorType>>();
			coroutines.Add(new KeyValuePair<string, GeneratorType>("oc1", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("oc2", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("fr1", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), (ConcreteType)"Y")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("fr2", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), (ConcreteType)"Y")));


			GeneratorType interleave = new GeneratorType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")),
				new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("interleave", interleave));

			var result = Solver.Solve(coroutines);

			Console.WriteLine("Final result:");
			Console.WriteLine(result);
		}

		[Fact]
		public void UseVariable()
		{

			List<KeyValuePair<string, GeneratorType>> coroutines = new List<KeyValuePair<string, GeneratorType>>();
			coroutines.Add(new KeyValuePair<string, GeneratorType>("a", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("b", new GeneratorType((PaperVariable)"a", (PaperVariable)"a")));


			var result = Solver.Solve(coroutines);

			Assert.Equal((ConcreteType)"Y", result.Yield);
			Assert.Equal(ConcreteType.Void, result.Receive);
		}


		[Fact]
		public void PopReceive()
		{
			List<KeyValuePair<string, GeneratorType>> coroutines = new List<KeyValuePair<string, GeneratorType>>();
			coroutines.Add(new KeyValuePair<string, GeneratorType>("a", new GeneratorType((ConcreteType)"A", ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("b", new GeneratorType(new SequenceType((ConcreteType)"B", (ConcreteType)"C"), (ConcreteType)"A")));

			var result = Solver.Solve(coroutines);

			Assert.Equal(new SequenceType((ConcreteType)"B", (ConcreteType)"C"), result.Yield);
			Assert.Equal(ConcreteType.Void, result.Receive);
		}


		[Fact]
		public void RunProlog()
		{
			List<KeyValuePair<string, GeneratorType>> coroutines = new List<KeyValuePair<string, GeneratorType>>();
			coroutines.Add(new KeyValuePair<string, GeneratorType>("child1", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Child", (ConcreteType)"John", (ConcreteType)"Sue")))));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("child2", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Child", (ConcreteType)"Jane", (ConcreteType)"Sue")))));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("child3", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Child", (ConcreteType)"Sue", (ConcreteType)"George")))));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("child4", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Child", (ConcreteType)"John", (ConcreteType)"Sam")))));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("child5", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Child", (ConcreteType)"Jane", (ConcreteType)"Sam")))));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("child6", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Child", (ConcreteType)"Sue", (ConcreteType)"Gina")))));

			coroutines.Add(new KeyValuePair<string, GeneratorType>("female1", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Female", (ConcreteType)"Sue")))));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("female2", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Female", (ConcreteType)"Jane")))));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("female3", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType)"Female", (ConcreteType)"June")))));

			coroutines.Add(
				new KeyValuePair<string, GeneratorType>("parent", new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"Child", (PaperVariable)"x", (PaperVariable)"y")), new SequenceType(new SequenceType((ConcreteType)"Parent", (PaperVariable)"y", (PaperVariable)"x")))));

			coroutines.Add(new KeyValuePair<string, GeneratorType>("query",
				new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new SequenceType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Yes"), (PaperVariable)"x")));
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


	}
}
