using Antlr4.Runtime.Misc;
using GeneratorCalculation;
using GoLang.Antlr;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

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
	}
}
