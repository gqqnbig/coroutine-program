using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{

		static void Main(string[] args)
		{

			Dictionary<PaperVariable, PaperWord> bindings = new Dictionary<PaperVariable, PaperWord>();
			bindings.Add("openStore", new CoroutineType(new SequenceType("Store"), new SequenceType("Store", "CurrentStore")));
			bindings.Add("openCashDesk", new CoroutineType(new SequenceType("CashDesk", "CurrentStore"), new SequenceType("CashDesk", "CurrentStore", "CurrentCashDesk")));
			bindings.Add("makeNewSale", new CoroutineType(new SequenceType("CurrentCashDesk"), new SequenceType("CurrentCashDesk", "Sale", "CurrentSale")));
			bindings.Add("enterItem", new CoroutineType(new SequenceType("CurrentSale", "Item"), new SequenceType("CurrentSale", "Item", "SalesLineItem", "CurrentSaleLine")));
			bindings.Add("createStore", new CoroutineType(ConcreteType.Void, new SequenceType("Store")));
			bindings.Add("createCashDesk", new CoroutineType(ConcreteType.Void, new SequenceType("CashDesk")));
			bindings.Add("createItem", new CoroutineType(ConcreteType.Void, new SequenceType("Item")));

			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("", new CoroutineType(ConcreteType.Void, new TupleType((PaperVariable)"openStore", (PaperVariable)"openCashDesk", (PaperVariable)"makeNewSale", (PaperVariable)"enterItem", (PaperVariable)"createStore", (PaperVariable)"createCashDesk", (PaperVariable)"createItem"))));
			coroutines.Add(new Generator("deleteItem", new CoroutineType(new SequenceType("Item"), ConcreteType.Void)));


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
