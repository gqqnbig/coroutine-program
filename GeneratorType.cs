using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation
{
	/// <summary>
	/// include int, variable, type.
	/// </summary>
	interface PaperWord { }

	interface PaperType : PaperWord { }


	class PaperVariable : PaperWord, PaperType
	{
		public PaperVariable(string name)
		{
			if (name.Length == 0 || char.IsLower(name[0]) == false)
				throw new ArgumentException("The first letter must be lowercase.", nameof(name));

			Name = name;
		}

		public string Name { get; set; }

		public static implicit operator PaperVariable(string n)
		{
			return new PaperVariable(n);
		}

		public override string ToString()
		{
			return Name;
		}
	}

	class PaperInt : PaperWord
	{
		public int Value { get; set; }

		public static implicit operator PaperInt(int n)
		{
			return new PaperInt { Value = n };
		}
	}

	class ConcreteType : PaperType
	{
		public static readonly ConcreteType Void = new ConcreteType("Void");

		public ConcreteType(string name)
		{
			if (name.Length == 0 || char.IsUpper(name[0]) == false)
				throw new ArgumentException("The first letter must be uppercase.", nameof(name));

			Name = name;
		}

		public string Name { get; }

		public static implicit operator ConcreteType(string n)
		{
			return new ConcreteType(n);
		}

		public override string ToString()
		{
			return Name;
		}
	}

	class VariableType : PaperType
	{
		public string Name { get; set; }
	}

	class GeneratorType : PaperType
	{
		public GeneratorType(PaperType @yield, PaperType receive)
		{
			Yield = yield;
			Receive = receive;
		}

		public PaperType Yield { get; set; }

		public PaperType Receive { get; set; }

		public bool Check()
		{
			//Receive is like input, yield is like output.
			//Yield cannot have variables that are unbound from Receive.
			throw new NotImplementedException();
		}


		public override string ToString()
		{
			return $"G {Yield} {Receive}";
		}
	}

	class OrType : PaperType
	{
		public List<PaperType> Types { get; } = new List<PaperType>();
	}

	class AndType : PaperType
	{
		public AndType(params PaperType[] types)
		{
			Types = new List<PaperType>(types);
		}

		public List<PaperType> Types { get; }

		public override string ToString()
		{
			return "(" + string.Join("&", Types) + ")";
		}
	}

	class SequenceType : PaperType
	{
		public List<PaperWord> Types { get; }

		public SequenceType(params PaperType[] types)
		{
			Types = new List<PaperWord>(types);
		}

		public override string ToString()
		{
			return "(" + string.Join(", ", Types) + ")";
		}
	}

	class FunctionType : PaperType
	{
		public FunctionType(string functionName, params PaperWord[] words)
		{
			FunctionName = functionName;

			Arguments = new List<PaperWord>(words);
		}

		public string FunctionName { get; set; }

		public List<PaperWord> Arguments { get; }

		public override string ToString()
		{
			return FunctionName + "(" + string.Join(", ", Arguments) + ")";
		}
	}

	class ListType : PaperType
	{
		public ListType(PaperType type, PaperWord size)
		{
			Type = type;
			Size = size;
		}

		public PaperType Type { get; set; }
		public PaperWord Size { get; set; }

		public override string ToString()
		{
			return Type.ToString() + Size.ToString();
		}
	}
}
