using Antlr4.Runtime.Misc;
using GoLang.Antlr;
using System;
using System.Collections.Generic;

namespace Go
{
	class MakeChannelVisitor : GoParserBaseVisitor<bool>
	{
		public string type = null;
		private Dictionary<string, FuncInfo> definitions;

		public MakeChannelVisitor(Dictionary<string, FuncInfo> definitions)
		{
			if (definitions == null)
				throw new ArgumentNullException(nameof(definitions));
			this.definitions = definitions;
		}

		public override bool VisitPrimaryExpr([NotNull] GoParser.PrimaryExprContext context)
		{
			string text = context.primaryExpr()?.GetText();

			if (text == "make")
			{
				type = ParameterTypeVisitor.GetChannelType(context.arguments().type_());
				return true;
			}
			else if (text != null && definitions.TryGetValue(text, out var funcInfo) && funcInfo.ChannelType != null)
			{
				type = funcInfo.ChannelType;
				return true;
			}

			return base.VisitPrimaryExpr(context);
		}
	}
}
