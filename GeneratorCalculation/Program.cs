using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{

		private static List<Generator> GetRules()
		{
			var terminatorFalse = new ListType((ConcreteType)"F", PaperStar.Instance);

			var rec = new GeneratorType(
				new Dictionary<SequenceType, List<SequenceType>>
				{
					//Use a "D"ummy type because (PaperInt)0 is not a PaperType.
					[new SequenceType(new ListType((ConcreteType)"D", (PaperVariable)"n"))] = new List<SequenceType> { new SequenceType(new ListType((ConcreteType)"D", (PaperInt)0)) }
				},
				receive: new SequenceType(new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperVariable)"n"))),
				yield: new SequenceType((PaperVariable)"falseG", (PaperVariable)"trueG", new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", new DecFunction((PaperVariable)"n")))));

			List<Generator> coroutines = new List<Generator>();

			coroutines.Add(new Generator("base", new GeneratorType(terminatorFalse, new SequenceType(new SequenceType((PaperVariable)"x", new ListType((PaperVariable)"y", (PaperInt)0))))));
			coroutines.Add(new Generator("recursion1", true, rec.Clone()));
			coroutines.Add(new Generator("recursion2", true, rec.Clone()));

			return coroutines;
		}

		static void Main(string[] args)
		{
			var bindings = new Dictionary<PaperVariable, PaperWord>();
			bindings.Add("openStore", new CoroutineType(new SequenceType("Store"), new SequenceType("Store", "CurrentStore"), "openStore"));
			bindings.Add("openCashDesk", new CoroutineType(new SequenceType("CashDesk", "CurrentStore"), new SequenceType("CashDesk", "CurrentStore", "CurrentCashDesk"), "openCashDesk"));
			bindings.Add("makeNewSale", new CoroutineType(new SequenceType("CurrentCashDesk"), new SequenceType("CurrentCashDesk", "Sale", "CurrentSale"), "makeNewSale"));
			bindings.Add("enterItem", new CoroutineType(new SequenceType("CurrentSale", "Item"), new SequenceType("CurrentSale", "Item", "SalesLineItem", "CurrentSaleLine"), "enterItem"));
			bindings.Add("createStore", new CoroutineType(ConcreteType.Void, new SequenceType("Store"), "createStore"));
			bindings.Add("createCashDesk", new CoroutineType(ConcreteType.Void, new SequenceType("CashDesk"), "createCashDesk"));
			bindings.Add("createItem", new CoroutineType(ConcreteType.Void, new SequenceType("Item"), "createItem"));
			bindings.Add("makeCashPayment", new CoroutineType((ConcreteType)"CurrentSale", (ConcreteType)"CashPayment", "makeCashPayment"));


			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("", new GeneratorType(new TupleType((PaperVariable)"openStore", (PaperVariable)"openCashDesk", (PaperVariable)"makeNewSale", (PaperVariable)"enterItem", (PaperVariable)"createStore", (PaperVariable)"createCashDesk", (PaperVariable)"createItem", (PaperVariable)"makeCashPayment"), ConcreteType.Void)));
			coroutines.Add(new Generator("deleteItem", new CoroutineType(new SequenceType("Item"), ConcreteType.Void, "deleteItem")));


			var result = new Solver().SolveWithBindings(coroutines, bindings);

			Console.WriteLine("Result: " + result);

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
