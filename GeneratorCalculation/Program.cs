using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{


		static void Main(string[] args)
		{
			List<Generator> coroutines = new List<Generator>();
			var truePart = new GeneratorType((ConcreteType)"Int", (ConcreteType)"T");
			var falsePart = new GeneratorType(ConcreteType.Void, (PaperVariable)"a");


			coroutines.Add(new Generator("if", new GeneratorType(new SequenceType(falsePart, truePart, (PaperVariable)"x"), (ConcreteType)"Int")));
			coroutines.Add(new Generator("starter", new GeneratorType((ConcreteType)"Int", ConcreteType.Void)));


			var result = Solver.Solve(coroutines);
			Console.WriteLine(result);
		}


	}

	/// <summary>
	/// labeled generator
	/// </summary>
	public class Generator
	{
		public string Name { get; }
		public bool IsInfinite { get; }

		public GeneratorType OriginalType { get; }
		public GeneratorType Type { get; set; }

		public Generator(string name, GeneratorType type) : this(name, false, type)
		{ }


		public Generator(string name, bool isInfinite, GeneratorType type)
		{
			Name = name;
			IsInfinite = isInfinite;
			Type = type;
			OriginalType = type.Clone();
		}

	}
}
