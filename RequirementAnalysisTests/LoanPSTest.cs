using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GeneratorCalculation;
using Xunit;

namespace RequirementAnalysis.Tests
{
	public class LoanPSTest
	{
		[Fact]
		public static void Run()
		{
			string content;
			var assembly = typeof(LoanPSTest).GetTypeInfo().Assembly;
			var file = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("loan.remodel"));

			using (var stream = typeof(CocomeTest).GetTypeInfo().Assembly.GetManifestResourceStream(file))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					content = reader.ReadToEnd();
				}
			}

			var inheritance = REModelStart.GetObjectInheritance(content);
			var generators = REModelStart.GetAllGenerators(content, inheritance);

			string[] interestedCoroutines =
			{
				"SubmitLoanRequestModule::enterLoanInformation",
				"SubmitLoanRequestModule::creditRequest",
				"ManageUserCRUDService::createStudent",
				"LibraryManagementSystemSystem::makeReservation",
				"LibraryManagementSystemSystem::borrowBook",
				"LibraryManagementSystemSystem::returnBook",
			};

			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1))
			{
				bindings.Add(g.Name, g.Type);
			}

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new GeneratorType(new TupleType(from b in bindings select b.Key), ConcreteType.Void)));
			//coroutines.AddRange(generators.Where(g => Array.IndexOf(lowPriorityCoroutines2111, g.Name) != -1));



			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Console.WriteLine(result);
		}
	}
}
