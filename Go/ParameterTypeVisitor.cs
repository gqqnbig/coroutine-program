using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using GoLang.Antlr;

namespace Go
{
	class ParameterTypeVisitor : GoParserBaseVisitor<bool>
	{
		public Dictionary<string, string> channelTypes = new Dictionary<string, string>();

		public override bool VisitParameterDecl([NotNull] GoParser.ParameterDeclContext context)
		{
			string ct = GetChannelType(context.type_());
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

		/// <summary>
		/// Returns the type of the channel. 
		/// If it's not a channel type, return null.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static string GetChannelType(GoParser.Type_Context context)
		{
			string ct = context.typeLit()?.channelType()?.elementType().GetText();
			return ct;
		}
	}
}
