using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Go
{
	class Program
	{
		static void Main(string[] args)
		{
			string path = @"E:\GeneratorCalculation\Go\tests\func.go";
			var stream = CharStreams.fromPath(path);
			GoLang.Antlr.GoLexer lexer = new GoLang.Antlr.GoLexer(stream);
			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GoLang.Antlr.GoParser parser = new GoLang.Antlr.GoParser(tokens);

			var tree = parser.sourceFile();

			//Console.WriteLine(tree.ChildCount);
			Console.WriteLine("Hello World!");

			GoStatementListener l = new GoStatementListener();
			ParseTreeWalker walker = new ParseTreeWalker();
			walker.Walk(l, tree);
		}
	}
}
