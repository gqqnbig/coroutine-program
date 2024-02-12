using System.Reflection;
using System.Linq;
using System.IO;

using Xunit;

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

			Assert.True(Program.CheckDeadlock(code));
		}

		[Fact]
		public static void TestFunc()
		{
			string code = GetEmbeddedFile("func.go");

			Assert.True(Program.CheckDeadlock(code));
		}

	}

}
