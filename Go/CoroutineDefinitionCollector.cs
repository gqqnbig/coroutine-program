using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using GeneratorCalculation;
using GoLang.Antlr;
using System.Collections.ObjectModel;

namespace Go
{
	class CoroutineDefinitionCollector : GoParserBaseVisitor<bool>
	{
		Dictionary<string, string> channelsInFunc = null;
		List<DataFlow> flow;

		public Dictionary<string, CoroutineDefinitionType> definitions = new Dictionary<string, CoroutineDefinitionType>();
		ReadOnlyDictionary<string, CoroutineDefinitionType> knownDefinitions;

		public CoroutineDefinitionCollector(Dictionary<string, CoroutineDefinitionType> knownDefinitions)
		{
			this.knownDefinitions = new ReadOnlyDictionary<string, CoroutineDefinitionType>(knownDefinitions);
		}


		public override bool VisitFunctionDecl([NotNull] GoParser.FunctionDeclContext context)
		{
			channelsInFunc = new Dictionary<string, string>();
			ParameterTypeVisitor v = new ParameterTypeVisitor();
			v.Visit(context.signature().parameters());
			foreach (var identifier in v.channelTypes.Keys)
			{
				channelsInFunc.Add(identifier, v.channelTypes[identifier]);
			}

			flow = new List<DataFlow>();

			VisitBlock(context.block());

			if (flow.Count > 0)
			{
				CoroutineDefinitionType coroutine = new CoroutineDefinitionType(flow);

				definitions.Add(context.IDENTIFIER().GetText(), coroutine);
				//This is coroutine definition.
				Console.WriteLine(context.IDENTIFIER().GetText() + ": " + coroutine);
			}

			return true;
		}



		public override bool VisitSendStmt([NotNull] GoParser.SendStmtContext context)
		{
			string channel = context.channel.GetText();
			string type;
			if (channelsInFunc.TryGetValue(channel, out type))
			{
				//Console.WriteLine($"Channel is {channel}:chan {type}");
			}
			else
				throw new FormatException();

			//to title case
			flow.Add(new DataFlow(Direction.Yielding, new ConcreteType(char.ToUpper(type[0]) + type.Substring(1))));
			return true;
			//return base.VisitSendStmt(context);
		}

		public override bool VisitShortVarDecl([NotNull] GoParser.ShortVarDeclContext context)
		{
			var variableName = context.identifierList().GetText();
			if (variableName.Contains(",") == false)
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


		public override bool VisitExpression([NotNull] GoParser.ExpressionContext context)
		{
			if (context.unary_op?.Type == GoLang.Antlr.GoLexer.RECEIVE)
			{
				string variableName = context.expression(0).GetText();
				string type = channelsInFunc[variableName];

				flow.Add(new DataFlow(Direction.Resuming, new ConcreteType(char.ToUpper(type[0]) + type.Substring(1))));
			}
			return base.VisitExpression(context);
		}

		public override bool VisitPrimaryExpr([NotNull] GoParser.PrimaryExprContext context)
		{
			if (context.arguments() != null)
			{
				string methodName = context.primaryExpr().GetText();
				if (knownDefinitions.TryGetValue(methodName, out var def))
				{
					flow.Add(new DataFlow(Direction.Yielding, new StartFunction(methodName)));
					//yieldTypes.Add(new FunctionType("Start", new PaperVariable(methodName)));
				}
			}

			return base.VisitPrimaryExpr(context);
		}
	}
}
