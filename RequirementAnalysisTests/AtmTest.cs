using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GeneratorCalculation;
using Xunit;

namespace RequirementAnalysis.Tests
{
	public class AtmTest
	{
		[Fact]
		public static void Run()
		{
			string content;
			var assembly = typeof(AtmTest).GetTypeInfo().Assembly;
			var file = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("atm.remodel"));

			using (var stream = typeof(CocomeTest).GetTypeInfo().Assembly.GetManifestResourceStream(file))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					content = reader.ReadToEnd();
				}
			}

			var inheritance = REModelStart.GetObjectInheritance(content);
			var generators = REModelStart.GetAllGenerators(content, inheritance);

			generators.Add(new Generator("inputCard", new CoroutineInstanceType(ConcreteType.Void, new SequenceType("CardIDValidated", "InputCard"))));
			generators.Add(new Generator("inputPassword", new CoroutineInstanceType(new SequenceType("CardIDValidated", "InputCard"), new SequenceType("CardIDValidated", "InputCard", "PasswordValidated"))));

			string[] interestedCoroutines =
			{
				"inputCard",
				"inputPassword",
				"AutomatedTellerMachineSystem::checkBalance",
				"AutomatedTellerMachineSystem::withdrawCash",
				"AutomatedTellerMachineSystem::depositFunds",
				"AutomatedTellerMachineSystem::printReceipt",
				"AutomatedTellerMachineSystem::ejectCard",
			};

			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1))
			{
				bindings.Add(g.Name, g.Type);
			}

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new CoroutineInstanceType(ConcreteType.Void, new TupleType(from b in bindings select b.Key))));
			//coroutines.AddRange(generators.Where(g => Array.IndexOf(lowPriorityCoroutines2111, g.Name) != -1));



			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Console.WriteLine(result);
		}
	}
}
