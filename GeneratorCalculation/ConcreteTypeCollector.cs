using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation
{
	class ConcreteTypeCollector
	{
		public HashSet<string> concreteTypes = new HashSet<string>();
		private Dictionary<PaperVariable, PaperWord> bindings;

		public ConcreteTypeCollector(Dictionary<PaperVariable, PaperWord> bindings)
		{
			this.bindings = bindings;
		}

		private void Visit(PaperType t)
		{
			if (t is ConcreteType ct)
			{
				Visit(ct);
			}
			else if (t is FunctionType ft)
			{
				Visit(ft);
			}
			else if (t is ListType lt)
			{
				Visit(lt);
			}
			else if (t is PaperVariable vt)
			{
				Visit(vt);
			}
			else if (t is SequenceType st)
			{
				Visit(st);
			}
			else if (t is TupleType tt)
			{
				Visit(tt);
			}
			else if (t is GeneratorType gt)
			{
				Visit(gt);
			}
			else
				throw new NotImplementedException();
		}

		public void Visit(GeneratorType type)
		{
			Visit(type.Receive);
			Visit(type.Yield);
		}


		private void Visit(ConcreteType t)
		{
			concreteTypes.Add(t.Name);
		}

		private void Visit(FunctionType t)
		{
			foreach (var arg in t.Arguments)
			{
				if (arg is PaperType pt)
					Visit(pt);
			}
		}
		private void Visit(ListType t)
		{
			Visit(t.Type);
		}
		private void Visit(PaperVariable t)
		{
			if (bindings.TryGetValue(t, out PaperWord value))
			{
				if (value is PaperType vType)
					Visit(vType);
			}

		}
		private void Visit(SequenceType t)
		{
			foreach (var item in t.Types)
			{
				Visit(item);
			}
		}
		private void Visit(TupleType t)
		{
			foreach (var item in t.Types)
			{
				Visit(item);
			}
		}
		//private void Visit(CoroutineDefinitionType t)
		//{

		//}
	}
}
