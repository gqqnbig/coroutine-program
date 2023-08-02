using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

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


		/// <summary>
		/// Mapping from identifier to its type
		/// </summary>
		/// <remarks>
		/// When we meet the same identifier twice, we then know do not add the type twice.
		/// </remarks>
		private List<KeyValuePair<string, ConcreteType>> receivedObjects = new List<KeyValuePair<string, ConcreteType>>();

		public ReceiveCollector(Dictionary<string, string> localVariables,
								Dictionary<string, string> properties,
								Dictionary<string, string> globalProperties
								)
		{
			this.localVariables = localVariables;
			this.properties = properties;
			this.globalProperties = globalProperties;
		}

		public List<ConcreteType> GetReceiveList()
		{
			return receivedObjects.Select(p => p.Value).ToList();
		}

		public override bool VisitBasicExpression([NotNull] REModelParser.BasicExpressionContext context)
		{
			if (context.ChildCount > 0)
			{
				if (context.GetChild(0).GetText().EndsWith("oclIsTypeOf"))
					return base.Visit(context.GetChild(0)); // don't visit the argument of oclIsTypeOf because it's a type.
			}

			return base.VisitBasicExpression(context);
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
				Regex regex = new Regex(@"->includes\((\w+)\)$");
				var m = regex.Match(text);
				if (m.Success)
				{
					var obj = m.Groups[1].Value;
					if (localVariables.ContainsKey(obj))
					{
						if (receivedObjects.All(p => p.Key != obj))
							receivedObjects.Add(new KeyValuePair<string, ConcreteType>(obj, localVariables[obj]));
					}
					else
						throw new NotImplementedException($"{obj} is to be checked in DB, but it's not defined locally.");

					return true;
				}

				if (text.EndsWith(".oclIsUndefined()"))
				{
					var obj = text.Substring(0, text.Length - ".oclIsUndefined()".Length);
					//Do nothing
					return base.VisitEqualityExpression(context);
				}



				var components = new List<string>(text.Split('.'));
				if (components[0] == "self")
					components.RemoveAt(0);

				AddToReceiveList(components[0]);
			}


			return base.VisitEqualityExpression(context);
		}

		private void AddToReceiveList(string key)
		{
			string t;
			if (localVariables.ContainsKey(key))
				t = localVariables[key];
			else if (properties.ContainsKey(key))
				t = key;
			else if (globalProperties.ContainsKey(key))
				t = key;
			else
				throw new FormatException($"{key} is undefined.");

			if (receivedObjects.All(p => p.Key != t))
				receivedObjects.Add(new KeyValuePair<string, ConcreteType>(key, t));
		}
	}
}
