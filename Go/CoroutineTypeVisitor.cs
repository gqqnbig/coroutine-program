using Antlr4.Runtime.Misc;
using GoLang.Antlr;
using System;
using System.Collections.Generic;
using System.Text;
using GeneratorCalculation;

namespace Go
{
	class CoroutineTypeVisitor : GoParserBaseVisitor<bool>
	{
		Dictionary<string, string> channelTypes = new Dictionary<string, string>();
		List<PaperType> receiveTypes;
		List<PaperType> yieldTypes;

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

			if (receiveTypes.Count > 0 || yieldTypes.Count > 0)
			{
				CoroutineType coroutine = new CoroutineType(new SequenceType(receiveTypes), new SequenceType(yieldTypes));
				//This is coroutine definition.
				Console.WriteLine(coroutine);
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
				Console.WriteLine($"Channel is {channel}:chan {type}");
			}
			else
				throw new FormatException();
			
			//to title case
			yieldTypes.Add(new ConcreteType(char.ToUpper(type[0]) + type.Substring(1)));
			return true;
			//return base.VisitSendStmt(context);
		}
	}
}
