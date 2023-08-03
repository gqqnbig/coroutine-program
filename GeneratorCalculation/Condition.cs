using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation
{
	public abstract class Condition
	{
	}

	public class InheritanceCondition : Condition
	{
		public PaperType Subclass { get; set; }
		public PaperType Superclass { get; set; }

		public override string ToString()
		{
			return $"{Subclass}: {Superclass}";
		}
	}

	public class AndCondition : Condition
	{
		public Condition Condition1;
		public Condition Condition2;

		public override string ToString()
		{
			return $"{Condition1} and {Condition2}";
		}

	}
}
