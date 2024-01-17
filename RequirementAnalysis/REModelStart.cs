using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using DiffSyntax.Antlr;
using System.Linq;
using GeneratorCalculation;
using Z3 = Microsoft.Z3;

namespace RequirementAnalysis
{
	public class REModelStart
	{
		static void Main(string[] args)
		{
			List<Generator> generators = new List<Generator>();

			InheritanceCondition ic = new InheritanceCondition((PaperVariable)"a", (ConcreteType)"Animal");
			generators.Add(new Generator("", new CoroutineInstanceType(ic, (PaperVariable)"a", ConcreteType.Void)));

			generators.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Apple")));
			generators.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, (ConcreteType)"Dog")));
			generators.Add(new Generator("", new CoroutineInstanceType((ConcreteType)"B", (ConcreteType)"A")));
			generators.Add(new Generator("", new CoroutineInstanceType((ConcreteType)"A", (ConcreteType)"B")));

			var solver = new Solver();
			var inheritance = new Dictionary<string, string>();
			inheritance.Add("Dog", "Animal");
			solver.CollectConcreteTypes(generators, null);
			InheritanceCondition.BuildFunction(solver, inheritance, out var func, out var funcBody);
			solver.AddZ3Function(func, funcBody);

			var result = solver.SolveWithBindings(generators);
		}


		public static CoroutineInstanceType Compose(List<Generator> generators, Dictionary<string, string> inheritance, string[] interestedCoroutines = null, string[] lowPriorityCoroutines = null)
		{
			List<Generator> filtered;
			if (interestedCoroutines != null)
				filtered = generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1).ToList();
			else
				filtered = generators;

			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in filtered)
				bindings.Add(g.Name, g.Type);

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, new TupleType(from b in bindings select b.Key))));
			if (lowPriorityCoroutines != null)
				coroutines.AddRange(generators.Where(g => Array.IndexOf(lowPriorityCoroutines, g.Name) != -1));


			var solver = new Solver();
			List<string> typesInInheritance = new List<string>();
			foreach (var item in inheritance)
			{
				typesInInheritance.Add(item.Key);
				typesInInheritance.Add(item.Value);
			}

			solver.CollectConcreteTypes(coroutines, bindings, typesInInheritance);
			InheritanceCondition.BuildFunction(solver, inheritance, out var func, out var funcBody);
			solver.AddZ3Function(func, funcBody);

			return solver.SolveWithBindings(coroutines, bindings);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <returns>subclass : superclass</returns>
		public static Dictionary<string, string> GetObjectInheritance(string content)
		{
			var result = new Dictionary<string, string>();
			foreach (Match m in Regex.Matches(content, @"Actor\s+(\w+)\s+extends\s+(\w+)"))
			{
				result.Add(m.Groups[1].Value, m.Groups[2].Value);
			}

			return result;
		}

		public static List<Generator> GetAllGenerators(string content, Dictionary<string, string> inheritance)
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
				var g = ContractAnalyzer.GetGenerator(serviceDefinitions, sectionContent, inheritance);
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
