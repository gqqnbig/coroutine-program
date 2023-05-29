using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using DiffSyntax.Antlr;
using GeneratorCalculation;

namespace SmartContractAnalysis
{
	class ReceiveCollector : REModelBaseVisitor<bool>
	{
		private readonly Dictionary<string, string> definitions;

		public List<ConcreteType> ReceiveList { get; } = new List<ConcreteType>();

		public ReceiveCollector(Dictionary<string, string> definitions)
		{
			this.definitions = definitions;
		}

		public override bool VisitEqualityExpression([NotNull] REModelParser.EqualityExpressionContext context)
		{
			if (context.additiveExpression(1) != null &&
				context.GetChild(1).GetText() == "=")
			{
				var text = context.additiveExpression(0).GetText();
				if (text.EndsWith(".oclIsUndefined()"))
				{
					var obj = text.Substring(0, text.Length - ".oclIsUndefined()".Length);

					Debug.Assert(definitions.ContainsKey(obj));

					ReceiveList.Add(definitions[obj]);
				}
			}


			return base.VisitEqualityExpression(context);
		}
	}
}
