using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Go
{
	public class StartFunction : FunctionType
	{
		public StartFunction(string variable) : base("Start", (PaperVariable)variable)
		{
		}

		public StartFunction(CoroutineDefinitionType definition) : base("Start", definition)
		{
		}

		public override PaperWord ApplyEquation(Dictionary<PaperVariable, PaperWord> equations)
		{
			return new StartFunction((CoroutineDefinitionType)Arguments[0].ApplyEquation(equations)).Evaluate();
		}

		public override PaperWord Evaluate()
		{
			return ((CoroutineDefinitionType)base.Arguments[0]).Start();
		}
	}
}
