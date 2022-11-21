using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace GeneratorCalculation
{
	public class Solver
	{

		public static Dictionary<PaperVariable, PaperWord> JoinConditions(List<Dictionary<PaperVariable, PaperWord>> conditions)
		{
			var c = new Dictionary<PaperVariable, PaperWord>();

			for (int i = 0; i < conditions.Count; i++)
			{
				c = JoinConditions(c, conditions[i]);
				if (c == null)
					return null;

			}
			return c;
		}


		public static Dictionary<PaperVariable, PaperWord> JoinConditions(Dictionary<PaperVariable, PaperWord> x, Dictionary<PaperVariable, PaperWord> y)
		{
			var k1 = new List<PaperVariable>(x.Keys);
			var k2 = new List<PaperVariable>(y.Keys);

			var duplicateKeys = k1.Intersect(k2).ToList();
			if (duplicateKeys.Count == 0)
			{
				//no potential conflicting keys
				return x.Concat(y).ToDictionary(d => d.Key, d => d.Value);
			}


			List<Dictionary<PaperVariable, PaperWord>> conditions = new List<Dictionary<PaperVariable, PaperWord>>();
			foreach (var key in duplicateKeys)
			{
				var c = x[key].IsCompatibleTo(y[key]);
				if (c == null)
					c = y[key].IsCompatibleTo(x[key]);

				conditions.Add(c);
			}

			var solveDuplication = JoinConditions(conditions);
			if (solveDuplication == null)
				return null;

			foreach (var key in duplicateKeys)
			{
				x.Remove(key);
				y.Remove(key);
			}

			var c3 = JoinConditions(x, y);
			return JoinConditions(c3, solveDuplication);
		}


		public static string FormatCondition(KeyValuePair<PaperVariable, PaperWord> p)
		{
			return $"{p.Key}/{p.Value}";
		}

		public static GeneratorType Solve(List<KeyValuePair<string, GeneratorType>> coroutines)
		{
			foreach (var g in coroutines)
			{
				g.Value.Check();
			}

			List<string> availableConstants = new List<string>();
			for (int i = 0; i < 26; i++)
				availableConstants.Add(((char)('a' + i)).ToString());

			List<string> constants = new List<string>();
			foreach (var g in coroutines)
			{
				foreach (var n in g.Value.GetVariables(constants).Select(v => v.Name))
					availableConstants.Remove(n);
			}

			foreach (var g in coroutines)
			{
				Console.WriteLine($"{g.Key}:\t{g.Value}");
			}


			foreach (var g in coroutines)
			{
				g.Value.ReplaceWithConstant(availableConstants, constants);
			}

			Console.WriteLine("== ReplaceWithConstant ==");
			Console.WriteLine($"constants: {string.Join(", ", constants)}");
			foreach (var g in coroutines)
			{
				Console.WriteLine($"{g.Key}:\t{g.Value}");
			}


			Solve(coroutines, constants, out var yieldsToOutside);

			var yields = new SequenceType(yieldsToOutside).Normalize();

			var result = new GeneratorType(yields, ConcreteType.Void);
			return result;

			//next(oc1) --> G void void: state transition.
		}


		static void Solve(List<KeyValuePair<string, GeneratorType>> pairs, List<string> constants, out List<PaperType> yieldsToOutside)
		{
			yieldsToOutside = new List<PaperType>();
			//find a generator type where the next type is not void.
			int i = 0;
			while (i < pairs.Count)
			{
				Console.WriteLine();
				foreach (var p in pairs)
					Console.WriteLine($"{p.Key}:\t{p.Value}");


				var coroutine = pairs[i].Value;

				PaperType yieldedType = null;

				Console.Write($"{pairs[i].Key}:\t{coroutine} ");
				GeneratorType g = coroutine.RunYield(constants, ref yieldedType);
				if (g != null)
				{
					Debug.Assert(coroutine.Receive == ConcreteType.Void);

					Console.WriteLine($"--> {g}, yielded: {yieldedType}");
					if (g.Yield == ConcreteType.Void)
					{
						Console.WriteLine($"{pairs[i].Key} reached the simplest form. Remove from the list.");

						pairs.RemoveAt(i);
						//what if nowhere to receive?
						if (Receive(yieldedType, -1, pairs, constants))
						{
							i = 0;
							continue;
						}
						else
							yieldsToOutside.Add(yieldedType);
					}


				}
				else
					Console.WriteLine(" --X");

				i++;
			}

		}

		static bool Receive(PaperType yieldedType, int fromIndex, List<KeyValuePair<string, GeneratorType>> pairs, List<string> constants)
		{
			var range = new List<int>();
			for (int i = fromIndex + 1; i < pairs.Count; i++)
				range.Add(i);
			for (int i = 0; i < fromIndex; i++)
				range.Add(i);

			foreach (var i in range)
			{
				var coroutine = pairs[i].Value;
				GeneratorType newGenerator;
				Dictionary<PaperVariable, PaperWord> conditions = coroutine.RunReceive(yieldedType, out newGenerator);
				if (conditions != null)
				{
					Console.Write($"{coroutine} can receive {yieldedType} and will pop the receive part");
					if (conditions.Count == 0)
					{
						Console.WriteLine(".");
						pairs[i] = new KeyValuePair<string, GeneratorType>(pairs[i].Key, newGenerator);
					}
					else
					{
						Console.Write(" on the conditions that ");
						Console.WriteLine(string.Join(", ", conditions.Select(p => $"{p.Key}/{p.Value}")) + ".");

						var result = newGenerator.ApplyEquation(conditions.ToList());
						if (result is GeneratorType resultGenerator)
						{
							Console.WriteLine($"Therefore it becomes {result}.");
							pairs[i] = new KeyValuePair<string, GeneratorType>(pairs[i].Key, resultGenerator);
						}
						else
						{
							Console.WriteLine("But the result doesn't fit the type.");
							//In that case, can we still use the popped generator?
							throw new NotImplementedException();
							pairs[i] = new KeyValuePair<string, GeneratorType>(pairs[i].Key, newGenerator);
						}
					}

					return true;

					//Solve(pairs, constants);
					//return;
				}
			}

			return false;
		}

	}

	public class DeadLockException : Exception
	{
		private readonly List<KeyValuePair<string, GeneratorType>> lockedGenerators;

		public DeadLockException(List<KeyValuePair<string, GeneratorType>> lockedGenerators) :
			base("The following generators are locked:\n" + string.Join("\n", lockedGenerators.Select(p => $"{p.Key}: {p.Value}")))
		{
			this.lockedGenerators = new List<KeyValuePair<string, GeneratorType>>(lockedGenerators);
		}


	}


}
