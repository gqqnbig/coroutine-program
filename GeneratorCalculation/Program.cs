using System;
using System.Collections.Generic;

namespace GeneratorCalculation
{
	class Program
	{
		static void Main(string[] args)
		{

			PaperType oc1 = new GeneratorType((ConcreteType)"Y", ConcreteType.Void);
			Console.WriteLine("oc1: " + oc1);


			PaperType oc2 = new GeneratorType((ConcreteType)"Y", ConcreteType.Void);
			Console.WriteLine("oc2: " + oc2);

			PaperType fr1 = new GeneratorType(new ListType((ConcreteType)"S", new PaperVariable("n")), ConcreteType.Void);
			Console.WriteLine("fr1: " + fr1);

			PaperType fr2 = new GeneratorType(new ListType((ConcreteType)"S", new PaperVariable("m")), ConcreteType.Void);
			Console.WriteLine("fr2: " + fr2);

			GeneratorType interleave = new GeneratorType(new ListType(new SequenceType((PaperVariable)"x", (PaperVariable)"y"), new FunctionType("min", (PaperVariable)"n", (PaperVariable)"m")),
				new SequenceType(new ListType((PaperVariable)"x", (PaperVariable)"n"), new ListType((PaperVariable)"y", (PaperVariable)"m")));


			Console.WriteLine(interleave.Check());

			Console.WriteLine("interleave: " + interleave);


			//next(oc1) --> G void void: state transition.
		}

		static void Solve(List<GeneratorType> coroutines)
		{
			//find a generator type where the next type is not void.

			for (int i = 0; i < coroutines.Count; i++)
			{
				var coroutine = coroutines[i];

				if (coroutine.Yield != ConcreteType.Void)
				{
					RunNext(coroutine);
				}
			}

		}

		static void RunNext(GeneratorType coroutine)
		{
			if (coroutine.Yield == null)
				throw new Exception();

			if (coroutine.Yield == ConcreteType.Void)
				return;


		}
	}
}
