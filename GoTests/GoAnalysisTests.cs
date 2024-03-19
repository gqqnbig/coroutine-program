using GeneratorCalculation;

using Xunit;

namespace Go.Tests
{
	public class GoAnalysisTests
	{
		[Fact]
		public static void TestInlineFunc()
		{
			string code = GoCompositionTests.GetEmbeddedFile("inline-func.go");

			var definitions = Program.GetDefinitions(code);

			Assert.True(definitions.TryGetValue("main", out var main));
			Assert.NotNull(main);

			Assert.Equal(Direction.Yielding, main.Flow[0].Direction);
			Assert.IsType<StartFunction>(main.Flow[0].Type);

			Assert.Equal(Direction.Yielding, main.Flow[1].Direction);
			Assert.IsType<StartFunction>(main.Flow[1].Type);


			var f = definitions["f"];
			Assert.NotNull(f);
			Assert.Single(f.Flow);

			Assert.False(Program.CheckDeadlock(definitions));
		}

		[Fact]
		public static void TestInlineFuncVarOutside()
		{
			string code = GoCompositionTests.GetEmbeddedFile("inline-func-varOutside.go");
			Assert.False(Program.CheckDeadlock(code));
		}
	}
}
