using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using DiffSyntax.Antlr;

namespace SmartContractAnalysis
{
	class ParametersCollector : REModelBaseVisitor<bool>
	{
		public static HashSet<string> CollectParameters(REModelParser.ParameterDeclarationsContext context)
		{
			if (context == null)
				return new HashSet<string>();

			var c = new ParametersCollector();
			c.VisitParameterDeclarations(context);
			return c.Parameters;
		}


		public HashSet<string> Parameters { get; } = new HashSet<string>();

		public override bool VisitParameterDeclaration([NotNull] REModelParser.ParameterDeclarationContext context)
		{
			Parameters.Add(context.ID().GetText());
			return true;
		}
	}
}
