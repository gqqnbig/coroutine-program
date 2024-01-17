using Xunit;
using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation.Tests
{
	public class GeneratorTypeTests
	{
		//[Fact()]
		//public void CheckTest()
		//{
		//	CoroutineInstanceType g = new CoroutineInstanceType(new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")),
		//		new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"z"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")));


		//	Assert.Throws<FormatException>(() => g.Check());
		//}

		//[Fact]
		//public void TestForbiddenBindings()
		//{
		//	Solver solver = new Solver();
		//	// Use reflection to access and populate the concreteSort field.
		//	// I'm not yet certain of the signature of Solver.RunReceive().
		//	var ctx = solver.z3Ctx;
		//	var concreteSortProperty = solver.GetType().GetProperty("ConcreteSort");
		//	concreteSortProperty.SetValue(solver, ctx.MkEnumSort("Concrete", "A", "B", "X"));

		//	Condition condition = Condition.NotEqual("b", "B");
		//	CoroutineType g = new CoroutineType(condition, new SequenceType((PaperVariable)"a", (PaperVariable)"b"), (ConcreteType)"X");

		//	CoroutineType ng;
		//	var conditions = g.RunReceive((ConcreteType)"A", solver, out ng);
		//	Assert.True(conditions != null, "The coroutine should have no problem in receiving A.");

		//	Assert.Equal(condition, ng.Condition);
		//}

		[Fact]
		public void TestConstructor()
		{
			var c = new Z3Condition(s => s.z3Ctx.MkTrue());
			var g = new CoroutineInstanceType(condition: c,
				receive: (ConcreteType)"A",
				yield: (ConcreteType)"B");

			Assert.Equal(Direction.Resuming, g.Flow[0].Direction);
			Assert.Equal((ConcreteType)"A", g.Flow[0].Type);

			Assert.Equal(Direction.Yielding, g.Flow[1].Direction);
			Assert.Equal((ConcreteType)"B", g.Flow[1].Type);
		}
	}

}