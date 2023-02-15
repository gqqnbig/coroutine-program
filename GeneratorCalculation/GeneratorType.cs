using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Z3 = Microsoft.Z3;

namespace GeneratorCalculation
{

	public class CoroutineType : PaperType
	{
		public Condition Condition { get; }

		public PaperType Yield { get; }

		public PaperType Receive { get; }

		public PaperVariable Source { get; }

		public bool CanRestore { get; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="receive"></param>
		/// <param name="yield"></param>
		/// <param name="source">This parameter is for information purpose. Only when canRestore is true, the solver then looks up the source in the bindings.</param>
		/// <param name="canRestore"></param>
		public CoroutineType(PaperType receive, PaperType yield, PaperVariable source = null, bool canRestore = false)
		{
			Receive = receive;
			Yield = yield;
			Source = source;
			CanRestore = canRestore;
		}

		public CoroutineType(Condition condition, PaperType receive, PaperType yield, PaperVariable source = null, bool canRestore = false)
		{
			Condition = condition;
			Receive = receive;
			Yield = yield;
			Source = source;
			CanRestore = canRestore;
		}


		public void Check()
		{
			//Receive is like input, yield is like output.
			//Yield cannot have variables that are unbound from Receive.

			List<string> constants = new List<string>();
			var inputVariables = Receive.GetVariables().Select(v => v.Name).ToList();
			var outputVariables = Yield.GetVariables().Select(v => v.Name).ToList();

			if (outputVariables.Any(v => inputVariables.Contains(v) == false))
			{
				var culprits = outputVariables.Where(v => inputVariables.Contains(v) == false).ToList();
				throw new FormatException($"{string.Join(", ", culprits)} are not bound by receive.");
			}


			//Console.WriteLine("Yield variables: " + string.Join(", ", ));

			//Console.WriteLine("Receive variables: " + string.Join(", ", ));
		}

		/// <summary>
		/// If it can yield, return the new type. Otherwise return null.
		/// </summary>
		/// <param name="constants"></param>
		/// <param name="g"></param>
		/// <param name="yieldedType"></param>
		/// <returns></returns>
		public virtual CoroutineType RunYield(Dictionary<PaperVariable, PaperWord> bindings, ref PaperType yieldedType)
		{
			if (Receive != ConcreteType.Void)
				return null;


			if (Yield.GetVariables().Except(bindings.Keys.ToList()).Any() == false)
			{
				PaperType remaining = null;
				if (Yield.Pop(ref yieldedType, ref remaining))
				{
					yieldedType = (PaperType)yieldedType.ApplyEquation(bindings.ToList());
					//Forbidden bindings are not needed when the coroutine starts to yield
					//because all variables have been bound.
					return new CoroutineType(Receive, remaining, Source, CanRestore);
				}
			}
			else
				Console.WriteLine("Unable to yield due to unbound variables");


			return null;
		}

		/// <summary>
		/// Check whether this generator can receive the given type.
		/// </summary>
		/// <param name="providedType"></param>
		/// <param name="conditions"></param>
		/// <returns></returns>
		public virtual Z3.BoolExpr RunReceive(PaperType providedType, Solver engine, out CoroutineType newGenerator)
		{
			newGenerator = null;
			//Dictionary<PaperVariable, PaperWord> conditions = new Dictionary<PaperVariable, PaperWord>();
			if (Receive == ConcreteType.Void)
				return engine.ConcreteSort.Context.MkFalse();

			PaperType head = null;
			PaperType remaining = null;
			if (Receive.Pop(ref head, ref remaining))
			{
				newGenerator = new CoroutineType(Condition, remaining, Yield, Source, CanRestore);
				var exp1 = AddConstraints(engine);
				if (exp1 == null)
					return head.BuildEquality(providedType, engine);
				else
					return engine.ConcreteSort.Context.MkAnd(head.BuildEquality(providedType, engine), exp1);
			}

			Debug.Assert(Receive == ConcreteType.Void);

			return engine.ConcreteSort.Context.MkFalse();
		}

		/// <summary>
		/// Return null if there's no additional conditions.
		/// </summary>
		/// <param name="engine"></param>
		/// <returns></returns>
		protected Z3.BoolExpr AddConstraints(Solver engine)
		{
			if (Condition != null)
				return this.Condition.GetExpr(engine);
			else
				return null;

			//Z3.BoolExpr[] args = new Z3.BoolExpr[ForbiddenBindings.Count];
			//int j = 0;
			//foreach (SequenceType key in ForbiddenBindings.Keys)
			//{
			//	if (key.Types.Count > 1)
			//		throw new NotImplementedException();

			//	var k = key.Types[0];
			//	var forbiddenSet = ForbiddenBindings[key];

			//	Z3.BoolExpr[] args1 = new Z3.BoolExpr[forbiddenSet.Count];
			//	for (int i = 0; i < forbiddenSet.Count; i++)
			//	{
			//		SequenceType fb = forbiddenSet[i];
			//		if (fb.Types.Count > 1)
			//			throw new NotImplementedException();

			//		var v = fb.Types[0];

			//		args1[i] = ctx.MkNot(k.BuildEquality(v, engine));
			//	}
			//	args[j] = ctx.MkAnd(args1);
			//}
			//return ctx.MkAnd(args);
		}


		public override string ToString()
		{
			string str;
			if (Condition != null)
				str = $"[{Receive}; {Yield}] where {Condition}";
			else
				str = $"[{Receive}; {Yield}]";

			if (Source != null)
				return Source.ToString() + (CanRestore ? "*" : "") + ": " + str;
			else
				return str;
		}


		public List<PaperVariable> GetVariables()
		{
			var inputVariables = Receive.GetVariables().ToList();
			var outputVariables = Yield.GetVariables().ToList();
			return inputVariables.Concat(outputVariables).ToList();
		}

		public Z3.BoolExpr BuildEquality(PaperWord other, Solver engine)
		{
			if (other is CoroutineType another)
			{
				// TODO: should use full match. IsCompatibleTo only checks the head element.
				return engine.ConcreteSort.Context.MkAnd(
					Yield.BuildEquality(another.Yield, engine),
					Receive.BuildEquality(another.Receive, engine));
			}

			return engine.ConcreteSort.Context.MkFalse();

		}

		PaperWord PaperWord.ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			return ApplyEquation(equations);
		}

