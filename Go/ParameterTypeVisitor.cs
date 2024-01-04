using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using GoLang.Antlr;

namespace Go
{
	class ParameterTypeVisitor: GoParserBaseVisitor<bool>
	{
		public Dictionary<string, string> channelTypes = new Dictionary<string, string>();

		public override bool VisitParameterDecl([NotNull] GoParser.ParameterDeclContext context)
		{
			string ct = context.type_().typeLit()?.channelType()?.elementType().GetText();
			if (ct != null)
			{
				var ids = context.identifierList().IDENTIFIER();
				foreach (var id in ids)
				{
					channelTypes.Add(id.GetText(), ct);
				}

			}

			return true;
		}
	}
}
