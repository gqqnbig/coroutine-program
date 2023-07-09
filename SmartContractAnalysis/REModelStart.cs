using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
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

				"ProcessSaleService::makeCashPayment",
				"ProcessSaleService::makeCardPayment",

			};
			//string[] lowPriorityCoroutines = { "ManageItemCRUDService::deleteItem" };

			foreach (var g in generators)
			{
				Console.WriteLine($"{g.Name}:\t{g.Type}");
			}

			Console.WriteLine("\nNow, let's compose interested coroutines.");



			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1))
			{
				bindings.Add(g.Name, g.Type);
			}

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new GeneratorType(new TupleType(from b in bindings select b.Key), ConcreteType.Void)));
			//coroutines.AddRange(generators.Where(g => Array.IndexOf(lowPriorityCoroutines, g.Name) != -1));



			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Console.WriteLine(result);


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
				var g = ContractAnalyzer.GetGenerator(serviceDefinitions, sectionContent);
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


	}

}
