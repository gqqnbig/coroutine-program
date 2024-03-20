
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

using Xunit;
using GeneratorCalculation;

namespace Go.Tests
{
	public class GoCompositionTests
	{
		public static string GetEmbeddedFile(string fileName)
		{
			string content;
			var assembly = typeof(GoCompositionTests).GetTypeInfo().Assembly;
			var file = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName));

			using (var stream = typeof(GoCompositionTests).GetTypeInfo().Assembly.GetManifestResourceStream(file))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					content = reader.ReadToEnd();
				}
			}
			return content;
		}

		[Fact]
		public static void TestBasic()
		{
			string code = GetEmbeddedFile("basic.go");

			Assert.False(Program.CheckDeadlock(code));
		}

		[Fact]
		public static void TestBasic3Receive()
		{
			string code = GetEmbeddedFile("basic3receive.go");

			Assert.True(Program.CheckDeadlock(code));
		}


		[Fact]
		public static void TestBasicReceiveInFunction()
		{
			string code = GetEmbeddedFile("basic-receiveInFunction.go");

			Assert.False(Program.CheckDeadlock(code));
		}


		[Fact]
		public static void TestBasicReceiveInFunctionExtra()
		{
			string code = GetEmbeddedFile("basic-receiveInFunction-extra.go");

			Dictionary<string, CoroutineDefinitionType> definitions = Program.GetDefinitions(code);

			List<CoroutineInstanceType> instances = new List<CoroutineInstanceType>();

			var m = definitions["main"].Start("main");
			instances.Add(m);

			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var d in definitions)
				bindings.Add(d.Key, d.Value);


			var gs = from i in instances
					 select new Generator(i.Source.ToString(), i);

			Solver solver = new Solver();
			solver.CanLoopExternalYield = false;
			solver.MainCoroutine = "main";
			var result = solver.SolveWithBindings(gs.ToList(), bindings, 50);

			//Console.WriteLine("Composition result is " + result);

			Assert.Equal(2, result.Flow.Count);
			Assert.Equal(new DataFlow(Direction.Resuming, (ConcreteType)"Int"), result.Flow[0]);
			Assert.Equal(new DataFlow(Direction.Resuming, (ConcreteType)"Int"), result.Flow[1]);
		}

		[Fact]
		public static void TestFunc()
		{
			string code = GetEmbeddedFile("func.go");

			Assert.True(Program.CheckDeadlock(code));
		}

		[Fact]
		public static void TestOutOfOrder()
		{
			// disable External yield?
			// However, if we analyze a partial file, we don't have to disable External Yield
			// because the code can yield to its caller.
			string code = GetEmbeddedFile("out-of-order.go");

			Dictionary<string, CoroutineDefinitionType> definitions = Program.GetDefinitions(code);
			List<CoroutineInstanceType> instances = new List<CoroutineInstanceType>();

			var m = definitions["main"].Start("main");
			instances.Add(m);

			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var d in definitions)
				bindings.Add(d.Key, d.Value);


			var gs = from i in instances
					 select new Generator(i.Source.ToString(), i);


			Solver solver = new Solver();
			solver.CanLoopExternalYield = false;
			solver.MainCoroutine = "main";
			var result = solver.SolveWithBindings(gs.ToList(), bindings, 50);

			Assert.True(result.Flow.Count > 0);
			Assert.Equal(Direction.Yielding, result.Flow[0].Direction);
			Assert.Equal((ConcreteType)"String", result.Flow[0].Type);
		}

		[Fact]
		public static void TestMainExit()
		{
			// If the main goroutine exits, there will be no deadlock, whether or not other goroutines are locking or running.
			string code = GetEmbeddedFile("main-exit.go");

			Assert.False(Program.CheckDeadlock(code));
		}


		[Theory]
		[InlineData("NoLiveGoroutines.go")]
		//[InlineData("NoReceiver.go", Skip = "This case requires balanced yielding and receiving.")]
		[InlineData("NoSender.go")]
		public static void TestYumaInauraBlock(string fileName)
		{
			string code = GetEmbeddedFile(fileName);

			Assert.True(Program.CheckDeadlock(code));
		}

		[Theory]
		[InlineData("SleepingReceiver.go")]
		[InlineData("SleepingSender.go")]
		public static void TestYumaInauraFinish(string fileName)
		{
			string code = GetEmbeddedFile(fileName);

			Assert.False(Program.CheckDeadlock(code));
		}

	}

}
