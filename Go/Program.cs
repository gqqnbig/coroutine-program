using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using GeneratorCalculation;

namespace Go
{
	class Program
	{
		static void Main(string[] args)
		{
			string path = @"E:\GeneratorCalculation\Go\tests\channels.go";
			var stream = CharStreams.fromPath(path);
			GoLang.Antlr.GoLexer lexer = new GoLang.Antlr.GoLexer(stream);
			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GoLang.Antlr.GoParser parser = new GoLang.Antlr.GoParser(tokens);

			var tree = parser.sourceFile();

			//Console.WriteLine(tree.children[1].GetText());


			CoroutineDefinitionCollector v = new CoroutineDefinitionCollector();
			v.Visit(tree);


			GoStatementListener l = new GoStatementListener(v.definitions);
			ParseTreeWalker walker = new ParseTreeWalker();
			walker.Walk(l, tree);
		}
	}
}
