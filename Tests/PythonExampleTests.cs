using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;
using Xunit;

namespace GeneratorCalculation.Tests
{
	public class PythonExampleTests
	{
		[Fact]
		public void Run()
		{

			List<Generator> coroutines = new List<Generator>();
			coroutines.Add(new Generator("oc1", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new Generator("oc2", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new Generator("fr1", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), (ConcreteType)"Y")));
			coroutines.Add(new Generator("fr2", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), (ConcreteType)"Y")));
			coroutines.Add(new Generator("zip", new GeneratorType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"m", (PaperVariable)"n")), new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"m"), new ListType((PaperVariable)"y", (PaperVariable)"n")))));

			var result = Solver.Solve(coroutines);

			Assert.Single(result.Flow);
			Assert.Contains("S, S", result.Flow[0].Type.ToString());
			Assert.Contains("min(a, b)", result.Flow[0].Type.ToString());
		}
	}
}
