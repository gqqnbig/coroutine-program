using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DiffSyntax.Antlr;
using System.Linq;
using GeneratorCalculation;

namespace SmartContractAnalysis
{
	class REModelStart
	{
		static void Main(string[] args)
		{
			// Step 1: Load the file content into a string.
			string path = @"D:\rm2pt\CaseStudies\CoCoME\RequirementsModel\cocome.remodel";
			string content = File.ReadAllText(path);

			var generators = GetAllGenerators(content);

			string[] interestedCoroutines =
			{
				"CoCoMESystem::openStore",
				"CoCoMESystem::openCashDesk",
				"ProcessSaleService::makeNewSale",
				"ProcessSaleService::enterItem",
				"ManageStoreCRUDService::createStore",
				"ManageCashDeskCRUDService::createCashDesk",
				"ManageItemCRUDService::createItem",
			};

			foreach (var g in generators)
			{
				Console.WriteLine($"{g.Name}:\t{g.Type}");
			}

			Console.WriteLine("\nNow, let's compose interested coroutines.");

			var ig = generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1).ToList();

			Solver.Solve(ig);



		}

		private static List<Generator> GetAllGenerators(string content)
		{
			Dictionary<string, ServiceBlock> serviceDefinitions = CollectProperties(content).ToDictionary(d => d.Name);

			List<Generator> generators = new List<Generator>();
			int startPosition = 0;
			// Step 2: Find the specific section that you want to parse.
			string marker = "Contract ";
			while (true)
			{
				int contractIndex = content.IndexOf(marker, startPosition);
				if (contractIndex < 0)
					break;

				int sectionEndIndex = content.IndexOf("}", contractIndex);
				if (sectionEndIndex < 0)
				{
					Console.WriteLine("Section end not found.");
					return generators;
				}

				string sectionContent = content.Substring(contractIndex, sectionEndIndex - contractIndex + 1);

				// Step 3: Parse the section using the Antlr4 parser.
				AntlrInputStream inputStream = new AntlrInputStream(sectionContent);
				REModelLexer lexer = new REModelLexer(inputStream);
				CommonTokenStream tokens = new CommonTokenStream(lexer);
				REModelParser parser = new REModelParser(tokens);
				REModelParser.ContractDefinitionContext tree = parser.contractDefinition();
				var g = ProcessContract(serviceDefinitions, tree);
				if (g != null)
					generators.Add(g);

				startPosition = sectionEndIndex;
			}

			return generators;
		}


		static List<ServiceBlock> CollectProperties(string code)
		{
			string servicePattern = @"Service\s+(\w+)\s*\{(.+?)\}";
			string propertyPattern = @"\[TempProperty\](.+)\n";

			List<ServiceBlock> serviceDefinitions = new List<ServiceBlock>();
			while (code.Length > 0)
			{
				Match m = Regex.Match(code, servicePattern, RegexOptions.Singleline);

				if (!m.Success)
					break;

				ServiceBlock service = new ServiceBlock();
				serviceDefinitions.Add(service);
				service.Name = m.Groups[1].Value;

				string serviceBlock = m.Groups[2].Value;

				Match propertyMatch = Regex.Match(serviceBlock, propertyPattern, RegexOptions.Singleline);

				if (propertyMatch.Success)
				{

					string propertyBlock = propertyMatch.Groups[1].Value;

					foreach (Match match in Regex.Matches(propertyBlock, @"(\w+)\s*:\s*(\w+)"))
					{
						service.Properties[match.Groups[1].Value] = match.Groups[2].Value;
						//Console.WriteLine($"{match.Groups[1].Value}: {match.Groups[2].Value}");
					}
				}

				code = code.Substring(m.Index + m.Length);
			}

			return serviceDefinitions;
		}



		static Generator ProcessContract(Dictionary<string, ServiceBlock> serviceDefinitions, REModelParser.ContractDefinitionContext tree)
		{
			string className = tree.ID(0).GetText();
			string methodName = tree.ID(1).GetText();


			//Console.WriteLine($"{className}::{methodName}");
			var service = serviceDefinitions[className];
			var global = serviceDefinitions.Values.First(d => d.Name.EndsWith("System"));

			var classDef = serviceDefinitions.Values.First(d => d.Name == className);

			//if (methodName != "makeNewOrder")
			//	return;

			Dictionary<string, string> definitions = new Dictionary<string, string>();
			if (tree.definitions() != null)
				foreach (var def in tree.definitions().definition())
					definitions.Add(def.ID().GetText(), def.type().GetText());


			var c = new ReceiveCollector(definitions, service.Properties, global.Properties);
			c.Visit(tree.precondition());

			//Console.WriteLine("- receive: " + string.Join(", ", c.ReceiveList));

			var yieldList = YieldCollector.GetYieldList(definitions, classDef.Properties, global.Properties, c.ReceiveList, tree.postcondition());
			//Console.WriteLine("- yield: " + string.Join(", ", yieldList));
			//Console.WriteLine();

			var g = new Generator($"{className}::{methodName}", new GeneratorType(new SequenceType(yieldList), new SequenceType(c.ReceiveList)));
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
