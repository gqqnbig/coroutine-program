using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			if (x == null || y == null)
				return null;

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

		public static GeneratorType Solve(List<Generator> coroutines, int steps = 500)
		{
			foreach (var g in coroutines)
			{
				g.Type.Check();
			}


			List<string> availableConstants = new List<string>();
			for (int i = 0; i < 26; i++)
				availableConstants.Add(((char)('a' + i)).ToString());

			List<string> constants = new List<string>();
			foreach (var g in coroutines)
			{
				foreach (var n in g.Type.GetVariables(constants).Select(v => v.Name))
					availableConstants.Remove(n);
			}

			foreach (var g in coroutines)
			{
				Console.WriteLine($"{g.Name}:\t{g.Type}");
			}


			foreach (var g in coroutines)
			{
				g.Type.ReplaceWithConstant(availableConstants, constants);
			}

			if (constants.Count > 0)
			{
				Console.WriteLine("== ReplaceWithConstant ==");
				Console.WriteLine($"constants: {string.Join(", ", constants)}");
				foreach (var g in coroutines)
					Console.WriteLine($"{g.Name}:\t{g.Type}");
			}


			return Solve(coroutines, constants, steps);
		}

		static bool RemoveVoid(List<Generator> pairs)
		{
			bool isProcessed = false;
			for (int i = 0; i < pairs.Count; i++)
			{
				Generator gx = pairs[i];

				if (gx.Type.Yield == ConcreteType.Void && gx.Type.Receive == ConcreteType.Void)
				{
					if (gx.IsInfinite)
					{
						Console.WriteLine($"{gx.Name} reached the simplest form. Reset to original.");
						gx.Type = gx.OriginalType.Clone();
					}
					else
					{
						Console.WriteLine($"{gx.Name} reached the simplest form. Remove from the list.");
						pairs.RemoveAt(i);
						i--;
					}
					isProcessed = true;
				}
			}

			return isProcessed;
		}


		static GeneratorType Solve(List<Generator> pairs, List<string> constants, int steps)
		{
			List<PaperType> yieldsToOutside = new List<PaperType>();
			//find a generator type where the next type is not void.
			Console.WriteLine();

			int i = 0;
			int s = 0;
			bool canWrap = false;
			while (pairs.Count > 0 && s++ < steps)
			{
				if (i >= pairs.Count)
				{
					if (canWrap)
					{
						i = 0;
						canWrap = false;
					}
					else
						break;
				}


				if (RemoveVoid(pairs))
				{
					i = 0;
					continue;
				}

				if(ReceiveGenerator(pairs, constants))
				{
					i = 0;
					continue;
				}

				var coroutine = pairs[i].Type;


				PaperType yieldedType = null;

				Console.Write($"{pairs[i].Name}:\t{coroutine} ");
				GeneratorType g = coroutine.RunYield(constants, ref yieldedType);
				if (g != null)
				{
					Debug.Assert(coroutine.Receive == ConcreteType.Void);


					//var g2 = CheckYield(pairs, constants, i + 1);
					//if (g2 != null)
					//{
					//	if (coroutine.Equals(g2) == false)
					//		throw new FormatException($"{coroutine} and {g2} both can yield, which is not allowed.");
					//	//TODO: may have to loop and check further yieldables.
					//}

					//yieldedType = yieldedType.Normalize();

					Console.WriteLine($"--> {g}, yielded: {yieldedType}");

					canWrap = true;


					pairs[i].Type = g;

					if (yieldedType is GeneratorType)
					{
						pairs.Insert(i + 1, new Generator("", (GeneratorType)yieldedType));
					}
					else
					{
						var receiverIndex = Receive(yieldedType, pairs, constants, i);
						if (receiverIndex != null)
						{
							i = receiverIndex.Value;
						}
						else
						{
							Console.WriteLine($"Add to external yield: {yieldedType}");
							yieldsToOutside.Add(yieldedType);
							i = 0;
						}
					}

					continue;


				}
				else
					Console.WriteLine(" -- Not ready to yield");

				i++;
			}

			if (s >= steps)
				throw new StepLimitExceededException();


			//allow at most one coroutine to have receive.
			var lockedCoroutines = pairs.Where(p => p.Type.Receive != ConcreteType.Void).ToList();
			if (lockedCoroutines.Count > 1)
				throw new DeadLockException(yieldsToOutside, lockedCoroutines);
			else if (lockedCoroutines.Count == 1)
				yieldsToOutside.Add(lockedCoroutines[0].Type.Yield);

			PaperType receive = lockedCoroutines.Count == 1 ? lockedCoroutines[0].Type.Receive : ConcreteType.Void;

			var yields = new SequenceType(yieldsToOutside).Normalize();

			var result = new GeneratorType(yields, receive);
			return result;
		}

		/// <summary>
		///
		/// 
		/// </summary>
		/// <param name="pairs"></param>
		/// <param name="constants"></param>
		/// <param name="start">inclusive</param>
		/// <returns>one coroutine can yield</returns>
		static GeneratorType CheckYield(List<Generator> pairs, List<string> constants, int start)
		{
			for (int i = start; i < pairs.Count; i++)
			{
				var coroutine = pairs[i].Type;
				PaperType yieldedType = null;
				GeneratorType g = coroutine.RunYield(constants, ref yieldedType);
				if (g != null)
					return coroutine;
			}

			return null;
		}


		static bool ReceiveGenerator(List<Generator> pairs, List<string> constants)
		{

			for (var i = 0; i < pairs.Count; i++)
			{
				PaperType head = null;
				PaperType remaining = null;
				pairs[i].Type.Receive.Pop(ref head, ref remaining);

				if (head is GeneratorType)
					throw new NotImplementedException();
				else if (head is ListType l)
				{
					if (l.Type is GeneratorType receiveG)
					{
						Dictionary<PaperVariable, PaperWord> conditions = new Dictionary<PaperVariable, PaperWord>();
						List<int> matches = new List<int>();
						for (int j = 0; j < pairs.Count; j++)
						{
							if (i == j)
								continue;

							var c = JoinConditions(conditions, receiveG.IsCompatibleTo(pairs[j].Type));
							if (c != null)
							{
								conditions = c;
								matches.Add(j);
							}
						}


						if (l.Size is PaperInt pi)
						{
							if (matches.Count < pi.Value)
							{
								Console.WriteLine($"No enough coroutines to match {receiveG}. Nothing is removed.");
								conditions = null;
							}
							else
								matches = matches.Take(pi.Value).ToList();

						}
						else if (l.Size is PaperVariable pv)
						{
							conditions = JoinConditions(conditions, new Dictionary<PaperVariable, PaperWord> { { pv.Name, (PaperInt)matches.Count } });
						}
						else
							throw new NotImplementedException();

						if (conditions != null)
						{
							try
							{
								//pairs[i].Type.Receive.Pop
								pairs[i].Type = new GeneratorType(pairs[i].Type.ForbiddenBindings, remaining, pairs[i].Type.Yield).ApplyEquation(conditions.ToList());
								Console.Write($"{pairs[i].Name} becomes {pairs[i].Type}");
								if (conditions.Count > 0)
								{
									Console.Write(" on the conditions that ");
									Console.Write(string.Join(", ", conditions.Select(p => $"{p.Key}/{p.Value}")));
								}

								Console.WriteLine(".");

								foreach (int indice in matches.OrderByDescending(v => v))
									pairs.RemoveAt(indice);

								//Run one more time
								ReceiveGenerator(pairs, constants);
								return true;
							}
							catch (PaperSyntaxException e)
							{
								Console.WriteLine(e.Message);
							}
						}
					}
				}
			}

			return false;
		}

		static int? Receive(PaperType pendingType, List<Generator> pairs, List<string> constants, int from)
		{
			Console.WriteLine();

			for (var i = 0; i < pairs.Count; i++)
			{
				var coroutine = pairs[(i + from) % pairs.Count].Type;
				GeneratorType newGenerator;
				Dictionary<PaperVariable, PaperWord> conditions = coroutine.RunReceive(pendingType, out newGenerator);
				if (conditions != null)
				{
					Console.Write($"{pairs[(i + from) % pairs.Count].Name}:\t{coroutine} can receive {pendingType}");

					//var g2 = CheckReceive(pendingType, pairs, (i + from) % pairs.Count + 1);
					//if (g2 != null)
					//{
					//	if (coroutine.Equals(g2) == false)
					//		throw new FormatException($"{coroutine} and {g2} both can receive, which is not allowed.");
					//	//TODO: may have to loop and check further receivable.
					//}

					if (conditions.Count == 0)
					{
						Console.WriteLine(".");
						pairs[(i + from) % pairs.Count].Type = newGenerator;
					}
					else
					{
						Console.Write(" on the conditions that ");
						Console.WriteLine(string.Join(", ", conditions.Select(p => $"{p.Key}/{p.Value}")) + ".");

						try
						{
							var resultGenerator = newGenerator.ApplyEquation(conditions.ToList());
							Console.WriteLine($"Therefore it becomes {resultGenerator}.");

							//if (resultGenerator.Yield == ConcreteType.Void)
							//{
							//	if (pairs[i].IsInfinite)
							//	{
							//		Console.WriteLine($"{pairs[i].Name} reached the simplest form. Reset to original.");
							//		pairs[i].Type = pairs[i].OriginalType.Clone();
							//	}
							//	else
							//	{
							//		Console.WriteLine($"{pairs[i].Name} reached the simplest form. Remove from the list.");
							//		pairs[i] = null;
							//	}
							//}
							//else 
							pairs[(i + from) % pairs.Count].Type = resultGenerator;
						}
						catch (PaperSyntaxException e)
						{
							Console.WriteLine("But the result doesn't fit the type. " + e.Message);
							continue;
						}
					}

					return (i + from) % pairs.Count;

					//Solve(pairs, constants);
					//return;
				}
				else
				{
					Console.WriteLine($"{pairs[(i + from) % pairs.Count].Name}:\t{pairs[(i + from) % pairs.Count].Type} -- Cannot receive {pendingType}");
				}
			}

			return null;
		}

		static GeneratorType CheckReceive(PaperType pendingType, List<Generator> pairs, int start)
		{
			for (int i = start; i < pairs.Count; i++)
			{
				var coroutine = pairs[i].Type;
				GeneratorType newGenerator;
				Dictionary<PaperVariable, PaperWord> conditions = coroutine.RunReceive(pendingType, out newGenerator);
				if (conditions != null)
					return coroutine;
			}

			return null;

		}

	}

	public class DeadLockException : Exception
	{
		public List<Generator> LockedGenerators { get; }
		public List<PaperType> YieldsToOutside { get; }

		public DeadLockException(List<PaperType> yieldsToOutside, List<Generator> lockedGenerators) :
			base("After yielding " + string.Join(", ", yieldsToOutside) + ", the following generators are locked:\n" + string.Join("\n", lockedGenerators.Select(p => $"{p.Name}: {p.Type}")))
		{
			this.LockedGenerators = new List<Generator>(lockedGenerators);
			this.YieldsToOutside = yieldsToOutside;
		}


	}

	public class StepLimitExceededException : Exception
	{

	}


}
