﻿using GeneratorCalculation;
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