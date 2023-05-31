using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using DiffSyntax.Antlr;
using GeneratorCalculation;

namespace SmartContractAnalysis
{
	class YieldCollector : REModelBaseVisitor<bool>
	{
		private Dictionary<string, string> letVariables = new Dictionary<string, string>();

		public List<ConcreteType> YieldList { get; } = new List<ConcreteType>();

		public override bool VisitLetExpression([NotNull] REModelParser.LetExpressionContext context)
		{
			letVariables.Add(context.ID().GetText(), context.type().GetText());
			return VisitExpression(context.expression());
		}

		public override bool VisitEqualityExpression([NotNull] REModelParser.EqualityExpressionContext context)
		{
			// obj.oclIsNew() = true || obj.oclIsNew()
			if (context.additiveExpression().Length == 1 ||
				context.additiveExpression(1).GetText() == "true" && context.GetChild(1).GetText() == "=")
			{
				var text = context.additiveExpression(0).GetText();
				if (text.EndsWith(".oclIsNew()"))
				{
					var obj = text.Substring(0, text.Length - ".oclIsNew()".Length);

					// The operation oclIsNew evaluates to true if, used in a postcondition, the object is created during performing the operation 
					// (i.e., it didn't exist at precondition time). 
					// from "The Object Constraint Language Specification" chapter 7.4
					if (letVariables.ContainsKey(obj))
						YieldList.Add(letVariables[obj]);
					else
						throw new FormatException($"{obj} is not defined in the let expression.");
				}
			}


			return base.VisitEqualityExpression(context);
		}
	}
}
