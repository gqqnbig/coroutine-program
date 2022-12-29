using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation
{
	public class DecFunction : FunctionType
	{
		public DecFunction(params PaperWord[] words) : base("dec", words)
		{
		}

		public DecFunction(IEnumerable<PaperWord> words) : base("dec", words)
		{
		}

		public override PaperWord ApplyEquation(List<KeyValuePair<PaperVariable, PaperWord>> equations)
		{
			return new DecFunction(Arguments[0].ApplyEquation(equations));
		}

		public override PaperWord Evaluate()
		{
			if (Arguments[0] is PaperInt aInt)
			{
				return (PaperInt)(aInt.Value - 1);
			}

			return this;
		}

	}
}
