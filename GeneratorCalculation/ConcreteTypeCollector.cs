﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation
{
	public class ConcreteTypeCollector
	{
		public HashSet<string> concreteTypes = new HashSet<string>();
		private Dictionary<PaperVariable, PaperWord> bindings;

		public ConcreteTypeCollector()
		{
			this.bindings = new Dictionary<PaperVariable, PaperWord>();
		}

		public ConcreteTypeCollector(Dictionary<PaperVariable, PaperWord> bindings)
		{
			this.bindings = bindings;
		}

		public void Visit(PaperType t)
		{
			switch (t)
			{
				case ConcreteType ct:
					Visit(ct);
					break;
				case FunctionType ct:
					Visit(ct);
					break;
				case ListType ct:
					Visit(ct);
					break;
				case PaperVariable ct:
					Visit(ct);
					break;
				case SequenceType ct:
					Visit(ct);
					break;
				case TupleType ct:
					Visit(ct);
					break;
				case CoroutineInstanceType ct:
					Visit(ct);
					break;
				case CoroutineDefinitionType ct:
					Visit(ct);
					break;
				default:
					throw new NotImplementedException();
			}

			//if (t is ConcreteType ct)
			//{
			//}
			//else if (t is FunctionType ft)
			//{
			//	Visit(ft);
			//}
			//else if (t is ListType lt)
			//{
			//	Visit(lt);
			//}
			//else if (t is PaperVariable vt)
			//{
			//	Visit(vt);
			//}
			//else if (t is SequenceType st)
			//{
			//	Visit(st);
			//}
			//else if (t is TupleType tt)
			//{
			//	Visit(tt);
			//}
			//else if (t is CoroutineInstanceType gt)
			//{
			//	Visit(gt);
			//}
			//else
			//	throw new NotImplementedException();
		}

		public void Visit(CoroutineInstanceType type)
		{
			foreach (var item in type.Flow)
			{
				Visit(item.Type);
			}

			if (type.Condition != null)
				type.Condition.GetConcreteTypes(concreteTypes);
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
				//A variable can point to a type that contains itself.
				bindings.Remove(t);
				if (value is PaperType vType)
					Visit(vType);
				bindings.Add(t, value);
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


		private void Visit(CoroutineDefinitionType t)
		{
			foreach (var item in t.Flow)
				Visit(item.Type);
		}
	}
}
