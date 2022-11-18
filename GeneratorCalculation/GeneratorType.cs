using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GeneratorCalculation
{
	/// <summary>
	/// include int, variable, type.
	/// </summary>
	public interface PaperWord { }

	public interface PaperType : PaperWord
	{
		List<PaperVariable> GetVariables();
	}


	public class PaperVariable : PaperWord, PaperType
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

		public List<PaperVariable> GetVariables()
		{
			return new List<PaperVariable>(new[] { this });
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

		public List<PaperVariable> GetVariables()
		{
			return new List<PaperVariable>();
		}
	}

	//class VariableType : PaperType
	//{
	//	public string Name { get; set; }
	//}

	public class GeneratorType : PaperType
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

			var inputVariables = Receive.GetVariables().Select(v => v.Name).ToList();
			var outputVariables = Yield.GetVariables().Select(v => v.Name).ToList();

			if (outputVariables.Any(v => inputVariables.Contains(v) == false))
			{
				var culprits = outputVariables.Where(v => inputVariables.Contains(v) == false).ToList();
				throw new FormatException($"{string.Join(", ", culprits)} are not bound by receive.");
			}


			//Console.WriteLine("Yield variables: " + string.Join(", ", ));

			//Console.WriteLine("Receive variables: " + string.Join(", ", ));
			return true;


			throw new NotImplementedException();
		}


		public override string ToString()
		{
			return $"G {Yield} {Receive}";
		}

		public List<PaperVariable> GetVariables()
		{
			var l = new List<PaperVariable>();
			l.AddRange(Yield.GetVariables());
			l.AddRange(Receive.GetVariables());
			return l;
		}
	}

	//class OrType : PaperType
	//{
	//	public List<PaperType> Types { get; } = new List<PaperType>();
	//}

	//class AndType : PaperType
	//{
	//	public AndType(params PaperType[] types)
	//	{
	//		Types = new List<PaperType>(types);
	//	}

	//	public List<PaperType> Types { get; }

	//	public override string ToString()
	//	{
	//		return "(" + string.Join("&", Types) + ")";
	//	}
	//}

	public class SequenceType : PaperType
	{
		public List<PaperType> Types { get; }

		public SequenceType(params PaperType[] types)
		{
			Types = new List<PaperType>(types);
		}

		public override string ToString()
		{
			return "(" + string.Join(", ", Types) + ")";
		}

		public List<PaperVariable> GetVariables()
		{
			return Types.SelectMany(t => t.GetVariables()).ToList();
		}
	}

	public class FunctionType : PaperType
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

		public List<PaperVariable> GetVariables()
		{
			var r = from a in Arguments
					where a is PaperType
					from v in ((PaperType)a).GetVariables()
					select v;

			return r.ToList();

		}
	}

	public class ListType : PaperType
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

		public List<PaperVariable> GetVariables()
		{
			var l = Type.GetVariables();
			if (Size is PaperType)
				l.AddRange(((PaperType)Size).GetVariables());

			return l;
		}
	}
}
