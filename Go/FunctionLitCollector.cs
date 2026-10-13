using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using GeneratorCalculation;
using GoLang.Antlr;
using System.Collections.ObjectModel;

namespace Go
{
	class FunctionLitCollector : FunctionBodyCollector
	{
		public static CoroutineDefinitionType Collect([NotNull] Antlr4.Runtime.Tree.IParseTree context,
			ReadOnlyDictionary<string, CoroutineDefinitionType> knownDefinitions,
			LayeredDictionary<string, string> knownChannels)
		{
			var c = new FunctionLitCollector(knownChannels);
			foreach (var d in knownDefinitions)
				c.definitions[d.Key] = new FuncInfo() { CoroutineType = d.Value };

			c.Visit(context);

			if (c.flow != null && c.flow.Count > 0)
			{
				CoroutineDefinitionType coroutine = new CoroutineDefinitionType(c.flow);
				return coroutine;
			}
			else
				return null;
		}



		//private readonly Dictionary<string, string> knownChannels;

		private FunctionLitCollector(LayeredDictionary<string, string> knownChannels)
		{
			this.channelsInFunc = knownChannels;
		}

		public override bool VisitFunctionLit([NotNull] GoParser.FunctionLitContext context)
		{
			try
			{
				channelsInFunc.AddLayer();
				//channelsInFunc = new Dictionary<string, string>(knownChannels);
				ParameterTypeVisitor v = new ParameterTypeVisitor();
				v.Visit(context.signature().parameters());
				foreach (var identifier in v.channelTypes.Keys)
				{
					channelsInFunc.Add(identifier, v.channelTypes[identifier]);
				}
				flow = new List<DataFlow>();
				deferFlow = new List<DataFlow>();

				VisitBlock(context.block());


				flow.AddRange(deferFlow);
				return true;
			}
			finally
			{
				channelsInFunc.RemoveLayer();
			}
		}




		public override bool VisitShortVarDecl([NotNull] GoParser.ShortVarDeclContext context)
		{
			var variableName = context.identifierList().GetText();
			if (variableName.Contains(",") == false)
			{
				MakeChannelVisitor v = new MakeChannelVisitor(definitions);
				v.Visit(context.expressionList());
				if (v.type != null)
				{
					//Console.WriteLine("Found {0}:chan {1}", variableName, v.type);
					channelsInFunc.Add(variableName, v.type);
					return Visit(context.expressionList());
				}
			}

			return base.VisitShortVarDecl(context);
		}



	}
}
