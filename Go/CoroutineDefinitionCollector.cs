using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;
using GoLang.Antlr;

namespace Go
{
	class CoroutineDefinitionCollector : GoParserBaseVisitor<bool>
	{
		Dictionary<string, string> channelTypes = new Dictionary<string, string>();
		List<PaperType> receiveTypes;
		List<PaperType> yieldTypes;

		public Dictionary<string, CoroutineDefinitionType> definitions = new Dictionary<string, CoroutineDefinitionType>();

		public override bool VisitFunctionDecl([NotNull] GoParser.FunctionDeclContext context)
		{
			ParameterTypeVisitor v = new ParameterTypeVisitor();
			v.Visit(context.signature().parameters());
			foreach (var identifier in v.channelTypes.Keys)
			{
				channelTypes.Add(identifier, v.channelTypes[identifier]);
			}


			receiveTypes = new List<PaperType>();
			yieldTypes = new List<PaperType>();

			VisitBlock(context.block());

			PaperType r = ConcreteType.Void;
			if (receiveTypes.Count > 0)
				r = new SequenceType(receiveTypes);
			PaperType y = ConcreteType.Void;
			if (yieldTypes.Count > 0)
				y = new SequenceType(yieldTypes);
			if (r != ConcreteType.Void || y != ConcreteType.Void)
			{
				CoroutineDefinitionType coroutine = new CoroutineDefinitionType(r, y);

				definitions.Add(context.IDENTIFIER().GetText(), coroutine);
				//This is coroutine definition.
				Console.WriteLine(context.IDENTIFIER().GetText() + ": " + coroutine);
			}

			foreach (var identifier in v.channelTypes.Keys)
			{
				channelTypes.Remove(identifier);
			}
			return true;
		}



		public override bool VisitSendStmt([NotNull] GoParser.SendStmtContext context)
		{
			string channel = context.channel.GetText();
			string type;
			if (channelTypes.TryGetValue(channel, out type))
			{
				//Console.WriteLine($"Channel is {channel}:chan {type}");
			}
			else
				throw new FormatException();

			//to title case
			yieldTypes.Add(new ConcreteType(char.ToUpper(type[0]) + type.Substring(1)));
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
					channelTypes.Add(variableName, v.type);
				}
			}

			return base.VisitShortVarDecl(context);
		}


		public override bool VisitExpression([NotNull] GoParser.ExpressionContext context)
		{
			if (context.unary_op?.Type == GoLang.Antlr.GoLexer.RECEIVE)
			{
				string variableName = context.expression(0).GetText();
				string type = channelTypes[variableName];

				receiveTypes.Add(new ConcreteType(char.ToUpper(type[0]) + type.Substring(1)));
			}
			return base.VisitExpression(context);
		}
	}
}
