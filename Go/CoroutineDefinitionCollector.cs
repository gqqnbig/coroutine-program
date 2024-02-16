using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using GeneratorCalculation;
using GoLang.Antlr;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace Go
{
	class CoroutineDefinitionCollector : GoParserBaseVisitor<bool>
	{
		private static readonly ILogger logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(CoroutineDefinitionCollector));

		Dictionary<string, string> channelsInFunc = null;
		List<DataFlow> flow;

		public Dictionary<string, CoroutineDefinitionType> definitions = new Dictionary<string, CoroutineDefinitionType>();
		//ReadOnlyDictionary<string, CoroutineDefinitionType> knownDefinitions;

		public CoroutineDefinitionCollector(Dictionary<string, CoroutineDefinitionType> knownDefinitions)
		{
			this.definitions = new Dictionary<string, CoroutineDefinitionType>(knownDefinitions);
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

				definitions[context.IDENTIFIER().GetText()] = coroutine;
				//This is coroutine definition.
				Console.WriteLine(context.IDENTIFIER().GetText() + ": " + coroutine);
			}

			return true;
		}

		public override bool VisitFunctionLit([NotNull] GoParser.FunctionLitContext context)
		{
			logger.LogWarning("FunctionLit should be handled by FunctionLitCollector: " + context.GetText());
			return false;
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
					return true;
				}


				// If this statement defines an inline function, save the function to definitions.
				var def = FunctionLitCollector.Collect(context.expressionList(), new ReadOnlyDictionary<string, CoroutineDefinitionType>(definitions), channelsInFunc);
				if (def != null)
				{
					definitions[variableName] = def;
					return true;
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


					// If this statement defines an inline function, save the function to definitions.
					var def = FunctionLitCollector.Collect(spec.expressionList(), new ReadOnlyDictionary<string, CoroutineDefinitionType>(definitions), channelsInFunc);
					if (def != null)
					{
						definitions[variableName] = def;
						continue;
					}
				}

			}
			return true;
		}

		public override bool VisitAssignment([NotNull] GoParser.AssignmentContext context)
		{
			var variableName = context.expressionList(0).GetText();
			if (variableName.Contains(",") == false)
			{
				// If this statement defines an inline function, save the function to definitions.
				var def = FunctionLitCollector.Collect(context.expressionList(1), new ReadOnlyDictionary<string, CoroutineDefinitionType>(definitions), channelsInFunc);
				if (def != null)
				{
					definitions[variableName] = def;
					return true;
				}
			}
			return base.VisitAssignment(context);
		}

		public override bool VisitGoStmt([NotNull] GoParser.GoStmtContext context)
		{
			return base.VisitGoStmt(context);
		}


		public override bool VisitExpression([NotNull] GoParser.ExpressionContext context)
		{
			if (context.unary_op?.Type == GoLang.Antlr.GoLexer.RECEIVE)
			{
				string variableName = context.expression(0).GetText();

				if (channelsInFunc.TryGetValue(variableName, out string type))
				{
					flow.Add(new DataFlow(Direction.Resuming, new ConcreteType(char.ToUpper(type[0]) + type.Substring(1))));
					return true;
				}
				else
					logger.LogInformation($"{variableName} seems to be a channel, but its type is unknown.");
			}
			return base.VisitExpression(context);
		}

		public override bool VisitPrimaryExpr([NotNull] GoParser.PrimaryExprContext context)
		{
			if (context.arguments() != null)
			{
				string methodName = context.primaryExpr().GetText();
				if (definitions.ContainsKey(methodName))
				{
					flow.Add(new DataFlow(Direction.Yielding, new StartFunction(methodName)));
					//yieldTypes.Add(new FunctionType("Start", new PaperVariable(methodName)));
					return true;
				}

				var def = FunctionLitCollector.Collect(context.primaryExpr(), new ReadOnlyDictionary<string, CoroutineDefinitionType>(definitions), channelsInFunc);
				if (def != null)
				{
					flow.Add(new DataFlow(Direction.Yielding, new StartFunction(def)));
					return true;
				}
			}

			return base.VisitPrimaryExpr(context);
		}
	}
}
