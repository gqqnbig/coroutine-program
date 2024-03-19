using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Antlr4.Runtime.Misc;
using Microsoft.Extensions.Logging;

using GeneratorCalculation;
using GoLang.Antlr;

namespace Go
{
	class FunctionBodyCollector : GoParserBaseVisitor<bool>
	{
		protected static readonly ILogger logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(FunctionBodyCollector));

		protected Dictionary<string, string> channelsInFunc = null;
		protected List<DataFlow> flow;

		public Dictionary<string, FuncInfo> definitions = new Dictionary<string, FuncInfo>();

		public override bool VisitExpression([NotNull] GoParser.ExpressionContext context)
		{
			if (channelsInFunc != null && context.unary_op?.Type == GoLang.Antlr.GoLexer.RECEIVE)
			{
				string variableName = context.expression(0).GetText();
				string methodName = null;
				int p = variableName.IndexOf("(");
				FuncInfo fInfo = null;
				if (p != -1)
				{
					//variableName has (), so it is a function call.
					methodName = variableName.Substring(0, p);
					definitions.TryGetValue(variableName.Substring(0, variableName.IndexOf("(")), out fInfo);
				}

				if (methodName != null && fInfo != null)
				{
					flow.Add(new DataFlow(Direction.Yielding, new StartFunction(methodName)));

					var type = fInfo.ChannelType;
					flow.Add(new DataFlow(Direction.Resuming, new ConcreteType(char.ToUpper(type[0]) + type.Substring(1))));
					return true;
				}
				else if (channelsInFunc.TryGetValue(variableName, out string type))
				{
					flow.Add(new DataFlow(Direction.Resuming, new ConcreteType(char.ToUpper(type[0]) + type.Substring(1))));
					return true;
				}
				// We can ignore some functions, such as time.After()
				else
					logger.LogInformation($"{variableName} seems to be a channel, but its type is unknown.");
			}
			return base.VisitExpression(context);
		}

		// The loop for i := range c receives values from the channel repeatedly until it is closed. 
		// But we don't support it for now.


		public override bool VisitGoStmt([NotNull] GoParser.GoStmtContext context)
		{
			var exp = context.expression();
			var pExp = exp.primaryExpr();
			if (pExp != null)
			{
				var type = CheckPrimaryExpr(pExp);
				if (type is PaperVariable vType)
					flow.Add(new DataFlow(Direction.Yielding, new StartFunction(vType)));
				else if (type is CoroutineDefinitionType dType)
					flow.Add(new DataFlow(Direction.Yielding, new StartFunction(dType)));
			}


			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <returns>PaperVariable or CoroutineDefinitionType</returns>
		PaperType CheckPrimaryExpr([NotNull] GoParser.PrimaryExprContext context)
		{
			if (context.arguments() != null)
			{
				string methodName = context.primaryExpr().GetText();
				if (definitions.ContainsKey(methodName))
					return new PaperVariable(methodName);


				var def = FunctionLitCollector.Collect(context.primaryExpr(), new ReadOnlyDictionary<string, CoroutineDefinitionType>(definitions.ToDictionary(i => i.Key, i => i.Value.CoroutineType)),
														channelsInFunc);
				if (def != null)
					return def;
			}

			return null;
		}

		public override bool VisitPrimaryExpr([NotNull] GoParser.PrimaryExprContext context)
		{
			// Anonymous functions are supported.
			var type = CheckPrimaryExpr(context);
			if (type is PaperVariable vType)
			{
				flow.Add(new DataFlow(Direction.Yielding, new InlineFunction(vType)));
				return true;
			}
			else if (type is CoroutineDefinitionType dType)
			{
				flow.Add(new DataFlow(Direction.Yielding, new InlineFunction(dType)));
				return true;
			}

			return base.VisitPrimaryExpr(context);
		}
	}
}
