using System;
using System.Collections.Generic;
using System.Text;
using DiffSyntax.Antlr;

namespace SmartContractAnalysis
{
	class BooleanUtils
	{
		public static REModelParser.AdditiveExpressionContext SomethingIsTrue(REModelParser.EqualityExpressionContext context)
		{
			if (context.additiveExpression().Length == 1)
				return context.additiveExpression(0);

			if (context.GetChild(1).GetText() == "=")
			{
				if (context.additiveExpression(1).GetText() == "true")
					return context.additiveExpression(0);
				else
					return context.additiveExpression(1);
			}
			return null;
		}

		public static REModelParser.AdditiveExpressionContext SomethingIsFalse(REModelParser.EqualityExpressionContext context)
		{
			if (context.additiveExpression().Length == 2 && context.GetChild(1).GetText() == "=")
			{
				if (context.additiveExpression(1).GetText() == "false")
					return context.additiveExpression(0);
				else if(context.additiveExpression(0).GetText() == "false")
					return context.additiveExpression(1);
			}

			return null;
		}

	}
}
