using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using DiffSyntax.Antlr;
using GeneratorCalculation;
using System.Text.RegularExpressions;
using System.Linq;

namespace SmartContractAnalysis
{
	/// <summary>
	/// This class collects the yielding types from the post condition section from a service definition.
	/// </summary>
	class YieldCollector : REModelBaseVisitor<bool>
	{

		public static List<ConcreteType> GetYieldList(Dictionary<string, string> localVariables, Dictionary<string, string> classProperties, Dictionary<string, string> globalProperties, 
														List<ConcreteType> receiveList, REModelParser.PostconditionContext context)
		{

			var post = context.expression();
			var y = new YieldCollector(localVariables, classProperties,globalProperties);
			y.Visit(post);

			List<ConcreteType> yieldList = new List<ConcreteType>(receiveList);
			yieldList.AddRange(y.ElementsAddedModified);
			foreach (var t in y.PropertiesModified.OrderBy(c => c.Name))
			{
				if (yieldList.Contains(t) == false)
					yieldList.Add(t);
			}
			foreach (var t in y.ElementsRemoved)
			{
				yieldList.Remove(t);
			}

			return yieldList;
		}


		private readonly Dictionary<string, string> localVariables;
		private readonly Dictionary<string, string> classProperties;
		private readonly Dictionary<string, string> globalProperties;
		private readonly Dictionary<string, string> letVariables = new Dictionary<string, string>();

		private YieldCollector(Dictionary<string, string> localVariables, Dictionary<string, string> classProperties, Dictionary<string, string> globalProperties)
		{
			this.localVariables = localVariables;
			this.classProperties = classProperties;
			this.globalProperties = globalProperties;
		}

		List<ConcreteType> ElementsAddedModified { get; } = new List<ConcreteType>();
		List<ConcreteType> ElementsRemoved { get; } = new List<ConcreteType>();
		HashSet<ConcreteType> PropertiesModified { get; } = new HashSet<ConcreteType>();


		public override bool VisitLetExpression([NotNull] REModelParser.LetExpressionContext context)
		{
			letVariables.Add(context.ID().GetText(), context.type().GetText());
			return VisitExpression(context.expression());
		}


		public override bool VisitEqualityExpression([NotNull] REModelParser.EqualityExpressionContext context)
		{
			// obj.oclIsNew() = true || obj.oclIsNew()
			var exp = BooleanUtils.SomethingIsTrue(context);
			if (exp != null)
			{
				var text = exp.GetText();
				if (text.EndsWith(".oclIsNew()"))
				{
					var obj = text.Substring(0, text.Length - ".oclIsNew()".Length);

					// The operation oclIsNew evaluates to true if, used in a postcondition, the object is created during performing the operation 
					// (i.e., it didn't exist at precondition time). 
					// from "The Object Constraint Language Specification" chapter 7.4
					if (letVariables.ContainsKey(obj))
						ElementsAddedModified.Add(letVariables[obj]);
					else
						throw new FormatException($"{obj} is not defined in the let expression.");

					// We are also sure that there's a condition like
					// allInstance()->includes(obj)
				}
				else if (text.EndsWith(".oclIsUndefined()"))
				{
					var obj = text.Substring(0, text.Length - ".oclIsUndefined()".Length);
					ElementsRemoved.Add(obj);
				}




				Regex regex = new Regex(@"->excludes\((\w+)\)$");
				var m = regex.Match(text);
				if (m.Success)
				{
					var removed = m.Groups[1].Value;
					if (localVariables.ContainsKey(removed))
						ElementsRemoved.Add(localVariables[removed]);
					else
						throw new NotImplementedException($"{removed} is to be removed, but it's not defined locally.");
				}

			}

			//REModel language assumes left value is assignable
			if (context.additiveExpression().Length == 2 && context.GetChild(1).GetText() == "=")
			{
				var left = context.additiveExpression(0).GetText();
				var components = new List<string>(left.Split('.'));
				if (components[0] == "self")
					components.RemoveAt(0);


				if (globalProperties.ContainsKey(components[0]) || classProperties.ContainsKey(components[0]))
					PropertiesModified.Add(components[0]);
			}




			return base.VisitEqualityExpression(context);
		}
	}
}
