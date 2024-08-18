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
			var c = new FunctionLitCollector(knownDefinitions, knownChannels);
			c.Visit(context);

			if (c.flow != null && c.flow.Count > 0)
			{
				CoroutineDefinitionType coroutine = new CoroutineDefinitionType(c.flow);
				return coroutine;
			}
			else
				return null;
		}



		ReadOnlyDictionary<string, CoroutineDefinitionType> knownDefinitions;
		//private readonly Dictionary<string, string> knownChannels;

		private FunctionLitCollector(ReadOnlyDictionary<string, CoroutineDefinitionType> knownDefinitions, LayeredDictionary<string, string> knownChannels)
		{
			this.knownDefinitions = knownDefinitions;
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

				return VisitBlock(context.block());
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

		public override bool VisitVarDecl([NotNull] GoParser.VarDeclContext context)
		{
			foreach (var spec in context.varSpec())
			{
				var variableName = spec.identifierList().GetText();
				if (variableName.Contains(","))
					continue; // TODO: deal with comma separated variable assignments.
				if (spec.expressionList() == null)
					continue;

				if (spec.type_() != null)
				{
					string t = ParameterTypeVisitor.GetChannelType(spec.type_());
					if (t != null)
					{
						channelsInFunc.Add(variableName, t);
						continue;
					}
				}

				if (spec.expressionList() != null)
				{

					MakeChannelVisitor v = new MakeChannelVisitor(definitions);
					v.Visit(spec.expressionList());
					if (v.type != null)
					{
						//Console.WriteLine("Found {0}:chan {1}", variableName, v.type);
						channelsInFunc.Add(variableName, v.type);
						Visit(spec.expressionList());
						continue;
					}

					//// If this statement defines an inline function, save the function to definitions.
					//var def = FunctionLitCollector.Collect(spec.expressionList(), new ReadOnlyDictionary<string, CoroutineDefinitionType>(definitions), channelsInFunc);
					//if (def != null)
					//{
					//	definitions[variableName] = def;
					//	continue;
					//}
				}
			}
			return true;
		}


	}
}
