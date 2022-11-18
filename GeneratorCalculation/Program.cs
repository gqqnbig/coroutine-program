using System;
using System.Collections.Generic;
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

			for (int i = 0; i < pairs.Count; i++)
			{
				var coroutine = pairs[i].Value;

				GeneratorType g = null;
				PaperType yieldedType = null;

				Console.Write($"{coroutine} ");
				if (coroutine.Next(constants, ref g, ref yieldedType))
				{
					Console.WriteLine($"--> {g}, yielded: {yieldedType}");
				}
				else
					Console.WriteLine(" --X");
			}

		}

		static void RunNext(GeneratorType coroutine)
		{
			if (coroutine.Yield == null)
				throw new Exception();

			if (coroutine.Yield == ConcreteType.Void)
				return;


		}
	}
}
