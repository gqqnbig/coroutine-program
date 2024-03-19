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
			Dictionary<string, string> knownChannels)
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


		Dictionary<string, string> channelsInFunc = null;
		List<DataFlow> flow;

		ReadOnlyDictionary<string, CoroutineDefinitionType> knownDefinitions;
		private readonly Dictionary<string, string> knownChannels;

		private FunctionLitCollector(ReadOnlyDictionary<string, CoroutineDefinitionType> knownDefinitions, Dictionary<string, string> knownChannels)
		{
			this.knownDefinitions = knownDefinitions;
			this.knownChannels = knownChannels;
		}

		public override bool VisitFunctionLit([NotNull] GoParser.FunctionLitContext context)
		{
			channelsInFunc = new Dictionary<string, string>(knownChannels);
			ParameterTypeVisitor v = new ParameterTypeVisitor();
			v.Visit(context.signature().parameters());
			foreach (var identifier in v.channelTypes.Keys)
			{
				channelsInFunc.Add(identifier, v.channelTypes[identifier]);
			}
			flow = new List<DataFlow>();

			return VisitBlock(context.block());
		}



		public override bool VisitSendStmt([NotNull] GoParser.SendStmtContext context)
		{
			if (channelsInFunc == null)
				return false;

			string channel = context.channel.GetText();
			string type;
			if (channelsInFunc.TryGetValue(channel, out type))
			{
				//Console.WriteLine($"Channel is {channel}:chan {type}");
			}
			else
				throw new FormatException($"Channel {channel} is unknown.");

			VisitExpression(context.expression(1));

			//to title case
			flow.Add(new DataFlow(Direction.Yielding, new ConcreteType(char.ToUpper(type[0]) + type.Substring(1))));
			return true;
			//return base.VisitSendStmt(context);
		}

		public override bool VisitShortVarDecl([NotNull] GoParser.ShortVarDeclContext context)
		{
			var variableName = context.identifierList().GetText();
			if (channelsInFunc != null && variableName.Contains(",") == false)
			{
				MakeChannelVisitor v = new MakeChannelVisitor();
				v.Visit(context.expressionList());
				if (v.type != null)
				{
					//Console.WriteLine("Found {0}:chan {1}", variableName, v.type);
					channelsInFunc.Add(variableName, v.type);
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

					MakeChannelVisitor v = new MakeChannelVisitor();
					v.Visit(spec.expressionList());
					if (v.type != null)
					{
						//Console.WriteLine("Found {0}:chan {1}", variableName, v.type);
						channelsInFunc.Add(variableName, v.type);
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
