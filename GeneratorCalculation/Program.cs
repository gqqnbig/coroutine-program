using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{

		static void Main(string[] args)
		{
			//var coroutines = new List<Generator>();
			//var openStore = new Generator("openStore", new CoroutineType(new SequenceType("Store"), new SequenceType("Store", "CurrentStore")));
			//var openCashDesk = new Generator("openCashDesk", new CoroutineType(new SequenceType("CashDesk", "CurrentStore"), new SequenceType("CashDesk", "CurrentStore", "CurrentCashDesk")));
			//var makeNewSale = new Generator("makeNewSale", new CoroutineType(new SequenceType("CurrentCashDesk"), new SequenceType("CurrentCashDesk", "Sale", "CurrentSale")));
			//var enterItem = new Generator("enterItem", new CoroutineType(new SequenceType("CurrentSale", "Item"), new SequenceType("CurrentSale", "Item", "SalesLineItem", "CurrentSaleLine")));
			//var createStore = new Generator("createStore", new CoroutineType(ConcreteType.Void, new SequenceType("Store")));
			//var createCashDesk = new Generator("createCashDesk", new CoroutineType(ConcreteType.Void, new SequenceType("CashDesk")));
			//var createItem = new Generator("createItem", new CoroutineType(ConcreteType.Void, new SequenceType("Item")));


			//coroutines.Add(new Generator("deleteItem", new CoroutineType(new SequenceType("Item"), ConcreteType.Void)));


			//var result = new Solver().Solve(coroutines);
			//Console.WriteLine(result);

			List<Generator> coroutines = new List<Generator>();
			coroutines.Add(new Generator("", new CoroutineType(ConcreteType.Void, (ConcreteType)"Y")));
			coroutines.Add(new Generator("", new CoroutineType((ConcreteType)"Y", (PaperVariable)"x")));

			Dictionary<PaperVariable, PaperWord> bindings = new Dictionary<PaperVariable, PaperWord>();
			bindings.Add("x", new CoroutineType((ConcreteType)"A", (ConcreteType)"B"));


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
