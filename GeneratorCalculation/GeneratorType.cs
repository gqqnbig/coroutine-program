using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Z3 = Microsoft.Z3;

namespace GeneratorCalculation
{

	public class CoroutineInstanceType : PaperType, IEquatable<CoroutineInstanceType>
	{
		/// <summary>
		/// can be null
		/// </summary>
		public Condition Condition { get; }

		public List<DataFlow> Flow { get; }

		//public PaperType Yield
		//{
		//	get
		//	{
		//		if (Flow[0].Direction == Direction.Yielding)
		//			return Flow[0].Type;
		//		else
		//			throw new NotSupportedException();
		//	}
		//}

		//public PaperType Receive
		//{
		//	get
		//	{
		//		if(flow
		//	}
		//}

		public PaperVariable Source { get; }

		public bool CanRestore { get; }

		private CoroutineInstanceType(PaperType receive, PaperType yield)
		{
			Flow = new List<DataFlow>();
			if (receive != ConcreteType.Void)
			{
				if (receive is SequenceType sReceive)
				{
					foreach (var item in sReceive.Types)
						Flow.Add(new DataFlow(Direction.Resuming, item));
				}
				else
					Flow.Add(new DataFlow(Direction.Resuming, receive));
			}
			if (yield != ConcreteType.Void)
			{
				if (yield is SequenceType sYield)
				{
					foreach (var item in sYield.Types)
						Flow.Add(new DataFlow(Direction.Yielding, item));
				}
				else
					Flow.Add(new DataFlow(Direction.Yielding, yield));
			}

		}

		public CoroutineInstanceType(params DataFlow[] flow)
		{
			Flow = new List<DataFlow>(flow);
		}

