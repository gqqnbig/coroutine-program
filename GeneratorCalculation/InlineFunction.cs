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
			// Arguments cannot be constants.
			var v = Arguments[0].ApplyEquation(equations);
			if (v is CoroutineDefinitionType vd)
				return new InlineFunction(vd);
			else if (v is PaperVariable)
			{
				// The argument is likely to be a function that returns a channel, with no sending or receiving operation.
				return new InlineFunction(new CoroutineDefinitionType(new List<DataFlow>()));
			}

			throw new NotImplementedException();
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
