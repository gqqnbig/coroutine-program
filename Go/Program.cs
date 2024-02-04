using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using GeneratorCalculation;

namespace Go
{
	public class Program
	{

		static void Main(string[] args)
		{
			string path = @"E:\GeneratorCalculation\GoTests\channels-method.go";

			string code = System.IO.File.ReadAllText(path);
			CheckDeadlock(code);

		}


		public static bool CheckDeadlock(string goCode)
		{
			AntlrInputStream inputStream = new AntlrInputStream(goCode);

			GoLang.Antlr.GoLexer lexer = new GoLang.Antlr.GoLexer(inputStream);
			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GoLang.Antlr.GoParser parser = new GoLang.Antlr.GoParser(tokens);


			var tree = parser.sourceFile();

			//Console.WriteLine(tree.children[1].GetText());

			Dictionary<string, CoroutineDefinitionType> definitions = new Dictionary<string, CoroutineDefinitionType>();

			// repeat and check if definitions update.
			for (int i = 0; ; i++)
			{
				CoroutineDefinitionCollector v = new CoroutineDefinitionCollector(definitions);
				v.Visit(tree);

				if (Equals(v.definitions, definitions))
					break;
				definitions = v.definitions;

				Console.WriteLine("Iterate {0} and check convergence", i);
			}


			List<CoroutineInstanceType> instances = new List<CoroutineInstanceType>();
			if (definitions.ContainsKey("main"))
			{
				instances.Add(definitions["main"].Start());

				var bindings = new Dictionary<PaperVariable, PaperWord>();
				foreach (var d in definitions)
				{
					bindings.Add(d.Key, d.Value);
				}


				var gs = from i in instances
						 select new Generator("", i);
				var result = new Solver().SolveWithBindings(gs.ToList(), bindings);

				Console.WriteLine("Composition result is " + result);

				var additional = result.Flow.FirstOrDefault(f => f.Direction == Direction.Resuming);
				if (additional != null)
				{
					Console.WriteLine("The program requires {0} to complete execution.", additional.Type);
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
