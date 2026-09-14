using Xunit;
using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation.Tests
{
	public class GeneratorTypeTests
	{
		[Fact()]
		public void CheckTest()
		{
			GeneratorType g = new GeneratorType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"z"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")),
				new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")));


			Assert.Throws<FormatException>(() => g.Check());
		}

		[Fact]
		public void TestForbiddenBindings()
		{
			Solver solver = new Solver();
			// Use reflection to access and populate the concreteSort field.
			// I'm not yet certain of the signature of Solver.RunReceive().
			var z3CtxField = solver.GetType().GetField("z3Ctx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var ctx = (Microsoft.Z3.Context)z3CtxField.GetValue(solver);
			var concreteSort = ctx.MkEnumSort("Concrete", "A", "B", "X");
			var concreteSortField = solver.GetType().GetField("concreteSort", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			concreteSortField.SetValue(solver, concreteSort);


			var forbiddenBindings = new Dictionary<SequenceType, List<SequenceType>>();
			forbiddenBindings[new SequenceType((PaperVariable)"b")] = new List<SequenceType> { new SequenceType((ConcreteType)"B") };
			GeneratorType g = new GeneratorType(forbiddenBindings, new SequenceType((PaperVariable)"a", (PaperVariable)"b"), (ConcreteType)"X");

			GeneratorType ng;
			var conditions = g.RunReceive((ConcreteType)"A", solver, out ng);
			Assert.True(conditions != null, "The coroutine should have no problem in receiving A.");

			Assert.Equal(forbiddenBindings, ng.ForbiddenBindings);
		}

		[Fact]
		public void TestConstructor()
		{
			var g = new CoroutineType(condition: new InheritanceCondition(),
				receive: (ConcreteType)"A",
				yield: (ConcreteType)"B");

			Assert.Equal((ConcreteType)"A", g.Receive);
			Assert.Equal((ConcreteType)"B", g.Yield);
		}
	}

}