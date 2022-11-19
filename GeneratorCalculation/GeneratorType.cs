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
		List<PaperVariable> GetVariables(List<string> constants);
		bool GetYield(ref PaperType yielded, ref PaperType remaining);

		void ReplaceWithConstant(List<string> availableConstants, List<string> usedConstants);
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

		public List<PaperVariable> GetVariables(List<string> constants)
		{
			if (constants.Contains(Name))
				return new List<PaperVariable>();
			else
				return new List<PaperVariable>(new[] { this });
		}

		public bool GetYield(ref PaperType yielded, ref PaperType remaining)
		{
			return false;
		}

		public void ReplaceWithConstant(List<string> availableConstants, List<string> usedConstants)
		{ }
	}

	class PaperInt : PaperWord
	{
		public int Value { get; set; }

		public static implicit operator PaperInt(int n)
		{
			return new PaperInt { Value = n };
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	class PaperStar : PaperWord
	{
		public static readonly PaperStar Instance = new PaperStar();

		private PaperStar()
		{

		}

		public override string ToString()
		{
			return "*";
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

		public List<PaperVariable> GetVariables(List<string> constants)
		{
			return new List<PaperVariable>();
		}

		public bool GetYield(ref PaperType yielded, ref PaperType remaining)
		{
			if (this == Void)
				return false;

			yielded = this;
			remaining = ConcreteType.Void;
			return true;
		}

		public void ReplaceWithConstant(List<string> availableConstants, List<string> usedConstants)
		{ }
	}

	//class VariableType : PaperType
	//{
	//	public string Name { get; set; }
	//}

	public class GeneratorType : FunctionType
	{
		public GeneratorType(PaperType @yield, PaperType receive) : base("G", yield, receive)
		{
			Yield = yield;
			Receive = receive;
		}

		public PaperType Yield { get; set; }

		public PaperType Receive { get; set; }

		public void Check()
		{
			//Receive is like input, yield is like output.
			//Yield cannot have variables that are unbound from Receive.

			List<string> constants = new List<string>();
			var inputVariables = Receive.GetVariables(constants).Select(v => v.Name).ToList();
			var outputVariables = Yield.GetVariables(constants).Select(v => v.Name).ToList();

			if (outputVariables.Any(v => inputVariables.Contains(v) == false))
			{
				var culprits = outputVariables.Where(v => inputVariables.Contains(v) == false).ToList();
				throw new FormatException($"{string.Join(", ", culprits)} are not bound by receive.");
			}


			//Console.WriteLine("Yield variables: " + string.Join(", ", ));

			//Console.WriteLine("Receive variables: " + string.Join(", ", ));
		}

		public bool Next(List<string> constants, ref GeneratorType g, ref PaperType yieldedType)
		{
			if (Yield.GetVariables(constants).Count == 0)
			{
				PaperType remaining = null;
				if (Yield.GetYield(ref yieldedType, ref remaining))
				{
					Yield = remaining;
					g = this;
					return true;
				}
			}
			else
				return false;

			throw new NotImplementedException();
		}


		public override string ToString()
		{
			return $"G {Yield} {Receive}";
		}

		//public List<PaperVariable> GetVariables()
		//{
		//	var l = new List<PaperVariable>();
		//	l.AddRange(Yield.GetVariables());
		//	l.AddRange(Receive.GetVariables());
		//	return l;
		//}
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
			if (types.Length == 0)
				throw new ArgumentException();

			Types = new List<PaperType>(types);
		}

		public override string ToString()
		{
			return "(" + string.Join(", ", Types) + ")";
		}

		public List<PaperVariable> GetVariables(List<string> constants)
		{
			return Types.SelectMany(t => t.GetVariables(constants)).ToList();
		}

		public bool GetYield(ref PaperType yielded, ref PaperType remaining)
		{
			if (Types.Count == 1)
			{
				yielded = Types[0];
				remaining = ConcreteType.Void;
				return true;
			}
			else if (Types.Count == 0)
				throw new ArgumentException();
			else
			{
				yielded = Types[0];
				Types.RemoveAt(0);
				remaining = new SequenceType(Types.ToArray());
				return true;

			}
		}

		public void ReplaceWithConstant(List<string> availableConstants, List<string> usedConstants)
		{
			foreach (var t in Types)
			{
				t.ReplaceWithConstant(availableConstants, usedConstants);
			}

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

		public List<PaperVariable> GetVariables(List<string> constants)
		{
			var r = from a in Arguments
					where a is PaperType
					from v in ((PaperType)a).GetVariables(constants)
					select v;

			return r.ToList();

		}

		public bool GetYield(ref PaperType yielded, ref PaperType remaining)
		{
			return false;
		}

		public void ReplaceWithConstant(List<string> availableConstants, List<string> usedConstants)
		{
			foreach (var w in Arguments)
			{
				if (w is PaperType word)
					word.ReplaceWithConstant(availableConstants, usedConstants);
			}
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

		public List<PaperVariable> GetVariables(List<string> constants)
		{
			var l = Type.GetVariables(constants);
			if (Size is PaperType)
				l.AddRange(((PaperType)Size).GetVariables(constants));

			return l;
		}

		public bool GetYield(ref PaperType yielded, ref PaperType remaining)
		{
			if (Size is PaperStar)
				throw new Exception("Star should have been replaced in the ReplaceWithConstant step.");

			yielded = this;
			remaining = ConcreteType.Void;
			return true;
		}

		public void ReplaceWithConstant(List<string> availableConstants, List<string> usedConstants)
		{
			if (Size is PaperStar)
			{
				string c = availableConstants[0];
				availableConstants.RemoveAt(0);
				usedConstants.Add(c);

				Size = new PaperVariable(c);
			}
		}
	}
}