		/// <summary>
		/// Never returns null
		/// </summary>
		/// <param name="equations"></param>
		/// <returns></returns>
		public CoroutineType ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			var newYield = Yield.ApplyEquation(equations);
			var newReceive = Receive.ApplyEquation(equations);
			if (newYield is PaperType newYieldType && newReceive is PaperType newReceiveType)
			{
				return new CoroutineType(Condition, newReceiveType, newYieldType);
				//var copy = new Dictionary<SequenceType, List<SequenceType>>();
				//foreach (SequenceType key in ForbiddenBindings.Keys)
				//{
				//	SequenceType valuedKey = (SequenceType)key.ApplyEquation(equations);
				//	var valuedSet = ForbiddenBindings[key].Select(s => (SequenceType)s.ApplyEquation(equations)).ToList();
				//	if (valuedSet.Any(s => s.Equals(valuedKey)))
				//		return new GeneratorType(ConcreteType.Void, ConcreteType.Void); // This identity element will be nuked.

				//	var c = new List<string>();
				//	if (valuedKey.GetVariables().Count == 0 && valuedSet.Sum(s => s.GetVariables().Count) == 0)
				//		continue; //Since both sides have no variables, we don't have to add them to ForbiddenBindings.

				//	if (copy.ContainsKey(valuedKey))
				//		copy[valuedKey].AddRange(valuedSet);
				//	else
				//		copy[valuedKey] = valuedSet;
				//}

				//return new GeneratorType(copy, newReceiveType, newYieldType);
			}
			else
				return this;
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			return false;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{
			Yield.ReplaceWithConstant(availableConstants, usedConstants);
			Receive.ReplaceWithConstant(availableConstants, usedConstants);
		}

		/// <summary>
		/// Either Void or GeneratorType in which all Avoid types are removed.
		/// </summary>
		/// <returns></returns>
		public virtual PaperType Normalize()
		{
			CoroutineType g;
			if (Condition != null)
				g = new CoroutineType(Condition, Receive.Normalize(), Yield.Normalize());
			else
				g = new CoroutineType(Receive.Normalize(), Yield.Normalize());

			if (g.Yield == ConcreteType.Void && g.Receive == ConcreteType.Void)
				return ConcreteType.Void;
			else
				return g;
		}


		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj is CoroutineType objGenerator)
			{
				return Receive.Equals(objGenerator.Receive) && Yield.Equals(objGenerator.Yield);
			}

			return false;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return Receive.GetHashCode() ^ Yield.GetHashCode();
		}

		public virtual CoroutineType Clone()
		{
			return new CoroutineType(Condition, Receive, Yield, Source, CanRestore);
		}
	}


	//public class LabeledCoroutineType : GeneratorType
	//{
	//	public string Name { get; }
	//	public bool IsInfinite { get; }

	//	public GeneratorType OriginalType { get; }

	//	public LabeledCoroutineType(PaperType receive, PaperType yield, string name = null, bool isInfinite = false) : base(yield, receive)
	//	{
	//		Name = name;
	//		IsInfinite = isInfinite;

	//		if (IsInfinite)
	//			OriginalType = new GeneratorType(receive, yield);

	//	}

	//	public LabeledCoroutineType(Dictionary<SequenceType, List<SequenceType>> forbiddenBindings, PaperType receive, PaperType yield, string name = null, bool isInfinite = false) : base(forbiddenBindings, receive, yield)
	//	{
	//		Name = name;
	//		IsInfinite = isInfinite;

	//		if (IsInfinite)
	//			OriginalType = new GeneratorType(receive, yield);
	//	}

	//	/// <summary>
	//	/// If it can yield, return the new type. Otherwise return null.
	//	/// </summary>
	//	/// <param name="constants"></param>
	//	/// <param name="g"></param>
	//	/// <param name="yieldedType"></param>
	//	/// <returns></returns>
	//	public override LabeledCoroutineType RunYield(List<string> constants, ref PaperType yieldedType)
	//	{
	//		if (Receive != ConcreteType.Void)
	//			return null;


	//		if (Yield.GetVariables(constants).Count == 0)
	//		{
	//			PaperType remaining = null;
	//			if (Yield.Pop(ref yieldedType, ref remaining))
	//				return new GeneratorType(remaining, Receive);

	//		}

	//		return null;
	//	}

	//	public override bool Equals(object obj)
	//	{
	//		if (obj is GeneratorType objGenerator)
	//		{
	//			return Receive.Equals(objGenerator.Receive) && Yield.Equals(objGenerator.Yield);
	//		}

	//		return false;
	//	}

	//	public override int GetHashCode()
	//	{
	//		return Name.GetHashCode();
	//	}

	//	public LabeledCoroutineType Clone()
	//	{
	//		return new LabeledCoroutineType(ForbiddenBindings, Receive, Yield, Name, IsInfinite);
	//	}
	//}


}
