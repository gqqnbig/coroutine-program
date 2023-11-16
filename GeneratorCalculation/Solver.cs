using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GeneratorCalculation
{
	public class Solver
	{
		private static readonly ILogger logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(Solver));
		
		private readonly List<GeneratorType> compositionOrder = new List<GeneratorType>();

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


		static List<string> BuildAvailableNames(List<Generator> coroutines, Dictionary<PaperVariable, PaperWord> bindings)
		{
			HashSet<string> availableConstants = new HashSet<string>();
			for (int i = 0; i < 26; i++)
				availableConstants.Add(((char)('a' + i)).ToString());

			//Dictionary<string, PaperType> constants = new Dictionary<string, PaperType>();
			foreach (var g in coroutines)
			{
				availableConstants.ExceptWith(g.Type.GetVariables().Select(v => v.Name));
			}

			foreach (var w in bindings.Values)
			{
				if (w is PaperType pt)
				{
					availableConstants.ExceptWith(pt.GetVariables().Select(v => v.Name));
				}
			}

			return availableConstants.ToList();
		}

		public GeneratorType SolveWithBindings(List<Generator> coroutines, Dictionary<PaperVariable, PaperWord> bindings = null, int steps = 500)
		{
			if (bindings == null)
				bindings = new Dictionary<PaperVariable, PaperWord>();

			//foreach (var g in coroutines)
			//{
			//	g.Type.Check();
			//}


			StringBuilder sb = new StringBuilder();
			foreach (var g in coroutines)
			{
				sb.AppendLine($"{g.Name}:\t{g.Type}");
			}

			Console.WriteLine("compose({0})", sb);
			if (bindings.Count > 0)
			{
				var m = from p in bindings
						select p.Key.ToString() +
						(p.Value == null ? " is a constant" : ("=" + p.Value.ToString()));
				Console.WriteLine("where " + string.Join(",\n", m) + "\n.");
			}

			List<string> availableConstants = BuildAvailableNames(coroutines, bindings);
			foreach (var g in coroutines)
			{
				g.Type.ReplaceWithConstant(availableConstants, bindings);
			}
			foreach (var w in bindings.Values.ToList())
			{
				if (w is PaperType pt)
					pt.ReplaceWithConstant(availableConstants, bindings);
			}


			var constant = bindings.Where(p => p.Value == null).Select(p => p.Key.Name).ToList();
			if (constant.Count > 0)
			{
				logger.LogInformation("== ReplaceWithConstant ==");
				logger.LogInformation($"constants: {string.Join(", ", constant)}");
				foreach (var g in coroutines)
					logger.LogInformation($"{g.Name}:\t{g.Type}");
			}


			var result = Solve(coroutines, bindings, steps);

			Console.WriteLine("\nComposition order:\n" + string.Join(" ->\n", compositionOrder.Select(g =>
			{
				if (g is CoroutineType cg)
					return string.IsNullOrEmpty(cg.Source?.Name) ? g.ToString() : cg.Source.Name;
				else
					return g.ToString();
			})));

			return result;
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
						logger.LogInformation($"{gx.Name} reached the simplest form. Reset to original.");
						gx.Type = gx.OriginalType.Clone();
					}
					else
					{
						logger.LogInformation($"{gx.Name} reached the simplest form. Remove from the list.");
						pairs.RemoveAt(i);
						i--;
					}
					isProcessed = true;
				}
			}

			return isProcessed;
		}


		GeneratorType Solve(List<Generator> pairs, Dictionary<PaperVariable, PaperWord> constants, int steps)
		{
			//find a generator type where the next type is not void.
			Console.WriteLine();

			List<PaperType> yieldsToOutside = SolveWithinSteps(pairs, constants, steps);


			//allow at most one coroutine to have receive.
			var lockedCoroutines = pairs.Where(p => p.Type.Receive != ConcreteType.Void).ToList();
			if (lockedCoroutines.Count > 1)
				throw new DeadLockException(yieldsToOutside, lockedCoroutines);
			else if (lockedCoroutines.Count == 1)
			{
				SequenceType s = lockedCoroutines[0].Type.Yield as SequenceType;
				if (s == null)
					yieldsToOutside.Add(lockedCoroutines[0].Type.Yield);
				else
					yieldsToOutside.AddRange(s.Types);
			}

			PaperType receive = lockedCoroutines.Count == 1 ? lockedCoroutines[0].Type.Receive : ConcreteType.Void;

			var yields = new SequenceType(yieldsToOutside).Normalize();

			var result = new GeneratorType(yields, receive);
			return result;
		}

		private List<PaperType> SolveWithinSteps(List<Generator> pairs, Dictionary<PaperVariable, PaperWord> constants, int steps)
		{
			List<PaperType> yieldsToOutside = new List<PaperType>();
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

				if (i == 0)
				{
					if (RemoveVoid(pairs))
						continue;

					if (ReceiveGenerator(pairs, constants.Keys.Select(v => v.Name).ToList()))
						continue;
				}
				// Even if pairs.Count == 1, we have to continue executing the yielding part 
				// because it may yield coroutines.

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

					compositionOrder.Add(pairs[i].Type);
					Console.WriteLine($"--> {g}, yielded: {yieldedType}");

					canWrap = true;


					pairs[i].Type = g;

					if (yieldedType is TupleType tTuple)
					{
						if (tTuple.Types.All(t => t is GeneratorType))
						{
							try
							{
								yieldedType = SolveWithBindings(tTuple.Types.Select(t => new Generator("", (GeneratorType)t)).ToList(), constants);
							}
							catch (DeadLockException e)
							{
								yieldedType = new SequenceType(e.YieldsToOutside);
								foreach (var eg in e.LockedGenerators)
									pairs.Insert(i + 1, eg);
							}
						}
					}


					if (yieldedType is GeneratorType)
					{
						pairs.Insert(i + 1, new Generator("", (GeneratorType)yieldedType));
					}
					else if (yieldedType is SequenceType ys)
					{
						yieldsToOutside.AddRange(ys.Types);
					}
					else
					{
						var receiverIndex = Receive(yieldedType, pairs, i);
						if (receiverIndex != null)
						{
							compositionOrder.Add(pairs[receiverIndex.Value].Type);

							i = receiverIndex.Value;
						}
						else
						{
							logger.LogInformation($"Add to external yield: {yieldedType}");
							yieldsToOutside.Add(yieldedType);
							i = 0;
						}
					}

					continue;


				}
				else
					Console.WriteLine(" -- Not ready to yield");

				i++;

				if (i >= pairs.Count)
				{
					if (LoopExternalYield(yieldsToOutside, pairs))
						canWrap = true;
				}
			}

			if (s >= steps)
				throw new StepLimitExceededException();

			return yieldsToOutside;
		}


		static bool LoopExternalYield(List<PaperType> yieldsToOutside, List<Generator> pairs)
		{
			Console.WriteLine("Loop external yield: " + string.Join(", ", yieldsToOutside));
			for (int i = 0; i < yieldsToOutside.Count; i++)
			{
				var pendingType = yieldsToOutside[i];

				for (int j = 0; j < pairs.Count; j++)
				{
					Debug.Assert(pendingType is GeneratorType == false);


					var receiverIndex = Receive(pendingType, pairs, 0);
					if (receiverIndex != null)
					{
						yieldsToOutside.RemoveAt(i);
						return true;
					}
				}
			}

			return false;
		}


		///// <summary>
		/////
		///// 
		///// </summary>
		///// <param name="pairs"></param>
		///// <param name="constants"></param>
		///// <param name="start">inclusive</param>
		///// <returns>one coroutine can yield</returns>
		//static GeneratorType CheckYield(List<Generator> pairs, List<string> constants, int start)
		//{
		//	for (int i = start; i < pairs.Count; i++)
		//	{
		//		var coroutine = pairs[i].Type;
		//		PaperType yieldedType = null;
		//		GeneratorType g = coroutine.RunYield(constants, ref yieldedType);
		//		if (g != null)
		//			return coroutine;
		//	}

		//	return null;
		//}


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
								logger.LogInformation($"No enough coroutines to match {receiveG}. Nothing is removed.");
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
								logger.LogInformation(e.Message);
							}
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pendingType"></param>
		/// <param name="pairs"></param>
		/// <param name="constants"></param>
		/// <param name="startIndex">From which index we start to evaluate</param>
		/// <returns></returns>
		static int? Receive(PaperType pendingType, List<Generator> pairs, int startIndex)
		{
			Console.WriteLine();

			for (var i = 0; i < pairs.Count; i++)
			{
				var coroutine = pairs[(i + startIndex) % pairs.Count].Type;
				GeneratorType newGenerator;
				Dictionary<PaperVariable, PaperWord> conditions = coroutine.RunReceive(pendingType, out newGenerator);
				if (conditions != null)
				{
					Console.Write($"{pairs[(i + startIndex) % pairs.Count].Name}:\t{coroutine} can receive {pendingType}");

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
						pairs[(i + startIndex) % pairs.Count].Type = newGenerator;
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
							pairs[(i + startIndex) % pairs.Count].Type = resultGenerator;
						}
						catch (PaperSyntaxException e)
						{
							Console.WriteLine("But the result doesn't fit the type. " + e.Message);
							continue;
						}
					}

					return (i + startIndex) % pairs.Count;

					//Solve(pairs, constants);
					//return;
				}
				else
				{
					Console.WriteLine($"{pairs[(i + startIndex) % pairs.Count].Name}:\t{pairs[(i + startIndex) % pairs.Count].Type} -- Cannot receive {pendingType}");
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
