using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using DiffSyntax.Antlr;
using GeneratorCalculation;

namespace RequirementAnalysis
{
	public class ContractAnalyzer
	{
		public static Generator GetGenerator(Dictionary<string, ServiceBlock> serviceDefinitions, string code, Dictionary<string, string> inheritance)
		{
			AntlrInputStream inputStream = new AntlrInputStream(code);
			REModelLexer lexer = new REModelLexer(inputStream);
			CommonTokenStream tokens = new CommonTokenStream(lexer);
			REModelParser parser = new REModelParser(tokens);
			REModelParser.ContractDefinitionContext tree = parser.contractDefinition();
			return ProcessContract(serviceDefinitions, tree, inheritance);
		}


		static Generator ProcessContract(Dictionary<string, ServiceBlock> serviceDefinitions, REModelParser.ContractDefinitionContext tree, Dictionary<string, string> inheritance)
		{
			string className = tree.ID(0).GetText();
			string methodName = tree.ID(1).GetText();


			//Console.WriteLine($"{className}::{methodName}");
			var service = serviceDefinitions[className];
			var global = serviceDefinitions.Values.First(d => d.Name.EndsWith("System"));

			//var classDef = serviceDefinitions.Values.First(d => d.Name == className);

			//if (methodName != "makeNewOrder")
			//	return;

			var parameters = ParametersCollector.CollectParameters(tree.parameterDeclarations());

			Dictionary<string, string> definitions = new Dictionary<string, string>();
			if (tree.definitions() != null)
				foreach (var def in tree.definitions().definition())
					definitions.Add(def.ID().GetText(), def.type().GetText());


			var c = new ReceiveCollector(definitions, parameters, service.Properties, global.Properties);
			c.Visit(tree.precondition());

			//Console.WriteLine("- receive: " + string.Join(", ", c.ReceiveList));

			List<PaperType> receiveList = c.GetReceiveList();
			List<PaperType> yieldList = YieldCollector.GetYieldList(definitions, parameters, service.Properties, global.Properties, receiveList, tree.postcondition());
			//Console.WriteLine("- yield: " + string.Join(", ", yieldList));
			//Console.WriteLine();

			List<PaperType> receiveList2;
			List<PaperType> yieldList2;
			var condition = ReplaceSuperclasses(inheritance, receiveList, yieldList, out receiveList2, out yieldList2);


			CoroutineType ct;
			if (condition == null)
				ct = new CoroutineType(new SequenceType(receiveList), new SequenceType(yieldList), $"{className}::{methodName}");
			else
				ct = new CoroutineType(condition, new SequenceType(receiveList2), new SequenceType(yieldList2), $"{className}::{methodName}");

			var g = new Generator($"{className}::{methodName}", ct);
			var n = g.Type.Normalize();
			if (n == ConcreteType.Void)
				return null;
			else
			{
				g.Type = (GeneratorType)n;
				return g;
			}
		}


		static List<string> GetSubclasses(Dictionary<string, string> inheritance, string superclass)
		{
			var matches = inheritance.Where(pair => pair.Value == superclass)
				.Select(pair => pair.Key);
			return matches.ToList();
		}


		static Condition ReplaceSuperclasses(Dictionary<string, string> inheritance,
										List<PaperType> receiveList, List<PaperType> yieldList,
										out List<PaperType> receiveList2, out List<PaperType> yieldList2)
		{
			int usedVariable = 0;
			Dictionary<string, char> variableMapping = new Dictionary<string, char>();
			receiveList2 = new List<PaperType>(receiveList);
			for (var i = 0; i < receiveList.Count; i++)
			{
				var tt = receiveList[i];
				ConcreteType t = tt as ConcreteType;
				if (t == null)
					continue;

				var subclasses = GetSubclasses(inheritance, t.Name);
				if (subclasses.Count == 0)
					continue;

				if (variableMapping.TryGetValue(t.Name, out char v) == false)
				{
					v = (char)((int)'a' + usedVariable);
					variableMapping[t.Name] = v;
					usedVariable++;
				}

				receiveList2[i] = new PaperVariable(v.ToString());
			}


			yieldList2 = new List<PaperType>(yieldList);
			for (int i = 0; i < yieldList.Count; i++)
			{
				var t = yieldList[i] as ConcreteType;
				if (t == null)
					continue;

				if (variableMapping.TryGetValue(t.Name, out char v))
					yieldList2[i] = new PaperVariable(v.ToString());
			}


			var em = variableMapping.GetEnumerator();
			if (em.MoveNext() == false)
				return null;

			Condition c = new InheritanceCondition { Subclass = new PaperVariable(em.Current.Value.ToString()), Superclass = new ConcreteType(em.Current.Key) };
			if (em.MoveNext() == false)
				return c;

			Condition c2 = new InheritanceCondition { Subclass = new PaperVariable(em.Current.Value.ToString()), Superclass = new ConcreteType(em.Current.Key) };
			c = new AndCondition { Condition1 = c, Condition2 = c2 };
			if (em.MoveNext() == false)
				return c;

			do
			{
				c = new AndCondition
				{
					Condition1 = c,
					Condition2 = new InheritanceCondition { Subclass = new PaperVariable(em.Current.Value.ToString()), Superclass = new ConcreteType(em.Current.Key) }
				};

			} while (em.MoveNext());

			return c;
		}

	}
}
