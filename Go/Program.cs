using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using GeneratorCalculation;

namespace Go
{
	class Program
	{
		static void Main(string[] args)
		{
			string path = @"E:\GeneratorCalculation\Go\tests\channels-deadlock.go";
			var stream = CharStreams.fromPath(path);
			GoLang.Antlr.GoLexer lexer = new GoLang.Antlr.GoLexer(stream);
			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GoLang.Antlr.GoParser parser = new GoLang.Antlr.GoParser(tokens);

			var tree = parser.sourceFile();

			//Console.WriteLine(tree.children[1].GetText());


			CoroutineDefinitionCollector v = new CoroutineDefinitionCollector();
			v.Visit(tree);

			Dictionary<string, CoroutineDefinitionType> definitions = v.definitions;
			GoStatementListener l = new GoStatementListener(definitions);
			ParseTreeWalker walker = new ParseTreeWalker();
			walker.Walk(l, tree);

			var instances = l.instanceTypes;
			Console.WriteLine("This program will create the following coroutine instances:");
			foreach (var item in l.instanceTypes)
			{
				Console.WriteLine(item);
			}

			if (definitions.ContainsKey("main"))
				instances.Add(definitions["main"].Start());


			var gs = from i in instances
					 select new Generator("", i);

			var result = new Solver().SolveWithBindings(gs.ToList());

			Console.WriteLine("Composition result is " + result);

			if (result.Receive != ConcreteType.Void)
				Console.WriteLine("The program requires one additional {0} to complete execution.", result.Receive);

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
