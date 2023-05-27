using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DiffSyntax.Antlr;

namespace SmartContractAnalysis
{
	class REModelStart
	{
		static void Main(string[] args)
		{
			// Step 1: Load the file content into a string.
			string path = @"D:\rm2pt\CaseStudies\CoCoME\RequirementsModel\cocome.remodel";
			string content = File.ReadAllText(path);

			int startPosition = 0;
			// Step 2: Find the specific section that you want to parse.
			string contractMarker = "Contract ";
			while (true)
			{
				int contractIndex = content.IndexOf(contractMarker, startPosition);
				if (contractIndex < 0)
				{
					Console.WriteLine("Contract not found.");
					return;
				}

				int sectionEndIndex = content.IndexOf("}", contractIndex);
				if (sectionEndIndex < 0)
				{
					Console.WriteLine("Section end not found.");
					return;
				}

				string sectionContent = content.Substring(contractIndex, sectionEndIndex - contractIndex + 1);

				// Step 3: Parse the section using the Antlr4 parser.
				AntlrInputStream inputStream = new AntlrInputStream(sectionContent);
				REModelLexer lexer = new REModelLexer(inputStream);
				CommonTokenStream tokens = new CommonTokenStream(lexer);
				REModelParser parser = new REModelParser(tokens);
				REModelParser.ContractDefinitionContext tree = parser.contractDefinition();
				string className = tree.ID(0).GetText();
				string methodName = tree.ID(1).GetText();


				Console.WriteLine($"{className}::{methodName}");

				startPosition = sectionEndIndex;
			}
		}
	}

}
