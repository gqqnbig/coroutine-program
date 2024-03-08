using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Z3 = Microsoft.Z3;

namespace GeneratorCalculation
{
	public class Solver : IDisposable
	{
		private static readonly ILogger logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(Solver));

		private readonly List<CoroutineInstanceType> compositionOrder = new List<CoroutineInstanceType>();

		public readonly Z3.Context z3Ctx;
		public Z3.EnumSort ConcreteSort { get; private set; }
		public bool CanLoopExternalYield { get; set; } = true;

		private Dictionary<string, Z3.FuncDecl> functionHeads = new Dictionary<string, Z3.FuncDecl>();
		private Dictionary<string, Z3.BoolExpr> functionBodies = new Dictionary<string, Z3.BoolExpr>();

		public Solver()
		{
			z3Ctx = new Z3.Context();
		}

		public void AddZ3Function(Z3.FuncDecl head, Z3.BoolExpr body)
		{
			functionHeads.Add(head.Name.ToString(), head);
			functionBodies.Add(head.Name.ToString(), body);
		}

		public Z3.FuncDecl GetFunctionHead(string name)
		{
			if (functionHeads.TryGetValue(name, out var value))
				return value;
			else
				throw new NotSupportedException($"Function {name} is not found. Did you call AddZ3Function()?");
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

		/// <summary>
		/// Collect ConcreteTypes in coroutines and their conditions.
		/// </summary>
		/// <param name="coroutines"></param>
		/// <param name="bindings"></param>
		/// <param name="additionalTypes"></param>
		public void CollectConcreteTypes(List<Generator> coroutines, Dictionary<PaperVariable, PaperWord> bindings, IEnumerable<string> additionalTypes = null)
		{
			if (bindings == null)
				bindings = new Dictionary<PaperVariable, PaperWord>();

			HashSet<string> allTypes = new HashSet<string>();
			foreach (var g in coroutines)
			{
				var c = new ConcreteTypeCollector(bindings);
				c.Visit(g.Type);
				allTypes.UnionWith(c.concreteTypes);
				//g.Type.Check();
			}
			allTypes.Remove(ConcreteType.Void.Name);
			if (additionalTypes != null)
				allTypes.UnionWith(additionalTypes);
			ConcreteSort = z3Ctx.MkEnumSort("Concrete", allTypes.ToArray());
			logger.LogInformation("Basic variables can take values {0}", string.Join(", ", allTypes));
		}

		public CoroutineInstanceType SolveWithBindings(List<Generator> coroutines, Dictionary<PaperVariable, PaperWord> bindings = null, int steps = 500)
		{
			if (bindings == null)
				bindings = new Dictionary<PaperVariable, PaperWord>();

			// SolveWithBindings may be recursively called,
			// so we have to check if concreteSort has been assigned.
			if (ConcreteSort == null)
				CollectConcreteTypes(coroutines, bindings);


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
				if (g is CoroutineInstanceType cg)
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

				if (gx.Type.Normalize() == ConcreteType.Void)
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


		CoroutineInstanceType Solve(List<Generator> pairs, Dictionary<PaperVariable, PaperWord> constants, int steps)
		{
			//find a generator type where the next type is not void.
			Console.WriteLine();

			List<PaperType> yieldsToOutside = SolveWithinSteps(pairs, constants, steps);


			//allow at most one coroutine to have receive.
			var lockedCoroutines = (from p in pairs
									let n = p.Type.Normalize()
									where n != ConcreteType.Void
									select p).ToList();

			if (lockedCoroutines.Count > 1)
				throw new DeadLockException(yieldsToOutside, lockedCoroutines);
			else if (lockedCoroutines.Count == 1)
			{
				lockedCoroutines[0].Type.Flow.InsertRange(0, yieldsToOutside.Select(y => new DataFlow(Direction.Yielding, y)));
				return lockedCoroutines[0].Type;
			}
			Debug.Assert(lockedCoroutines.Count == 0);

			PaperType receive = ConcreteType.Void;

			var yields = new SequenceType(yieldsToOutside).Normalize();

			var result = new CoroutineInstanceType(receive, yields);
			return result;
		}

		private List<PaperType> SolveWithinSteps(List<Generator> pairs, Dictionary<PaperVariable, PaperWord> bindings, int steps)
		{
			List<PaperType> yieldsToOutside = new List<PaperType>();
			int i = 0;
			bool canWrap = false;
			while (pairs.Count > 0 && steps-- > 0)
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

					if (ReceiveGenerator(pairs))
						continue;
				}
				// Even if pairs.Count == 1, we have to continue executing the yielding part 
				// because it may yield coroutines.


				var coroutine = pairs[i].Type;

				PaperType yieldedType = null;
				ReceiveGenerator(pairs);

				Console.Write($"{pairs[i].Name}:\t{coroutine} ");

				if (coroutine.Flow.Count > 0 && coroutine.Flow[0].Direction == Direction.Yielding)
				{
					yieldedType = coroutine.Flow[0].Type;

					if (yieldedType is SequenceType)
						logger.LogWarning($"SequenceType {yieldedType} is not supported for receiving.");

					yieldedType = (PaperType)yieldedType.ApplyEquation(bindings);

					coroutine.Flow.RemoveAt(0);
					compositionOrder.Add(pairs[i].Type);
					Console.WriteLine($"--> {coroutine}, yielded: {yieldedType}");

					canWrap = true;


					pairs[i].Type = coroutine;

					if (yieldedType is TupleType tTuple)
					{
						if (tTuple.Types.All(t => t is CoroutineInstanceType))
						{
							try
							{
								yieldedType = SolveWithBindings(tTuple.Types.Select(t => new Generator("", (CoroutineInstanceType)t)).ToList(), bindings);
							}
							catch (DeadLockException e)
							{
								yieldedType = new SequenceType(e.YieldsToOutside);
								foreach (var eg in e.LockedGenerators)
									pairs.Insert(i + 1, eg);
							}
						}
					}


					if (yieldedType is CoroutineInstanceType)
					{
						pairs.Insert(i + 1, new Generator("", (CoroutineInstanceType)yieldedType));
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

				if (CanLoopExternalYield && i >= pairs.Count)
				{
					if (LoopExternalYield(yieldsToOutside, pairs))
						canWrap = true;
				}
			}

			if (steps <= 0)
				throw new StepLimitExceededException();

			return yieldsToOutside;
		}


		bool LoopExternalYield(List<PaperType> yieldsToOutside, List<Generator> pairs)
		{
			Console.WriteLine("Loop external yield: " + string.Join(", ", yieldsToOutside));
			for (int i = 0; i < yieldsToOutside.Count; i++)
			{
				var pendingType = yieldsToOutside[i];

				for (int j = 0; j < pairs.Count; j++)
				{
					Debug.Assert(pendingType is CoroutineInstanceType == false);


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


		bool ReceiveGenerator(List<Generator> pairs)
		{

			for (var i = 0; i < pairs.Count; i++)
			{
				PaperType head = null;
				PaperType remaining = null;

				if (pairs[i].Type.Flow.Count == 0 || pairs[i].Type.Flow[0].Direction == Direction.Yielding)
					continue;

				pairs[i].Type.Flow[0].Type.Pop(ref head, ref remaining);

				if (head is CoroutineInstanceType)
					throw new NotImplementedException();
				else if (head is ListType l)
				{
					if (l.Type is CoroutineInstanceType receiveG)
					{
						using (var solver = z3Ctx.MkSolver())
						{
							solver.Add(functionBodies.Values.ToList());
							List<int> matches = new List<int>();
							for (int j = 0; j < pairs.Count; j++)
							{
								if (i == j)
									continue;

								//solver.Push();

								Z3.BoolExpr eqExpr = receiveG.BuildEquality(pairs[j].Type, this);
								if (solver.Check(eqExpr) == Z3.Status.SATISFIABLE)
								{
									matches.Add(j);
								}
								//else
								//solver.Pop(); // Remove the bad conditions added by IsCompatibleTo.

							}


							if (l.Size is PaperInt pi)
							{
								if (matches.Count < pi.Value)
								{
									logger.LogInformation($"No enough coroutines to match {receiveG}. Nothing is removed.");
									solver.Add(solver.Context.MkFalse());
								}
								else
									matches = matches.Take(pi.Value).ToList();

							}
							else if (l.Size is PaperVariable pv)
							{
								//Deal with [a;b]^i. 
								//Variable i must be equal to the number of matches.

								solver.Add(z3Ctx.MkEq(z3Ctx.MkIntConst(pv.Name), z3Ctx.MkInt(matches.Count)));
							}
							else
								throw new NotImplementedException();

							if (solver.Check() == Z3.Status.SATISFIABLE)
							{
								try
								{
									Dictionary<PaperVariable, PaperWord> conditions = Z3Helper.GetAssignments(solver);

									//pairs[i].Type.Receive.Pop
									pairs[i].Type.Flow.RemoveAt(0);
									pairs[i].Type = pairs[i].Type.ApplyEquation(conditions);

									Console.Write($"{pairs[i].Name} becomes {pairs[i].Type}");
									if (conditions.Count > 0)
									{
										Console.Write(" on the conditions that ");
										Console.Write(string.Join(", ", conditions.Select(p => p.Key + "/" + p.Value)));
									}

									Console.WriteLine(".");

									foreach (int indice in matches.OrderByDescending(v => v))
										pairs.RemoveAt(indice);

									//Run one more time
									ReceiveGenerator(pairs);
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
		int? Receive(PaperType pendingType, List<Generator> pairs, int startIndex)
		{
			Console.WriteLine();

			for (var i = 0; i < pairs.Count; i++)
			{
				var coroutine = pairs[(i + startIndex) % pairs.Count].Type;
				if (coroutine.Flow.Count == 0 || coroutine.Flow[0].Direction == Direction.Yielding)
				{
					Console.WriteLine($"{pairs[i].Name}:\t{pairs[i].Type} -- Cannot receive {pendingType}");
					continue;
				}

				using (Z3.Solver solver = z3Ctx.MkSolver())
				{
					var acceptor = coroutine.Flow[0].Type;

					solver.Add(functionBodies.Values.ToList());
					var exp = acceptor.BuildEquality(pendingType, this);
					solver.Add(exp);
					exp = coroutine.AddConstraints(this);
					if (exp != null)
						solver.Add(exp);
					if (solver.Check() == Z3.Status.SATISFIABLE)
					{
						var tmp = coroutine.Clone();
						tmp.Flow.RemoveAt(0);
						CoroutineInstanceType newGenerator = tmp;

						Console.Write($"{pairs[(i + startIndex) % pairs.Count].Name}:\t{coroutine} can receive {pendingType}");

						//var g2 = CheckReceive(pendingType, pairs, (i + from) % pairs.Count + 1);
						//if (g2 != null)
						//{
						//	if (coroutine.Equals(g2) == false)
						//		throw new FormatException($"{coroutine} and {g2} both can receive, which is not allowed.");
						//	//TODO: may have to loop and check further receivable.
						//}


						if (solver.Model.NumConsts == 0)
						{
							Console.WriteLine(".");
							pairs[(i + startIndex) % pairs.Count].Type = newGenerator;
						}
						else
						{
							Dictionary<PaperVariable, PaperWord> conditions = Z3Helper.GetAssignments(solver);
							Console.Write(" on the conditions that ");
							Console.Write(string.Join(", ", conditions.Select(p => p.Key + "/" + p.Value)));
							Console.WriteLine(".");

							try
							{
								var resultGenerator = newGenerator.ApplyEquation(conditions);
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
			}

			return null;
		}



		//static GeneratorType CheckReceive(PaperType pendingType, List<Generator> pairs, int start)
		//{
		//	for (int i = start; i < pairs.Count; i++)
		//	{
		//		var coroutine = pairs[i].Type;
		//		GeneratorType newGenerator;
		//		Dictionary<PaperVariable, PaperWord> conditions = coroutine.RunReceive(pendingType, out newGenerator);
		//		if (conditions != null)
		//			return coroutine;
		//	}

		//	return null;

		//}

		public void Dispose()
		{
			z3Ctx.Dispose();
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
