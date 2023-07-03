using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace GeneratorCalculation
{

	/// <summary>
	/// include int, variable, type.
	/// </summary>
	public interface PaperWord
	{
		/// <summary>
		/// Check if this type equals to another type for receiving purpose.
		/// </summary>
		/// <param name="t"></param>
		/// <returns>Returns <c>null</c> if not compatible. Returns a dict of size 0 if it's always compatible.</returns>
		Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t);

		/// <summary>
		/// Even if the equations do not apply to this word, this method should return itself.
		/// </summary>
		/// <param name="equations">this parameter should not be modified</param>
		/// <returns></returns>
		/// <remarks>
		/// The main implementation resides in <see cref="PaperVariable"/>.
		/// </remarks>
		PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations);// { return this; }
	}

	public interface PaperType : PaperWord
	{
		List<PaperVariable> GetVariables();

		/// <summary>
		/// Pop the head element from the type.
		/// </summary>
		/// <param name="yielded"></param>
		/// <param name="remaining"></param>
		/// <returns></returns>
		bool Pop(ref PaperType yielded, ref PaperType remaining);

		/// <summary>
		/// For constants, PaperWord is null.
		/// </summary>
		/// <param name="availableConstants"></param>
		/// <param name="usedConstants"></param>
		void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants);


		PaperType Normalize();
	}


	public class PaperVariable : PaperWord, PaperType
	{
		public PaperVariable(string name)
		{
			if (name.Length == 0 || char.IsLower(name[0]) == false)
				throw new ArgumentException("The first letter must be lowercase.", nameof(name));

			Name = name;
		}

		public string Name { get; }

		public static implicit operator PaperVariable(string n)
		{
			return new PaperVariable(n);
		}

		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// It's textual equality, not syntactic equality.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return obj is PaperVariable objVariable && objVariable.Name == Name;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public List<PaperVariable> GetVariables()
		{
			return new List<PaperVariable>(new[] { this });
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			//We pop first, then make the head compatible to the yielded type.
			yielded = this;
			remaining = ConcreteType.Void;
			return true;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{ }

		public PaperType Normalize()
		{
			return this;
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			if (t is PaperType tType)
			{
				tType = tType.Normalize();

				if (tType is ConcreteType || tType is PaperVariable)
				{ }
				else
					return null;
			}

			var d = new Dictionary<PaperVariable, PaperWord>();
			d.Add(this, t);
			return d;
		}

		public PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			var keys = equations.Select(p => p.Key).ToList();
			if (keys.Count != keys.Distinct().Count())
				throw new NotSupportedException("Duplicate keys are not supported.");

			var eq = equations.FirstOrDefault(p => p.Key.Equals(this));
			if (eq.Key == null)
				return this;
			else if (eq.Value == null) // null denotes a constant.
				return this;
			else
				return eq.Value;
		}
	}

	public class PaperInt : PaperWord
	{
		public int Value { get; set; }

		public static implicit operator PaperInt(int n)
		{
			return new PaperInt { Value = n };
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			if (t is PaperInt tInt && tInt.Value == this.Value)
				return new Dictionary<PaperVariable, PaperWord>();

			return null;
		}

		public PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			return this;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj is PaperInt objInt)
				return Value == objInt.Value;
			return false;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}

	public class PaperStar : PaperWord
	{
		public static readonly PaperStar Instance = new PaperStar();

		private PaperStar()
		{

		}

		public override string ToString()
		{
			return "*";
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			return null;
		}

		public PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			throw new NotImplementedException();
		}
	}

	public class ConcreteType : PaperType
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

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			if (this == Void)
				return false;

			yielded = this;
			remaining = ConcreteType.Void;
			return true;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{ }

		public PaperType Normalize()
		{
			return this;
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			if (t is ConcreteType t2 && t2.Name == this.Name)
				return new Dictionary<PaperVariable, PaperWord>();

			if (t is ListType tList)
			{
				var condition1 = tList.Size.IsCompatibleTo((PaperInt)1);
				if (condition1 == null)
					return null;

				var condition2 = tList.Type.IsCompatibleTo(this);
				if (condition2 == null)
					return null;

				var c = Solver.JoinConditions(condition1, condition2);
				return c;
			}

			return null;
		}

		public PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			return this;
		}

		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj is ConcreteType objConcrete)
			{
				return Name.Equals(objConcrete.Name);
			}

			return false;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}

	public class SequenceType : PaperType
	{
		private static readonly ILogger logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(SequenceType));

		public List<PaperType> Types { get; }

		public SequenceType(params PaperType[] types) : this((IEnumerable<PaperType>)types)
		{
			if (types.Length == 0)
				throw new ArgumentException();
		}

		public SequenceType(params ConcreteType[] types) : this((IEnumerable<PaperType>)types)
		{
			if (types.Length == 0)
				throw new ArgumentException();
		}

		public SequenceType(IEnumerable<PaperType> types)
		{
			foreach (var t in types)
			{
				Debug.Assert(t is SequenceType == false, "A sequence can't nest another sequence " + t);
			}


			Types = new List<PaperType>(types);
		}

		public override string ToString()
		{
			return "<" + string.Join(", ", Types) + ">";
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			//if (Types.Count <= 1)
			//	throw new ArgumentException();

			if (t is SequenceType tSequence)
			{
				if (tSequence.Types.Count != Types.Count)
					return null;

				List<Dictionary<PaperVariable, PaperWord>> conditions = new List<Dictionary<PaperVariable, PaperWord>>(Types.Count);
				for (int i = 0; i < Types.Count; i++)
					conditions.Add(Types[i].IsCompatibleTo(tSequence.Types[i]));

				return Solver.JoinConditions(conditions);
			}
			return null;
		}

		public SequenceType ApplyEquation(Dictionary<PaperVariable, PaperWord> equations)
		{
			return (SequenceType)ApplyEquation(equations.ToList());
		}

		/// <summary>
		/// always return SequenceType.
		/// </summary>
		/// <param name="equations"></param>
		/// <returns></returns>
		public PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			PaperType[] newTypes = new PaperType[Types.Count];
			for (int i = 0; i < Types.Count; i++)
			{
				var t = Types[i].ApplyEquation(equations);
				if (t is PaperType tType)
					newTypes[i] = tType;
				else
				{
					logger.LogInformation($"Application is ignored because it produces an illegal type for {nameof(SequenceType)}.");
					newTypes[i] = Types[i];
				}
			}

			return new SequenceType(newTypes);
		}

		public List<PaperVariable> GetVariables()
		{
			return Types.SelectMany(t => t.GetVariables()).ToList();
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
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
				remaining = new SequenceType(Types.Skip(1).ToArray());
				return true;

			}
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{
			foreach (var t in Types)
			{
				t.ReplaceWithConstant(availableConstants, usedConstants);
			}

		}

		public PaperType Normalize()
		{
			var a = new SequenceType(from t in Types
									 let g = t.Normalize()
									 where t != ConcreteType.Void
									 select t);
			if (a.Types.Count == 0)
				return ConcreteType.Void;
			return a;
		}

		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj is SequenceType objSequence)
			{
				if (Types.Count != objSequence.Types.Count)
					return false;
				for (int i = 0; i < Types.Count; i++)
				{
					if (Types[i].Equals(objSequence.Types[i]) == false)
						return false;
				}

				return true;
			}

			return false;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			int hashcode = 0;
			for (int i = 0; i < Types.Count; i++)
			{
				hashcode ^= (i + Types[i].GetHashCode());
			}

			return hashcode;
		}
	}

	public class TupleType : PaperType
	{
		public List<PaperType> Types { get; }

		public TupleType(params PaperType[] types) : this((IEnumerable<PaperType>)types)
		{
			if (types.Length == 0)
				throw new ArgumentException();
		}

		public TupleType(IEnumerable<PaperType> types)
		{
			Types = new List<PaperType>(types);
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			if (t is TupleType tTuple)
			{
				if (Types.Count != tTuple.Types.Count)
					return null;


				var dict = new Dictionary<PaperVariable, PaperWord>();
				for (int i = 0; i < Types.Count; i++)
					dict = Solver.JoinConditions(dict, Types[i].IsCompatibleTo(tTuple.Types[i]));
				return dict;
			}
			return null;
		}

		public PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			PaperType[] newTypes = new PaperType[Types.Count];
			for (int i = 0; i < Types.Count; i++)
			{
				var t = Types[i].ApplyEquation(equations);
				if (t is PaperType tType)
					newTypes[i] = tType;
				else
				{
					Console.WriteLine($"Application is ignored because it produces an illegal type for {nameof(TupleType)}.");
					newTypes[i] = Types[i];
				}
			}

			return new TupleType(newTypes);
		}

		public List<PaperVariable> GetVariables()
		{
			var list = new List<PaperVariable>();
			foreach (var t in Types)
			{
				list.AddRange(t.GetVariables());
			}

			return list;
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			yielded = this;
			remaining = ConcreteType.Void;
			return true;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{
			foreach (var t in Types)
			{
				t.ReplaceWithConstant(availableConstants, usedConstants);
			}
		}

		public PaperType Normalize()
		{
			return this;
		}


		public override string ToString()
		{
			return "(" + string.Join(", ", Types) + ")";
		}
	}


	public class FunctionType : PaperType
	{
		public FunctionType(string functionName, params PaperWord[] words) : this(functionName, (IEnumerable<PaperWord>)words)
		{ }

		public FunctionType(string functionName, IEnumerable<PaperWord> words)
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

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			return null;
		}

		public virtual PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			return new FunctionType(FunctionName, Arguments.Select(a => a.ApplyEquation(equations))).Evaluate();
		}

		public List<PaperVariable> GetVariables()
		{
			var r = from a in Arguments
					where a is PaperType
					from v in ((PaperType)a).GetVariables()
					select v;

			return r.ToList();

		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			return false;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{
			foreach (var w in Arguments)
			{
				if (w is PaperType word)
					word.ReplaceWithConstant(availableConstants, usedConstants);
			}
		}

		public virtual PaperType Normalize()
		{
			return this;
		}

		public virtual PaperWord Evaluate()
		{
			return this;
		}

		public override bool Equals(object obj)
		{
			if (obj is FunctionType objFunction)
			{
				if (FunctionName.Equals(objFunction.FunctionName) == false)
					return false;

				if (Arguments.Count != objFunction.Arguments.Count)
					return false;

				for (int i = 0; i < Arguments.Count; i++)
				{
					if (Arguments[i].Equals(objFunction.Arguments[i]) == false)
						return false;
				}

				return true;
			}

			return false;
		}


		public override int GetHashCode()
		{
			int h = FunctionName.GetHashCode() + Arguments.Count * 11;

			for (int i = 0; i < Arguments.Count; i++)
				h += Arguments[i].GetHashCode() * i;
			return h;
		}
	}

	public class ListType : PaperType
	{
		public ListType(PaperType type, PaperWord size)
		{
			if (size == null)
				throw new ArgumentNullException(nameof(size));

			if (size is PaperInt sInt && sInt.Value < 0)
				throw new PaperSyntaxException("List size must be non-negative. Your size is " + size);


			Type = type;
			Size = size;
		}

		public PaperType Type { get; set; }
		public PaperWord Size { get; set; }

		public override string ToString()
		{
			return Type.ToString() + Size.ToString();
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord other)
		{
			if (other is ListType otherList)
			{
				var c1 = Type.IsCompatibleTo(otherList.Type);
				var c2 = Size.IsCompatibleTo(otherList.Size);
				return Solver.JoinConditions(c1, c2);
			}
			return null;
		}

		public PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			var newType = Type.ApplyEquation(equations);
			if (newType is PaperType newTypeType)
				return new ListType(newTypeType, Size.ApplyEquation(equations));
			else
				return this;
		}

		public List<PaperVariable> GetVariables()
		{
			var l = Type.GetVariables();
			if (Size is PaperType)
				l.AddRange(((PaperType)Size).GetVariables());

			return l;
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			if (Size is PaperStar)
				throw new Exception("Star should have been replaced in the ReplaceWithConstant step.");

			yielded = this;
			remaining = ConcreteType.Void;
			return true;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{
			if (Size is PaperStar)
			{
				string c = availableConstants[0];
				availableConstants.RemoveAt(0);
				usedConstants.Add(c, null);

				Size = new PaperVariable(c);
			}
		}

		public PaperType Normalize()
		{

			if (Size is PaperInt i && i.Value == 0)
				return ConcreteType.Void;

			var t = Type.Normalize();
			if (t == ConcreteType.Void)
				return ConcreteType.Void;

			else if (Size is FunctionType sFunction)
				return new ListType(t, sFunction.Evaluate());
			else
				return new ListType(t, Size);
		}

		public override bool Equals(object obj)
		{
			if (obj is ListType objList)
			{
				return Type.Equals(objList.Type) && Size.Equals(objList.Size);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode() ^ (Size.GetHashCode() << 2);
		}
	}

	public class PaperSyntaxException : Exception
	{
		public PaperSyntaxException(string message) : base(message)
		{

		}
	}
}
