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
			coroutines.Add(new Generator("oc1", new CoroutineType(ConcreteType.Void, (ConcreteType)"Y")));
			coroutines.Add(new Generator("oc2", new CoroutineType(ConcreteType.Void, (ConcreteType)"Y")));
			coroutines.Add(new Generator("fr1", new CoroutineType((ConcreteType)"Y", new ListType((ConcreteType)"S", PaperStar.Instance))));
			coroutines.Add(new Generator("fr2", new CoroutineType((ConcreteType)"Y", new ListType((ConcreteType)"S", PaperStar.Instance))));
			coroutines.Add(new Generator("zip", new CoroutineType(new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"m"), new ListType((PaperVariable)"y", (PaperVariable)"n")), new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"m", (PaperVariable)"n")))));

			var result = new Solver().SolveWithBindings(coroutines);

			Assert.Single(result.Flow);
			Assert.Contains("S, S", result.Flow[0].Type.ToString());
			Assert.Contains("min(a, b)", result.Flow[0].Type.ToString());
		}
	}
}
