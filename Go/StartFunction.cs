using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Go
{
	class StartFunction : FunctionType
	{
		public StartFunction(CoroutineDefinitionType definition) : base("Start", definition)
		{
		}

		public override PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			return new StartFunction((CoroutineDefinitionType)Arguments[0]).Evaluate();
		}

		public override PaperWord Evaluate()
		{
			return ((CoroutineDefinitionType)base.Arguments[0]).Start();
		}
	}
}
