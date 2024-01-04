using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Antlr4.Runtime.Misc;
using GoLang.Antlr;

namespace Go
{
	class GoStatementListener : GoParserBaseListener
	{
		int funcCount = 0;
		HashSet<string> coroutines = new HashSet<string>();
		string container = null;


		public override void EnterFunctionDecl([NotNull] GoParser.FunctionDeclContext context)
		{
			container = context.IDENTIFIER().GetText();
			base.EnterFunctionDecl(context);
		}

		public override void EnterGoStmt([NotNull] GoParser.GoStmtContext context)
		{
			var text = context.GetText();
			Debug.Assert(text.StartsWith("go"));
			text = text.Substring(2);
			int p = text.IndexOf("(");
			if (p == -1)
				throw new NotSupportedException($"`go {text}` in {container}() is not a method call.");
			string methodName = text.Substring(0, p);

			if (methodName == "func")
			{
				methodName += (++funcCount) + container;
			}

			Console.WriteLine($"{methodName}() in {container}() is a coroutine.");
			coroutines.Add(methodName);

			base.EnterGoStmt(context);
		}


	}
}
