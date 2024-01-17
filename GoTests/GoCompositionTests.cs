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
			string code = GetEmbeddedFile("channels.go");

			Assert.False(Program.CheckDeadlock(code));
		}


	}

}
