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

	}
}
