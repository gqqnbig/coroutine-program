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
			// obj.oclIsUndefined() = false
			var exp = BooleanUtils.SomethingIsFalse(context);
			if (exp != null)
			{
				var text = exp.GetText();
				if (text.EndsWith(".oclIsUndefined()"))
				{
					var obj = text.Substring(0, text.Length - ".oclIsUndefined()".Length);
					var components = new List<string>(obj.Split('.'));
					if (components[0] == "self")
						components.RemoveAt(0);

					AddToReceiveList(components[0]);
				}
			}

			exp = BooleanUtils.SomethingIsTrue(context);
			if (exp != null)
			{
				var text = exp.GetText();
				var components = new List<string>(text.Split('.'));
				if (components[0] == "self")
					components.RemoveAt(0);

				AddToReceiveList(components[0]);
			}


			return base.VisitEqualityExpression(context);
		}

		private void AddToReceiveList(string key)
		{
			//Debug.Assert(definitions.ContainsKey(obj));
			if (localVariables.ContainsKey(key))
				ReceiveList.Add(localVariables[key]);
			else if (properties.ContainsKey(key))
				ReceiveList.Add(key);
			else if (globalProperties.ContainsKey(key))
				ReceiveList.Add(key);
			else
				throw new FormatException($"{key} is undefined.");
		}
	}
}
