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
		private readonly Dictionary<string, string> localVariables;
		private readonly Dictionary<string, string> properties;
		private readonly Dictionary<string, string> globalProperties;

		public List<ConcreteType> ReceiveList { get; } = new List<ConcreteType>();

		public ReceiveCollector(Dictionary<string, string> localVariables,
								Dictionary<string, string> properties,
								Dictionary<string, string> globalProperties
								)
		{
			this.localVariables = localVariables;
			this.properties = properties;
			this.globalProperties = globalProperties;
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

					//Debug.Assert(definitions.ContainsKey(obj));
					if (localVariables.ContainsKey(obj))
						ReceiveList.Add(localVariables[obj]);
					else if (properties.ContainsKey(obj))
						ReceiveList.Add(properties[obj]);
					else if (globalProperties.ContainsKey(obj))
						ReceiveList.Add(globalProperties[obj]);
					else
						throw new FormatException($"{obj} is undefined.");

				}
			}


			return base.VisitEqualityExpression(context);
		}
	}
}
