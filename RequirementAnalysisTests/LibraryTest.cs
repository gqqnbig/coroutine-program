using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GeneratorCalculation;
using Xunit;

namespace RequirementAnalysis.Tests
{
	public class LibraryTest
	{
		[Fact]
		public static void BorrowBook()
		{
			string content;
			var assembly = typeof(LibraryTest).GetTypeInfo().Assembly;
			var file = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("library.remodel"));

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
				"ManageBookCRUDService::createBook",
				"ManageBookCopyCRUDService::addBookCopy",
				"ManageUserCRUDService::createStudent",
				"LibraryManagementSystemSystem::makeReservation",
				"LibraryManagementSystemSystem::borrowBook",
				"LibraryManagementSystemSystem::returnBook",

			};
			string[] lowPriorityCoroutines =
			{
				"ManageUserCRUDService::deleteUser",
				"ManageBookCRUDService::deleteBook",
				"ManageBookCopyCRUDService::deleteBookCopy",
			};



			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1))
			{
				bindings.Add(g.Name, g.Type);
			}

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new GeneratorType(new TupleType(from b in bindings select b.Key), ConcreteType.Void)));
			coroutines.AddRange(generators.Where(g => Array.IndexOf(lowPriorityCoroutines, g.Name) != -1));



			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Console.WriteLine(result);
		}


		[Fact]
		public static void RecommendBook()
		{
			string content;
			var assembly = typeof(LibraryTest).GetTypeInfo().Assembly;
			var file = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("library.remodel"));

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
				"ManageUserCRUDService::createFaculty",
				"ManageUserCRUDService::createStudent",
				"LibraryManagementSystemSystem::recommendBook",
				"ListBookHistory::listRecommendBook",
			};
			string[] lowPriorityCoroutines =
			{
				"ManageUserCRUDService::deleteUser",
			};

			var bindings = new Dictionary<PaperVariable, PaperWord>();
			foreach (var g in generators.Where(g => Array.IndexOf(interestedCoroutines, g.Name) != -1))
			{
				bindings.Add(g.Name, g.Type);
			}

			var coroutines = new List<Generator>();

			coroutines.Add(new Generator("", new GeneratorType(new TupleType(from b in bindings select b.Key), ConcreteType.Void)));


			var result = new Solver().SolveWithBindings(coroutines, bindings);
			Console.WriteLine(result);
		}
	}
}
