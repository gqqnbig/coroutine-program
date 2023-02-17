using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneratorCalculation
{

	public class GeneratorType : PaperType
	{
		public GeneratorType(params DataFlow[] flow)
		{
			Flow = new List<DataFlow>(flow);
		}

		public GeneratorType(IEnumerable<DataFlow> flow)
		{
			Flow = new List<DataFlow>(flow);
		}

		public GeneratorType(PaperType @yield, PaperType receive)
		{
			//Yield = yield;
			//Receive = receive;


			Flow = new List<DataFlow>();
			if (receive is SequenceType rs)
			{
				foreach (var t in rs.Types)
					Flow.Add(new DataFlow(Direction.Resuming, t));
			}
			else if (receive != ConcreteType.Void)
				Flow.Add(new DataFlow(Direction.Resuming, receive));

			if (@yield is SequenceType ys)
			{
				foreach (var t in ys.Types)
					Flow.Add(new DataFlow(Direction.Yielding, t));
			}
			else if (@yield != ConcreteType.Void)
				Flow.Add(new DataFlow(Direction.Yielding, @yield));

		}

		//public PaperType Yield { get; }

		//public PaperType Receive { get; }

		public List<DataFlow> Flow { get; }


		///// <summary>
		///// 
		///// </summary>
		///// <param name="freeVariables"></param>
		///// <param name="allVariables">Bounding variables, used in Resuming part</param>
		//public void Check(Dictionary<PaperVariable, ConcreteType> freeVariables, HashSet<PaperVariable> allVariables)
		//{
		//	//Receive is like input, yield is like output.
		//	//Yield cannot have variables that are unbound from Receive.

		//	List<string> constants = new List<string>();

		//	var fv = GetUnboundVariables(constants);
		//	foreach (var x in fv)
		//	{
		//		if (freeVariables.TryAdd(x, null) == false)
		//			throw new FormatException($"Free variable {x} is already used in other coroutines.");
		//	}

		//	allVariables.UnionWith(GetVariables(constants));
		//}


		public override string ToString()
		{
			var s = from f in Flow
					select (f.Direction == Direction.Yielding ? "+" : "-") + f.Type;

			return $"R[{string.Join("; ", s)} ]";
		}

		public HashSet<PaperVariable> GetVariables(List<string> constants)
		{
			var inputVariables = Flow.Where(f => f.Direction == Direction.Resuming).SelectMany(f => f.Type.GetVariables(constants));
			var outputVariables = Flow.Where(f => f.Direction == Direction.Yielding).SelectMany(f => f.Type.GetVariables(constants));
			return inputVariables.Concat(outputVariables).ToHashSet();
		}

		public HashSet<PaperVariable> GetUnboundVariables(List<string> constants)
		{
			var inputVariables = Flow.Where(f => f.Direction == Direction.Resuming).SelectMany(f => f.Type.GetUnboundVariables(constants));
			var outputVariables = Flow.Where(f => f.Direction == Direction.Yielding).SelectMany(f => f.Type.GetUnboundVariables(constants));
			return outputVariables.Except(inputVariables).ToHashSet();
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			if (t is GeneratorType another)
			{
				var x = Normalize();
				var y = another.Normalize();

				if (x is GeneratorType xg && y is GeneratorType yg)
				{
					if (xg.Flow.Count != yg.Flow.Count)
						return null;


					Dictionary<PaperVariable, PaperWord> conditions = new Dictionary<PaperVariable, PaperWord>();
					for (int i = 0; i < xg.Flow.Count; i++)
					{
						if (xg.Flow[i].Direction != yg.Flow[i].Direction)
							return null;

						Dictionary<PaperVariable, PaperWord> c = xg.Flow[i].Type.IsCompatibleTo(yg.Flow[i].Type);
						conditions = Solver.JoinConditions(conditions, c);
						if (conditions == null)
							return null;
					}

					return conditions;
				}
			}

			return null;
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
		public GeneratorType ApplyEquation(Dictionary<PaperVariable, PaperWord> equations)
		{
			try
			{
				var newFlow = Flow.Select(f => new DataFlow(f.Direction, (PaperType)f.Type.ApplyEquation(equations)));
				return new GeneratorType(newFlow);
			}
			catch (InvalidCastException)
			{
				return this;
			}
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			return false;
		}

		public void ReplaceWithConstant(List<string> availableConstants, Dictionary<PaperVariable, ConcreteType> usedConstants)
		{
			foreach (var f in Flow)
			{
				f.Type.ReplaceWithConstant(availableConstants, usedConstants);
			}
		}

		/// <summary>
		/// Either Void or GeneratorType in which all Avoid types are removed.
		/// </summary>
		/// <returns></returns>
		public PaperType Normalize()
		{
			var nf = (from f in Flow
					  let n = f.Type.Normalize()
					  where n != ConcreteType.Void
					  select new DataFlow(f.Direction, n)).ToList();

			if (nf.Count == 0)
				return ConcreteType.Void;
			else
				return new GeneratorType(nf);
		}

		public HashSet<ConcreteType> GetConcreteTypes()
		{
			HashSet<ConcreteType> result = new HashSet<ConcreteType>();
			foreach (var f in Flow)
			{
				result = result.Union(f.Type.GetConcreteTypes()).ToHashSet();
			}
			return result;
		}


		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj is GeneratorType objGenerator)
			{
				if (Flow.Count != objGenerator.Flow.Count)
					return false;

				for (int i = 0; i < Flow.Count; i++)
				{
					if (Flow[i].Equals(objGenerator.Flow[i]) == false)
						return false;
				}
				return true;
			}

			return false;
		}

		// override object.GetHashCode
		public override int GetHashCode()
		{
			int hash = typeof(GeneratorType).GetHashCode();
			for (int i = 0; i < Flow.Count; i++)
			{
				hash = hash ^ (10 * i + Flow[i].GetHashCode());
			}

			return hash;
		}

		public GeneratorType Clone()
		{
			return new GeneratorType(Flow);
		}
	}

	public class DataFlow
	{
		public DataFlow(Direction direction, PaperType type)
		{
			Direction = direction;
			Type = type;
		}

		public Direction Direction { get; }

		public PaperType Type { get; }


		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj is DataFlow objD)
				return objD.Direction == Direction && objD.Type.Equals(Type);
			return false;
		}

	}

	public enum Direction
	{
		Yielding,
		Resuming
	}
}
