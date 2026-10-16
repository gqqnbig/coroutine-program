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

			var falseG = new CoroutineInstanceType(
				receive: new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"y")),
				yield: ConcreteType.Void);
			var trueG = new CoroutineInstanceType(new SequenceType(new TupleType((PaperVariable)"x", (PaperVariable)"x")), terminatorTrue);
			var rec = new CoroutineInstanceType(
				receive: new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n"))),
				yield: new SequenceType(trueG, falseG, new TupleType((PaperVariable)"x", (PaperVariable)"y"), new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n")))));

			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("recursion1", true, rec.Clone()));
			coroutines.Add(new Generator("recursion2", true, rec.Clone()));
			coroutines.Add(new Generator("base", new CoroutineInstanceType(new SequenceType(new TupleType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0))), terminatorFalse)));

			return coroutines;
		}

		static void Main(string[] args)
		{
			var bindings = new Dictionary<PaperVariable, PaperWord>();
			bindings.Add("r1", new CoroutineInstanceType((ConcreteType)"A", (ConcreteType)"B"));
			bindings.Add("r2", new CoroutineInstanceType((ConcreteType)"B", (ConcreteType)"D"));

			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, new TupleType((PaperVariable)"r1", (PaperVariable)"r2"))));
			coroutines.Add(new Generator("l", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"A")));


			var result = new Solver().SolveWithBindings(coroutines, bindings);

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

		public CoroutineInstanceType OriginalType { get; }
		public CoroutineInstanceType Type { get; set; }

		public Generator(string name, CoroutineInstanceType type) : this(name, false, type)
		{ }


		public Generator(string name, bool isInfinite, CoroutineInstanceType type)
		{
			Name = name;
			IsInfinite = isInfinite;
			Type = type;
			OriginalType = type.Clone();
		}

	}
}
