using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{

		private static List<Generator> GetRules()
		{
			var terminatorTrue = new ListType((ConcreteType)"T", PaperStar.Instance);
			var terminatorFalse = new ListType((ConcreteType)"F", PaperStar.Instance);

			var falseG = new GeneratorType(
				receive: new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"y")),
				yield: ConcreteType.Void);
			var trueG = new GeneratorType(terminatorTrue, new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"x")));
			var rec = new GeneratorType(
				receive: new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n"))),
				yield: new SequenceType(trueG, falseG, new TupleType((PaperVariable)"x", (PaperVariable)"y"), new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n")))));

			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("recursion1", true, rec.Clone()));
			coroutines.Add(new Generator("recursion2", true, rec.Clone()));
			coroutines.Add(new Generator("base", new GeneratorType(terminatorFalse, new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0))))));

			return coroutines;
		}

		static void Main(string[] args)
		{


			List<Generator> coroutines = GetRules();
			coroutines.Add(new Generator("starter", new GeneratorType(new SequenceType(new TupleType((ConcreteType)"Path", new ListType((ConcreteType)"String", (PaperInt)3))), ConcreteType.Void)));

			var result = new Solver().SolveWithBindings(coroutines);

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