		public CoroutineInstanceType(IEnumerable<DataFlow> flow, PaperVariable source, bool canRestore)
		{
			Flow = new List<DataFlow>(flow);
			Source = source;
			CanRestore = canRestore;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="receive"></param>
		/// <param name="yield"></param>
		/// <param name="source">This parameter is for information purpose. Only when canRestore is true, the solver then looks up the source in the bindings.</param>
		/// <param name="canRestore"></param>
		public CoroutineInstanceType(PaperType receive, PaperType yield, PaperVariable source = null, bool canRestore = false) : this(receive, yield)
		{

			Source = source;
			CanRestore = canRestore;
		}

		public CoroutineInstanceType(Condition condition, IEnumerable<DataFlow> flow, PaperVariable source, bool canRestore)
		{
			Condition = condition;
			Flow = new List<DataFlow>(flow);
			Source = source;
			CanRestore = canRestore;
		}


		public CoroutineInstanceType(Condition condition, PaperType receive, PaperType yield, PaperVariable source = null, bool canRestore = false) : this(receive, yield)
		{
			Condition = condition;

			Source = source;
			CanRestore = canRestore;
		}


		public void Check()
		{
			//Receive is like input, yield is like output.
			//Yield cannot have variables that are unbound from Receive.

			//List<string> constants = new List<string>();
			//var inputVariables = Receive.GetVariables().Select(v => v.Name).ToList();
			//var outputVariables = Yield.GetVariables().Select(v => v.Name).ToList();

			//if (outputVariables.Any(v => inputVariables.Contains(v) == false))
			//{
			//	var culprits = outputVariables.Where(v => inputVariables.Contains(v) == false).ToList();
			//	throw new FormatException($"{string.Join(", ", culprits)} are not bound by receive.");
			//}


			//Console.WriteLine("Yield variables: " + string.Join(", ", ));

			//Console.WriteLine("Receive variables: " + string.Join(", ", ));
		}

		///// <summary>
		///// If it can yield, return the new type. Otherwise return null.
		///// </summary>
		///// <param name="constants"></param>
		///// <param name="g"></param>
		///// <param name="yieldedType"></param>
		///// <returns></returns>
		//public virtual CoroutineType RunYield(Dictionary<PaperVariable, PaperWord> bindings, ref PaperType yieldedType)
		//{
		//	if (Receive != ConcreteType.Void)
		//		return null;


		//	if (Yield.GetVariables().Except(bindings.Keys.ToList()).Any() == false)
		//	{
		//		PaperType remaining = null;
		//		if (Yield.Pop(ref yieldedType, ref remaining))
		//		{
		//			yieldedType = (PaperType)yieldedType.ApplyEquation(bindings);
		//			//Forbidden bindings are not needed when the coroutine starts to yield
		//			//because all variables have been bound.
		//			return new CoroutineType(Receive, remaining, Source, CanRestore);
		//		}
		//	}
		//	else
		//		Console.WriteLine("Unable to yield due to unbound variables");


		//	return null;
		//}

		///// <summary>
		///// Check whether this generator can receive the given type.
		///// </summary>
		///// <param name="providedType"></param>
		///// <param name="conditions"></param>
		///// <returns></returns>
		//public virtual Z3.BoolExpr RunReceive(PaperType providedType, Solver engine, out CoroutineType newGenerator)
		//{
		//	newGenerator = null;
		//	//Dictionary<PaperVariable, PaperWord> conditions = new Dictionary<PaperVariable, PaperWord>();
		//	if (Receive == ConcreteType.Void)
		//		return engine.ConcreteSort.Context.MkFalse();

		//	PaperType head = null;
		//	PaperType remaining = null;
		//	if (Receive.Pop(ref head, ref remaining))
		//	{
		//		newGenerator = new CoroutineType(Condition, remaining, Yield, Source, CanRestore);
		//		var exp1 = AddConstraints(engine);
		//		if (exp1 == null)
		//			return head.BuildEquality(providedType, engine);
		//		else
		//			return engine.ConcreteSort.Context.MkAnd(head.BuildEquality(providedType, engine), exp1);
		//	}

		//	Debug.Assert(Receive == ConcreteType.Void);

		//	return engine.ConcreteSort.Context.MkFalse();
		//}

		/// <summary>
		/// Return null if there's no additional conditions.
		/// </summary>
		/// <param name="engine"></param>
		/// <returns></returns>
		internal Z3.BoolExpr AddConstraints(Solver engine)
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
			StringBuilder sb = new StringBuilder();

			if (Source != null)
			{
				sb.Append(Source.ToString());
				if (CanRestore)
					sb.Append("*");

				sb.Append(": ");
			}

			sb.Append("[");
			// If any type is CoroutineType, put each flow item on its own line.
			// This is to improve readability.
			if (Flow.Any(f => f.Type is CoroutineInstanceType))
				sb.Append(string.Join("~~\n", Flow.Select(f => (f.Direction == Direction.Yielding ? "+" : "-") + f.Type)));
			else
				sb.Append(string.Join("~~", Flow.Select(f => (f.Direction == Direction.Yielding ? "+" : "-") + f.Type)));
			sb.Append("]");



			if (Condition != null)
				sb.Append(" where " + Condition);

			return sb.ToString();
		}


		public List<PaperVariable> GetVariables()
		{
			var vars = new List<PaperVariable>();
			foreach (var item in Flow)
			{
				vars.AddRange(item.Type.GetVariables());
			}
			return vars;
		}

		public Z3.BoolExpr BuildEquality(PaperWord other, Solver engine)
		{
			if (other is CoroutineInstanceType another && Flow.Count == another.Flow.Count)
			{
				Z3.BoolExpr[] exprs = new Z3.BoolExpr[Flow.Count];
				for (int i = 0; i < Flow.Count; i++)
				{
					exprs[i] = Flow[i].Type.BuildEquality(another.Flow[i].Type, engine);
				}

				return engine.ConcreteSort.Context.MkAnd(exprs);
			}

			return engine.ConcreteSort.Context.MkFalse();

		}

		PaperWord PaperWord.ApplyEquation(Dictionary<PaperVariable, PaperWord> equations)
		{
			return ApplyEquation(equations);
		}

