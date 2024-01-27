using System;
using System.Collections.Generic;
using System.Linq;
using Z3 = Microsoft.Z3;

namespace GeneratorCalculation
{

	public class InheritanceCondition : Condition
	{
		const string functionName = "inherit";

		public PaperType Subclass { get; private set; }
		public PaperType Superclass { get; private set; }
		//public Z3.FuncDecl Function { get; private set; }

		public InheritanceCondition(PaperType subclass, PaperType superclass)
		{
			Subclass = subclass;
			Superclass = superclass;
		}

		private static Z3.Expr GetZ3Expr(Solver s, PaperType t)
		{
			if (t is ConcreteType subC)
				return s.ConcreteSort.Consts.First(c => c.ToString().Equals(subC.Name));
			else if (t is PaperVariable subV)
				return s.z3Ctx.MkConst(subV.Name, s.ConcreteSort);
			else
				throw new NotImplementedException();
		}

		public override Z3.BoolExpr GetExpr(Solver s)
		{
			if (expr == null)
			{
				var f = s.GetFunctionHead(functionName);
				Z3.Expr x = GetZ3Expr(s, Subclass);
				Z3.Expr y = GetZ3Expr(s, Superclass);
				expr = s.z3Ctx.MkEq(f[x, y], s.z3Ctx.MkTrue());
			}
			return expr;
		}

		public override string ToString()
		{
			return $"{Subclass} <: {Superclass}";
		}

		public static void BuildFunction(Solver solver, Dictionary<string, string> inheritance, out Z3.FuncDecl func, out Z3.BoolExpr funcBody)
		{
			Z3.Context ctx = solver.z3Ctx;
			var concreteSort = solver.ConcreteSort;
			func = ctx.MkFuncDecl(functionName, new Z3.Sort[] { concreteSort, concreteSort }, ctx.BoolSort);

			var x = ctx.MkConst("x", concreteSort);
			var y = ctx.MkConst("y", concreteSort);

			Z3.BoolExpr[] list = new Z3.BoolExpr[inheritance.Count];
			int i = 0;
			foreach (var item in inheritance)
			{
				var subclass = solver.ConcreteSort.Consts.First(c => c.ToString() == item.Key);
				var superclass = solver.ConcreteSort.Consts.First(c => c.ToString() == item.Value);
				list[i++] = ctx.MkAnd(ctx.MkEq(x, subclass), ctx.MkEq(y, superclass));
			}


			funcBody = ctx.MkForall(new Z3.Expr[] { x, y },
				ctx.MkImplies(
					ctx.MkNot(ctx.MkOr(list)),
					ctx.MkEq(func[x, y], ctx.MkFalse())
				)
			);


		}

		public override void GetConcreteTypes(HashSet<string> set)
		{
			var c1 = new ConcreteTypeCollector();
			c1.Visit(Superclass);
			set.UnionWith(c1.concreteTypes);

			var c2 = new ConcreteTypeCollector();
			c2.Visit(Subclass);
			set.UnionWith(c2.concreteTypes);
		}
	}
}
