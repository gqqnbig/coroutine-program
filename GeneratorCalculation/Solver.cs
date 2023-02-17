﻿using System;
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

		private static Dictionary<PaperVariable, ConcreteType> RemoveStar(List<Generator> coroutines)
		{
			var allUsedNames = new HashSet<string>();
			foreach (var g in coroutines)
				allUsedNames.UnionWith(g.Type.GetVariables(new List<string>()).Select(v => v.Name));


			List<string> availableConstants = new List<string>();
			for (int i = 0; i < 26; i++)
			{
				var name = ((char)('a' + i)).ToString();
				if (allUsedNames.Contains(name) == false)
					availableConstants.Add(name);
			}


			var assignments = new Dictionary<PaperVariable, ConcreteType>();
			foreach (var g in coroutines)
			{
				g.Type.ReplaceWithConstant(availableConstants, assignments);
			}

			if (assignments.Count > 0)
			{
				Console.WriteLine("== ReplaceWithConstant ==");
				Console.WriteLine($"constants: {string.Join(", ", assignments)}");
				foreach (var g in coroutines)
					Console.WriteLine($"{g.Name}:\t{g.Type}");
			}

			return assignments;
		}

		public static GeneratorType Solve(List<Generator> coroutines)
		{
			//multi-pass

			foreach (var g in coroutines)
				Console.WriteLine($"{g.Name}:\t{g.Type}");

			var assignments = RemoveStar(coroutines);
			var constants = assignments.Select(a => a.Key.Name).ToList();

			foreach (var g in coroutines)
				foreach (var v in g.Type.GetUnboundVariables(constants))
					assignments.Add(v, null);

			if (assignments.Any(item => item.Value == null))
				Console.WriteLine("Enter multi-pass stage");


			var concreteTypes = new HashSet<ConcreteType>();
			foreach (var g in coroutines)
				concreteTypes.UnionWith(g.Type.GetConcreteTypes());
			concreteTypes.Remove(ConcreteType.Void);

			//assign concrete types to each free variable
			List<GeneratorType> result = SolveWithFreeVariables(coroutines, assignments.ToList(), 0, concreteTypes.ToList());




			throw new NotImplementedException();//to reduce
			return result[0];
		}


		static List<GeneratorType> SolveWithFreeVariables(List<Generator> pairs, List<KeyValuePair<PaperVariable, ConcreteType>> freeVariables, int assignmentIndex, List<ConcreteType> concreteTypes)
		{
			if (assignmentIndex >= freeVariables.Count)
			{

				Console.WriteLine($"Start to solve with {string.Join(",", freeVariables.Select(fv => fv.Key + "=" + fv.Value))}:");
				List<string> constants = (from fv in freeVariables
										  where fv.Value == ConcreteType.Const
										  select fv.Key.Name).ToList();
				var equations = freeVariables.Where(p => p.Value != ConcreteType.Const).ToDictionary(p => p.Key, p => (PaperWord)p.Value);

				var copy = pairs.Select(p => new Generator(p.Name, p.IsInfinite, p.Type.Clone().ApplyEquation(equations))).ToList();
				return new List<GeneratorType>() { Solve(copy, constants) };
			}


			if (freeVariables[assignmentIndex].Value != null)
				return SolveWithFreeVariables(pairs, freeVariables, assignmentIndex + 1, concreteTypes);
			else
			{
				List<GeneratorType> result = new List<GeneratorType>();
				foreach (var t in concreteTypes)
				{
					freeVariables[assignmentIndex] = new KeyValuePair<PaperVariable, ConcreteType>(freeVariables[assignmentIndex].Key, t);
					result.AddRange(SolveWithFreeVariables(pairs, freeVariables, assignmentIndex + 1, concreteTypes));
				}

				return result;
			}
		}



		static GeneratorType Solve(List<Generator> pairs, List<string> constants)
		{
			List<PaperType> yieldsToOutside = new List<PaperType>();
			//find a generator type where the next type is not void.
			Console.WriteLine();

			int i = 0;
			while (i < pairs.Count)
			{
				//foreach (var p in pairs)
				//	Console.WriteLine($"{p.Key}:\t{p.Value}");


				var coroutine = pairs[i].Type;

				if (coroutine.Normalize() == ConcreteType.Void)
				{
					Generator gx = pairs[i];
					if (gx.IsInfinite)
					{
						Console.WriteLine($"{gx.Name} reached the simplest form. Reset to original.");
						gx.Type = gx.OriginalType.Clone();
					}
					else
					{
						Console.WriteLine($"{gx.Name} reached the simplest form. Remove from the list.");
						pairs.RemoveAt(i);
					}

					continue;
				}




				PaperType yieldedType = null;
				ReceiveGenerator(pairs, constants);

				Console.Write($"{pairs[i].Name}:\t{coroutine} ");

				if (coroutine.Flow[0].Direction == Direction.Yielding)
				{
					yieldedType = coroutine.Flow[0].Type;

					//yieldedType = yieldedType.Normalize();

					coroutine.Flow.RemoveAt(0);
					Console.WriteLine($"--> {coroutine}, yielded: {yieldedType}");

					if (yieldedType is GeneratorType)
					{
						pairs.Insert(i + 1, new Generator("", (GeneratorType)yieldedType));
					}
					//what if nowhere to receive?
					else if (Receive(yieldedType, pairs, constants))
					{
					}
					else
						yieldsToOutside.Add(yieldedType);

					i = 0;
					continue;
				}
				else
					Console.WriteLine(" -- Not ready to yield");

				i++;
			}


			//throw new NotImplementedException();

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
			else
				return new GeneratorType(new SequenceType(yieldsToOutside), ConcreteType.Void);
		}


		static void ReceiveGenerator(List<Generator> pairs, List<string> constants)
		{

			for (var i = 0; i < pairs.Count; i++)
			{
				PaperType head = null;
				PaperType remaining = null;

				if (pairs[i].Type.Flow.Count == 0 || pairs[i].Type.Flow[0].Direction == Direction.Yielding)
					continue;

				pairs[i].Type.Flow[0].Type.Pop(ref head, ref remaining);

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


						try
						{
							//pairs[i].Type.Receive.Pop
							pairs[i].Type.Flow.RemoveAt(0);
							pairs[i].Type = pairs[i].Type.ApplyEquation(conditions);
							Console.Write($"{pairs[i].Name} becomes {pairs[i].Type}");
							Console.Write(" on the conditions that ");
							Console.WriteLine(string.Join(", ", conditions.Select(p => $"{p.Key}/{p.Value}")) + ".");
							foreach (int indice in matches.OrderByDescending(v => v))
								pairs.RemoveAt(indice);

							//Run one more time
							ReceiveGenerator(pairs, constants);
						}
						catch (PaperSyntaxException e)
						{
							Console.WriteLine(e.Message);
						}
					}
				}
			}
		}

		static bool Receive(PaperType yieldedType, List<Generator> pairs, List<string> constants)
		{
			Console.WriteLine();

			for (var i = 0; i < pairs.Count; i++)
			{
				var coroutine = pairs[i].Type;
				if (coroutine.Flow.Count == 0 || coroutine.Flow[0].Direction == Direction.Yielding)
				{
					Console.WriteLine($"{pairs[i].Name}:\t{pairs[i].Type} -- Cannot receive {yieldedType}");
					continue;
				}

				var acceptor = coroutine.Flow[0].Type;
				Dictionary<PaperVariable, PaperWord> conditions = acceptor.IsCompatibleTo(yieldedType);
				if (conditions != null)
				{
					var tmp = coroutine.Clone();
					tmp.Flow.RemoveAt(0);
					GeneratorType newGenerator = tmp.ApplyEquation(conditions);

					Console.Write($"{pairs[i].Name}:\t{coroutine} can receive {yieldedType}");
					if (conditions.Count == 0)
					{
						Console.WriteLine($". Therefore it becomes {newGenerator}.");
						pairs[i].Type = newGenerator;
					}
					else
					{
						Console.Write(" on the conditions that ");
						Console.WriteLine(string.Join(", ", conditions.Select(p => $"{p.Key}/{p.Value}")) + ".");

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
							pairs[i].Type = resultGenerator;
						}
						catch (PaperSyntaxException e)
						{
							Console.WriteLine("But the result doesn't fit the type. " + e.Message);
							continue;
						}
					}

					return true;

					//Solve(pairs, constants);
					//return;
				}
			}

			return false;
		}

		static List<DataFlow> reduce(Queue<DataFlow> x, Queue<DataFlow> y)
		{
			if (x.Count == 0)
			{
				if (y.Count == 0)
					return new List<DataFlow>();
				else if (y.Count == 1)
					return y;
				else
					return new List<DataFlow>() { new DataFlow(Direction.Yielding, new GeneratorType(y)) };
			}


			if (x.Peek().Direction != y.Peek().Direction)
				throw new NotImplementedException();

			if (x.Peek().Type.Equals(y.Peek().Type))
			{
				var result = new List<DataFlow>();
				result.Add(new DataFlow(x.Peek().Direction, x.Peek().Type));
				x.Dequeue();
				y.Dequeue();
				result.AddRange(reduce(x, y));
			}

			throw new NotImplementedException();
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


}