		/// <summary>
		/// Never returns null
		/// </summary>
		/// <param name="equations"></param>
		/// <returns></returns>
		public CoroutineInstanceType ApplyEquation(Dictionary<PaperVariable, PaperWord> equations)
		{
			List<DataFlow> flow = new List<DataFlow>();
			foreach (var item in Flow)
			{
				var newType = item.Type.ApplyEquation(equations);
				if (newType is PaperType == false)
					throw new PaperSyntaxException($"{item} would be evaluated to {newType} which is incompatible with this position.");

				flow.Add(new DataFlow(item.Direction, (PaperType)newType));
			}
			return new CoroutineInstanceType(Condition, flow, Source, CanRestore);
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			return false;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{
			foreach (var item in Flow)
			{
				item.Type.ReplaceWithConstant(availableConstants, usedConstants);
			}
		}

		/// <summary>
		/// Either Void or GeneratorType in which all Avoid types are removed.
		/// </summary>
		/// <returns></returns>
		public virtual PaperType Normalize()
		{
			var flow = new List<DataFlow>();
			foreach (var item in Flow)
			{
				var t = item.Type.Normalize();
				if (t != ConcreteType.Void)
					flow.Add(new DataFlow(item.Direction, t));
			}

			if (flow.Count == 0)
				return ConcreteType.Void;
			else
				return new CoroutineInstanceType(Condition, flow, Source, CanRestore);
		}


		public bool Equals([AllowNull] CoroutineInstanceType other)
		{
			if (Flow.SequenceEqual(other.Flow) == false)
				return false;

			if (Source == null && other.Source != null)
				return false;
			else if (Source == null && other.Source == null)
			{ }
			else if (Source.Equals(other.Source) == false)
				return false;

			if (Condition == null && other.Condition != null)
				return false;
			else if (Condition == null && other.Condition == null)
			{ }
			else if (Condition.Equals(other.Condition) == false)
				return false;


			return CanRestore == other.CanRestore;
		}

		public override bool Equals(object obj)
		{
			if (obj is CoroutineInstanceType objGenerator)
				return Equals(objGenerator);
			return false;
		}

		public override int GetHashCode()
		{
			throw new NotSupportedException(nameof(CoroutineInstanceType) + " cannot be used as key in a hashtable or a dictionary.");
			//return 0;
		}


		public virtual CoroutineInstanceType Clone()
		{
			return new CoroutineInstanceType(Condition, Flow, Source, CanRestore);
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


	public class CoroutineDefinitionType : PaperType
	{
		public Condition Condition { get; }

		public PaperType Yield { get; }

		public PaperType Receive { get; }

		public Dictionary<SequenceType, List<SequenceType>> ForbiddenBindings { get; } = new Dictionary<SequenceType, List<SequenceType>>();



		public CoroutineDefinitionType(PaperType receive, PaperType @yield, Condition condition = null)
		{
			Receive = receive;
			Yield = yield;
			Condition = condition;
		}

		public CoroutineInstanceType Start()
		{
			return new CoroutineInstanceType(Receive, Yield);
		}

		public override string ToString()
		{
			return $"~>[{Receive}; {Yield}]";
		}

		// override object.Equals
		public override bool Equals(object obj)
		{
			CoroutineDefinitionType y = obj as CoroutineDefinitionType;
			if (y == null)
				return false;

			return Yield.Equals(y.Yield) && Receive.Equals(y.Receive);
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			return Yield.GetHashCode() << 2 + Receive.GetHashCode();
		}

		public Z3.BoolExpr BuildEquality(PaperWord other, Solver engine)
		{
			throw new NotImplementedException();
		}

		public PaperWord ApplyEquation(Dictionary<PaperVariable, PaperWord> equations)
		{
			throw new NotImplementedException();
		}

		public List<PaperVariable> GetVariables()
		{
			var list = Yield.GetVariables();
			list.AddRange(Receive.GetVariables());
			return list;
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			throw new NotImplementedException();
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, PaperWord> usedConstants)
		{
			Receive.ReplaceWithConstant(availableConstants, usedConstants);
			Yield.ReplaceWithConstant(availableConstants, usedConstants);
		}

		public PaperType Normalize()
		{
			throw new NotImplementedException();
		}
	}
}
