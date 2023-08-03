using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using DiffSyntax.Antlr;
using GeneratorCalculation;

namespace SmartContractAnalysis
{
	public class ContractAnalyzer
	{
		public static Generator GetGenerator(Dictionary<string, ServiceBlock> serviceDefinitions, string code)
		{
			AntlrInputStream inputStream = new AntlrInputStream(code);
			REModelLexer lexer = new REModelLexer(inputStream);
			CommonTokenStream tokens = new CommonTokenStream(lexer);
			REModelParser parser = new REModelParser(tokens);
			REModelParser.ContractDefinitionContext tree = parser.contractDefinition();
			return ProcessContract(serviceDefinitions, tree);
		}


		static Generator ProcessContract(Dictionary<string, ServiceBlock> serviceDefinitions, REModelParser.ContractDefinitionContext tree)
		{
			string className = tree.ID(0).GetText();
			string methodName = tree.ID(1).GetText();


			//Console.WriteLine($"{className}::{methodName}");
			var service = serviceDefinitions[className];
			var global = serviceDefinitions.Values.First(d => d.Name.EndsWith("System"));

			//var classDef = serviceDefinitions.Values.First(d => d.Name == className);

			//if (methodName != "makeNewOrder")
			//	return;

			Dictionary<string, string> definitions = new Dictionary<string, string>();
			if (tree.definitions() != null)
				foreach (var def in tree.definitions().definition())
					definitions.Add(def.ID().GetText(), def.type().GetText());


			var c = new ReceiveCollector(definitions, service.Properties, global.Properties);
			c.Visit(tree.precondition());

			//Console.WriteLine("- receive: " + string.Join(", ", c.ReceiveList));

			var receiveList = c.GetReceiveList();
			var yieldList = YieldCollector.GetYieldList(definitions, service.Properties, global.Properties, receiveList, tree.postcondition());
			//Console.WriteLine("- yield: " + string.Join(", ", yieldList));
			//Console.WriteLine();

			var g = new Generator($"{className}::{methodName}", new CoroutineType(new SequenceType(receiveList), new SequenceType(yieldList), $"{className}::{methodName}"));
			var n = g.Type.Normalize();
			if (n == ConcreteType.Void)
				return null;
			else
			{
				g.Type = (GeneratorType)n;
				return g;
			}
		}


	}
}
