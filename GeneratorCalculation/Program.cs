using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeneratorCalculation
{
	class Program
	{
		static void Main(string[] args)
		{
			List<KeyValuePair<string, GeneratorType>> coroutines = new List<KeyValuePair<string, GeneratorType>>();
			coroutines.Add(new KeyValuePair<string, GeneratorType>("oc1", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("oc2", new GeneratorType((ConcreteType)"Y", ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("fr1", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), ConcreteType.Void)));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("fr2", new GeneratorType(new ListType((ConcreteType)"S", PaperStar.Instance), ConcreteType.Void)));


			GeneratorType interleave = new GeneratorType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")),
				new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")));
			coroutines.Add(new KeyValuePair<string, GeneratorType>("interleave", interleave));

			foreach (var g in coroutines) { g.Value.Check(); }

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


			Solve(coroutines, constants);

			//next(oc1) --> G void void: state transition.
		}

		static void Solve(List<KeyValuePair<string, GeneratorType>> pairs, List<string> constants)
		{
			//find a generator type where the next type is not void.
			int i = 0;
			while (i < pairs.Count)
			{
				Console.WriteLine();
				foreach (var p in pairs)
					Console.WriteLine($"{p.Key}:\t{p.Value}");


				var coroutine = pairs[i].Value;

				GeneratorType g = null;
				PaperType yieldedType = null;

				Console.Write($"{pairs[i].Key}:\t{coroutine} ");
				if (coroutine.RunYield(constants, ref g, ref yieldedType))
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
							throw new Exception("Deadlock?");
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
						Console.WriteLine(".");
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
}
