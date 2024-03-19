using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation
{
	public class InlineFunction : FunctionType
	{
		public InlineFunction(PaperVariable variable) : base("Inline", variable)
		{
		}

		public InlineFunction(CoroutineDefinitionType definition) : base("Inline", definition)
		{
		}

		public override PaperWord ApplyEquation(Dictionary<PaperVariable, PaperWord> equations)
		{
			return new InlineFunction((CoroutineDefinitionType)Arguments[0].ApplyEquation(equations));

			throw new NotImplementedException();
			//return new StartFunction((CoroutineDefinitionType)Arguments[0].ApplyEquation(equations)).Evaluate();
		}

		public override PaperWord Evaluate()
		{
			throw new NotImplementedException();
			//return ((CoroutineDefinitionType)base.Arguments[0]).Start();
		}

		public List<DataFlow> EvaluateFlow()
		{
			CoroutineDefinitionType def = (CoroutineDefinitionType)Arguments[0];
			if (def == null)
				throw new NotSupportedException();

			return def.Start().Flow;

		}
	}
}
