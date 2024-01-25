using System;
using Z3 = Microsoft.Z3;

namespace GeneratorCalculation
{
	public abstract class Condition
	{
		public static Condition NotEqual(string x, string y)
		{
			return new NotCondition(new EqualCondition(x, y));
		}


		public static Condition NotEqual(PaperWord x, PaperWord y)
		{
			return new NotCondition(new EqualCondition(x, y));
		}

		protected Z3.BoolExpr expr;

		/// <summary>
		/// Build Z3 expression based on known ConcreteSort and Context.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public abstract Z3.BoolExpr GetExpr(Solver s);

	}

	public class Z3Condition : Condition
	{
		public Z3Condition(Func<Solver, Z3.BoolExpr> expBuilder)
		{
			this.expBuilder = expBuilder;
		}

		Func<Solver, Z3.BoolExpr> expBuilder;


		public override Z3.BoolExpr GetExpr(Solver s)
		{
			if (expr == null)
				expr = expBuilder(s);

			return expr;
		}
	}

	public class EqualCondition : Condition
	{
		private PaperWord x;
		private PaperWord y;

		public EqualCondition(string x, string y)
		{
			if (string.IsNullOrEmpty(x))
				throw new ArgumentException("x cannot be null or empty.");
			if (string.IsNullOrEmpty(y))
				throw new ArgumentException("y cannot be null or empty.");

			if (char.IsLower(x[0]))
				this.x = new PaperVariable(x);
			else
				this.x = new ConcreteType(x);


			if (char.IsLower(y[0]))
				this.y = new PaperVariable(y);
			else
				this.y = new ConcreteType(y);
		}

		public EqualCondition(PaperWord x, PaperWord y)
		{
			this.x = x;
			this.y = y;
		}

		public override Z3.BoolExpr GetExpr(Solver s)
		{
			if (expr == null)
				expr = x.BuildEquality(y, s);
			return expr;
		}
	}

	public class NotCondition : Condition
	{
		private EqualCondition equalCondition;

		public NotCondition(EqualCondition equalCondition)
		{
			this.equalCondition = equalCondition;
		}

		public override Z3.BoolExpr GetExpr(Solver s)
		{
			if (expr == null)
				expr = s.z3Ctx.MkNot(equalCondition.GetExpr(s));

			return expr;
		}
	}


	public class InheritanceCondition : Condition
	{
		public PaperType Subclass { get; set; }
		public PaperType Superclass { get; set; }

		public override Z3.BoolExpr GetExpr(Solver s)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return $"{Subclass}: {Superclass}";
		}
	}

	public class AndCondition : Condition
	{
		public Condition Condition1;
		public Condition Condition2;

		public override Z3.BoolExpr GetExpr(Solver s)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return $"{Condition1} and {Condition2}";
		}

	}
}
