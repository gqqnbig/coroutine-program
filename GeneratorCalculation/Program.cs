using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{
		static void Main(string[] args)
		{
			List<KeyValuePair<string, GeneratorType>> coroutines = new List<KeyValuePair<string, GeneratorType>>();
			coroutines.Add(new KeyValuePair<string, GeneratorType>("oc1", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("oc2", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("fr1", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), (ConcreteType)"Y")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("fr2", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), (ConcreteType)"Y")));


			GeneratorType interleave = new GeneratorType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")),
				new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("interleave", interleave));

			var result= Solver.Solve(coroutines);

			Console.WriteLine("Final result:");
			Console.WriteLine(result);
		}
	}
}
