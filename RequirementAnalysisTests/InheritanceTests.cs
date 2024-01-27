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

			InheritanceCondition ic = new InheritanceCondition { Subclass = (PaperVariable)"a", Superclass = (ConcreteType)"Animal" };
			generators.Add(new Generator("", new CoroutineType(ic, (PaperVariable)"a", ConcreteType.Void)));

			generators.Add(new Generator("", new CoroutineType(ConcreteType.Void, (ConcreteType)"Apple")));
			generators.Add(new Generator("", new CoroutineType(ConcreteType.Void, (ConcreteType)"Dog")));
			generators.Add(new Generator("", new CoroutineType((ConcreteType)"B", (ConcreteType)"A")));
			generators.Add(new Generator("", new CoroutineType((ConcreteType)"A", (ConcreteType)"B")));

			try
			{
				var result = new Solver().SolveWithBindings(generators);
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
