using Xunit;
using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation.Tests
{
	public class GeneratorTypeTests
	{

		[Fact()]
		public void CheckTest()
		{
			GeneratorType g = new GeneratorType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"z"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")),
				new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")));


			Assert.Throws<FormatException>(() => g.Check(new Dictionary<PaperVariable, ConcreteType>()));
		}
	}
}