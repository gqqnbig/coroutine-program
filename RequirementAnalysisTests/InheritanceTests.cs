using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RequirementAnalysis.Tests
{

	public class InheritanceTests
	{
		[Fact]
		public void TestCondition()
		{
			List<Generator> generators = new List<Generator>();

			InheritanceCondition ic = new InheritanceCondition((PaperVariable)"a", (ConcreteType)"Animal");
			generators.Add(new Generator("", new CoroutineInstanceType(ic, (PaperVariable)"a", ConcreteType.Void)));

			generators.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Apple")));
			generators.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Dog")));
			generators.Add(new Generator("", new CoroutineInstanceType((ConcreteType)"B", (ConcreteType)"A")));
			generators.Add(new Generator("", new CoroutineInstanceType((ConcreteType)"A", (ConcreteType)"B")));

			try
			{
				var solver = new Solver();
				var inheritance = new Dictionary<string, string>();
				inheritance.Add("Dog", "Animal");
				solver.CollectConcreteTypes(generators, null);
				InheritanceCondition.BuildFunction(solver, inheritance, out var func, out var funcBody);
				solver.AddZ3Function(func, funcBody);

				var result = solver.SolveWithBindings(generators);
				Assert.True(false, "DeadLockException is expected.");
			}
			catch (DeadLockException e)
			{
				Assert.True(e.YieldsToOutside.Count > 0,
							"Dog is Animal, so it will be received. \"Apple\" should be yielded to the outside.");
				Assert.Equal(e.YieldsToOutside[0], (ConcreteType)"Apple");
			}

		}

	}
}
