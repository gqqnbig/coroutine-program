using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneratorCalculation
{

	public class GeneratorType : PaperType
	{
		public GeneratorType(PaperType @yield, PaperType receive)
		{
			Yield = yield;
			Receive = receive;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="forbiddenBindings">
		/// Key and value both can have <see cref="PaperVariable"/>.
		/// <code>[(x,y)] = {(String, Path)}</code>
		/// <code>[(x)] = {(y), (S)}</code>
		/// </param>
		/// <param name="receive"></param>
		/// <param name="yield"></param>
		public GeneratorType(Dictionary<SequenceType, List<SequenceType>> forbiddenBindings, PaperType receive, PaperType @yield)
		{
			ForbiddenBindings = forbiddenBindings;
			Receive = receive;
			Yield = yield;
		}

		public PaperType Yield { get; }

		public PaperType Receive { get; }

		public Dictionary<SequenceType, List<SequenceType>> ForbiddenBindings { get; } = new Dictionary<SequenceType, List<SequenceType>>();

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

		/// <summary>
		/// If it can yield, return the new type. Otherwise return null.
		/// </summary>
		/// <param name="constants"></param>
		/// <param name="g"></param>
		/// <param name="yieldedType"></param>
		/// <returns></returns>
		public GeneratorType RunYield(List<string> constants, ref PaperType yieldedType)
		{
			if (Receive != ConcreteType.Void)
				return null;


			if (Yield.GetVariables(constants).Count == 0)
			{
				PaperType remaining = null;
				if (Yield.Pop(ref yieldedType, ref remaining))
					return new GeneratorType(remaining, Receive);

			}

			return null;
		}

		/// <summary>
		/// Check whether this generator can receive the given type.
		/// </summary>
		/// <param name="providedType"></param>
		/// <param name="conditions"></param>
		/// <returns></returns>
		public Dictionary<PaperVariable, PaperWord> RunReceive(PaperType providedType, out GeneratorType newGenerator)
		{
			newGenerator = null;
			Dictionary<PaperVariable, PaperWord> conditions = new Dictionary<PaperVariable, PaperWord>();
			if (Receive == ConcreteType.Void)
				return null;

			PaperType head = null;
			PaperType remaining = null;
			if (Receive.Pop(ref head, ref remaining))
			{
				var c = head.IsCompatibleTo(providedType);
				if (c == null)
					return null;

				conditions = Solver.JoinConditions(conditions, c);
				if (HasForbiddenBindings(conditions))
					return null;

				newGenerator = new GeneratorType(Yield, remaining);
				return conditions;
			}

			Debug.Assert(Receive == ConcreteType.Void);
			return null;
		}

		private bool HasForbiddenBindings(Dictionary<PaperVariable, PaperWord> valueMappings)
		{
			foreach (SequenceType key in ForbiddenBindings.Keys)
			{
				var valuedKey = key.ApplyEquation(valueMappings);
				var forbiddenSet = ForbiddenBindings[key];

				if (forbiddenSet.Select(s => s.ApplyEquation(valueMappings)).Any(s => s.Equals(valuedKey)))
					return true;
			}

			return false;
		}


		public override string ToString()
		{
			if (ForbiddenBindings.Count == 0)
				return $"[{Receive}; {Yield}]";
			else
			{
				string constrain = string.Join(", ", ForbiddenBindings.Select(p => p.Key + " not in {" + string.Join(", ", p.Value) + "}"));
				return $"[{Receive}; {Yield}] where {constrain}";
			}
		}


		public List<PaperVariable> GetVariables(List<string> constants)
		{
			var inputVariables = Receive.GetVariables(constants).ToList();
			var outputVariables = Yield.GetVariables(constants).ToList();
			return inputVariables.Concat(outputVariables).ToList();
		}

		public Dictionary<PaperVariable, PaperWord> IsCompatibleTo(PaperWord t)
		{
			if (t is GeneratorType another)
				// TODO: should use full match. IsCompatibleTo only checks the head element.
				return Solver.JoinConditions(Yield.IsCompatibleTo(another.Yield), Receive.IsCompatibleTo(another.Receive));

			return null;

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
		public GeneratorType ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			var newYield = Yield.ApplyEquation(equations);
			var newReceive = Receive.ApplyEquation(equations);
			if (newYield is PaperType newYieldType && newReceive is PaperType newReceiveType)
			{
				var copy = new Dictionary<SequenceType, List<SequenceType>>();
				foreach (SequenceType key in ForbiddenBindings.Keys)
				{
					SequenceType valuedKey = (SequenceType)key.ApplyEquation(equations);
					var valuedSet = ForbiddenBindings[key].Select(s => (SequenceType)s.ApplyEquation(equations)).ToList();
					if (valuedSet.Any(s => s.Equals(valuedKey)))
						return new GeneratorType(ConcreteType.Void, ConcreteType.Void); // This identity element will be nuked.

					var c = new List<string>();
					if (valuedKey.GetVariables(c).Count == 0 && valuedSet.Sum(s => s.GetVariables(c).Count) == 0)
						continue; //Since both sides have no variables, we don't have to add them to ForbiddenBindings.

					if (copy.ContainsKey(valuedKey))
						copy[valuedKey].AddRange(valuedSet);
					else
						copy[valuedKey] = valuedSet;
				}

				return new GeneratorType(copy, newReceiveType, newYieldType);
			}
			else
				return this;
		}

		public bool Pop(ref PaperType yielded, ref PaperType remaining)
		{
			return false;
		}

		public void ReplaceWithConstant(List<string> availableConstants, List<string> usedConstants)
		{
			Yield.ReplaceWithConstant(availableConstants, usedConstants);
			Receive.ReplaceWithConstant(availableConstants, usedConstants);
		}

		public PaperType Normalize()
		{
			var g = new GeneratorType(Yield.Normalize(), Receive.Normalize());
			if (g.Yield == ConcreteType.Void && g.Receive == ConcreteType.Void)
				return ConcreteType.Void;
			else
				return g;
		}


		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj is GeneratorType objGenerator)
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

		public GeneratorType Clone()
		{
			return new GeneratorType(ForbiddenBindings, Receive, Yield);
		}
	}
}
