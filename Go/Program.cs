using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using GeneratorCalculation;

namespace Go
{
	public class Program
	{

		static void Main(string[] args)
		{
			string path = null;
			if (args.Length > 0)
				path = args[args.Length - 1];

			//path = @"E:\GeneratorCalculation\GoTests\basic.go";

			string code = System.IO.File.ReadAllText(path);
			CheckDeadlock(code);

		}

		public static Dictionary<string, CoroutineDefinitionType> GetDefinitions(string goCode)
		{
			AntlrInputStream inputStream = new AntlrInputStream(goCode);

			GoLang.Antlr.GoLexer lexer = new GoLang.Antlr.GoLexer(inputStream);
			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GoLang.Antlr.GoParser parser = new GoLang.Antlr.GoParser(tokens);


			var tree = parser.sourceFile();

			//Console.WriteLine(tree.children[1].GetText());

			var definitions = new Dictionary<string, FuncInfo>();

			// repeat and check if definitions update.
			for (int i = 0; ; i++)
			{
				CoroutineDefinitionCollector v = new CoroutineDefinitionCollector(definitions);
				v.Visit(tree);

				if (Equals(v.definitions, definitions))
					break; // We reach invariant point.
				definitions = v.definitions;

				Console.WriteLine("Iterate {0} and check convergence", i);
			}

			return definitions.ToDictionary(i => i.Key, i => i.Value.CoroutineType);
		}


		public static bool CheckDeadlock(string goCode)
		{
			Dictionary<string, CoroutineDefinitionType> definitions = GetDefinitions(goCode);
			return CheckDeadlock(definitions);
		}


		public static bool CheckDeadlock(Dictionary<string, CoroutineDefinitionType> definitions)
		{
			List<CoroutineInstanceType> instances = new List<CoroutineInstanceType>();
			if (definitions.ContainsKey("main"))
			{
				var m = definitions["main"].Start("main");
				instances.Add(m);

				var bindings = new Dictionary<PaperVariable, PaperWord>();
				foreach (var d in definitions)
				{
					bindings.Add(d.Key, d.Value);
				}


				var gs = from i in instances
						 select new Generator(i.Source.ToString(), i);

				try
				{
					Solver solver = new Solver();
					solver.CanLoopExternalYield = false;
					solver.MainCoroutine = "main";
					var result = solver.SolveWithBindings(gs.ToList(), bindings, 50);

					Console.WriteLine("Composition result is " + result);

					var additional = result.Flow.FirstOrDefault(f => f.Direction == Direction.Resuming);
					if (additional != null)
					{
						Console.WriteLine("The program requires {0} to complete execution.", additional.Type);
						return true;
					}
				}
				catch (DeadLockException ex)
				{
					Console.WriteLine(ex.Message);
					return true;
				}
			}

			return false;
		}


		static bool Equals<K, V>(Dictionary<K, V> d1, Dictionary<K, V> d2)
		{
			if (d1.Count != d2.Count)
				return false;

			bool res = d1.All(
				 d1KV =>
				 {
					 V d2Value;
					 return d2.TryGetValue(d1KV.Key, out d2Value) && (
										   //d1KV.Value == d2Value ||
										   d1KV.Value?.Equals(d2Value) == true);
				 });
			return res;
		}

	}
}
