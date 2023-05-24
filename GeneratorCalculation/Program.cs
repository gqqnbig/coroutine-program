using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{

		static void Main(string[] args)
		{
			var coroutines = new List<Generator>();
			coroutines.Add(new Generator("createItem", new GeneratorType((ConcreteType)"Item", ConcreteType.Void)));

			coroutines.Add(new Generator("createStore", new GeneratorType((ConcreteType)"Store", ConcreteType.Void)));
			coroutines.Add(new Generator("createCashDesk", new GeneratorType((ConcreteType)"CashDesk", ConcreteType.Void)));
			coroutines.Add(new Generator("openStore", new GeneratorType(new SequenceType((ConcreteType)"Store", (ConcreteType)"CurrentStore"), (ConcreteType)"Store")));
			coroutines.Add(new Generator("openCashDesk", new GeneratorType(new SequenceType((ConcreteType)"CashDesk", (ConcreteType)"CurrentStore", (ConcreteType)"CurrentCashDesk"), new SequenceType((ConcreteType)"CashDesk", (ConcreteType)"CurrentStore"))));

			coroutines.Add(new Generator("makeNewSale", new GeneratorType(new SequenceType((ConcreteType)"Sale", (ConcreteType)"CurrentSale", (ConcreteType)"CurrentCashDesk"), (ConcreteType)"CurrentCashDesk")));
			coroutines.Add(new Generator("enterItem", new GeneratorType(new SequenceType((ConcreteType)"SalesLineItem", (ConcreteType)"CurrentSale", (ConcreteType)"Item"),
				new SequenceType((ConcreteType)"Item", (ConcreteType)"CurrentSale"))));
			coroutines.Add(new Generator("endSale", new GeneratorType((ConcreteType)"CurrentSale", (ConcreteType)"CurrentSale")));
			coroutines.Add(new Generator("makeCashPayment", new GeneratorType(new SequenceType((ConcreteType)"CurrentSale", (ConcreteType)"CashPayment"), (ConcreteType)"CurrentSale")));


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
