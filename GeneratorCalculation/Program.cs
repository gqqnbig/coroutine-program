using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{

		static void Main(string[] args)
		{
			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("openStore", new CoroutineType(new SequenceType("Store"), new SequenceType("Store", "CurrentStore"))));
			coroutines.Add(new Generator("openCashDesk", new CoroutineType(new SequenceType("CashDesk", "CurrentStore"), new SequenceType("CashDesk", "CurrentStore", "CurrentCashDesk"))));
			coroutines.Add(new Generator("makeNewSale", new CoroutineType(new SequenceType("CurrentCashDesk"), new SequenceType("CurrentCashDesk", "Sale", "CurrentSale"))));
			coroutines.Add(new Generator("enterItem", new CoroutineType(new SequenceType("CurrentSale","Item"), new SequenceType("CurrentSale", "Item", "SalesLineItem", "CurrentSaleLine"))));
			coroutines.Add(new Generator("createStore", new CoroutineType(ConcreteType.Void, new SequenceType("Store"))));
			coroutines.Add(new Generator("createCashDesk", new CoroutineType(ConcreteType.Void, new SequenceType("CashDesk"))));
			coroutines.Add(new Generator("createItem", new CoroutineType(ConcreteType.Void, new SequenceType("Item"))));
			

			var result = new Solver().Solve(coroutines);
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
