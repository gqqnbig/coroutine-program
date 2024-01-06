using Antlr4.Runtime.Misc;
using GoLang.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace Go
{
	class MakeChannelVisitor : GoParserBaseVisitor<bool>
	{
		public string type = null;

		public override bool VisitPrimaryExpr([NotNull] GoParser.PrimaryExprContext context)
		{
			if (context.primaryExpr()?.GetText() == "make")
			{
				type = ParameterTypeVisitor.GetChannelType(context.arguments().type_());
				return true;
			}
			return base.VisitPrimaryExpr(context);
		}
	}
}
