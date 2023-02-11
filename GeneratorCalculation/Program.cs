using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{
		static List<Generator> GetPrologKnowledgeBase()
		{
			var gNegate1 = new GeneratorType((ConcreteType)"Yes", (ConcreteType)"Negate");
			var gNegate2 = new GeneratorType(ConcreteType.Void, (ConcreteType)"Yes");

			var coroutines = new List<Generator>
			{
				new Generator("child1", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "John", (ConcreteType) "Sue")))),
				new Generator("child2", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "Jane", (ConcreteType) "Sue")))),
				new Generator("child3", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "Sue", (ConcreteType) "George")))),
				new Generator("child4", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "John", (ConcreteType) "Sam")))),
				new Generator("child5", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "Jane", (ConcreteType) "Sam")))),
				new Generator("child6", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Child", (ConcreteType) "Sue", (ConcreteType) "Gina")))),
				new Generator("female1", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Female", (ConcreteType) "Sue")))),
				new Generator("female2", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Female", (ConcreteType) "Jane")))),
				new Generator("female3", new GeneratorType(ConcreteType.Void, new SequenceType(new SequenceType((ConcreteType) "Female", (ConcreteType) "June")))),
				new Generator("female-other", new GeneratorType((ConcreteType)"No", new SequenceType(new SequenceType((ConcreteType) "Female", (PaperVariable) "x")))),
				new Generator("parent", new GeneratorType(new SequenceType(new SequenceType((ConcreteType) "Child", (PaperVariable) "x", (PaperVariable) "y")), new SequenceType(new SequenceType((ConcreteType) "Parent", (PaperVariable) "y", (PaperVariable) "x")))),

				new Generator("Negate", new GeneratorType(new SequenceType(gNegate1,gNegate2),(ConcreteType)"No")),
			};
			return coroutines;
		}


		static void Main(string[] args)
		{
			var coroutines = GetPrologKnowledgeBase();

			coroutines.Add(new Generator("query", new GeneratorType(new SequenceType(new SequenceType((ConcreteType)"Parent", (PaperVariable)"x", (ConcreteType)"John"), new SequenceType((ConcreteType)"Female", (PaperVariable)"x"), (ConcreteType)"Negate", (ConcreteType)"Yes"), (PaperVariable)"x")));
			coroutines.Add(new Generator("starter", new GeneratorType((ConcreteType)"Sam", ConcreteType.Void)));

			var result = Solver.Solve(coroutines);
			Console.WriteLine(result);
		}


	}

	/// <summary>
	/// labeled generator
	/// </summary>
	public class Generator
	{
		public string Name { get; }
		public bool IsInfinite { get; }

		public GeneratorType OriginalType { get; }
		public GeneratorType Type { get; set; }

		public Generator(string name, GeneratorType type) : this(name, false, type)
		{ }


		public Generator(string name, bool isInfinite, GeneratorType type)
		{
			Name = name;
			IsInfinite = isInfinite;
			Type = type;
			OriginalType = type.Clone();
		}

	}
}
