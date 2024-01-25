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

		public Z3Condition(Func<Solver, Z3.BoolExpr> expBuilder, string informalDescription = null)
		{
			this.expBuilder = expBuilder;
			InformalDescription = informalDescription;
		}

		Func<Solver, Z3.BoolExpr> expBuilder;

		public string InformalDescription { get; }

		public override Z3.BoolExpr GetExpr(Solver s)
		{
			if (expr == null)
				expr = expBuilder(s);

			return expr;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(InformalDescription) == false)
				return InformalDescription;
			else if (expr != null)
				return expr.ToString();
			else
				return base.ToString();
		}
	}

	public class EqualCondition : Condition
	{
		public PaperWord X { get; private set; }
		public PaperWord Y { get; private set; }

		public EqualCondition(string x, string y)
		{
			if (string.IsNullOrEmpty(x))
				throw new ArgumentException("x cannot be null or empty.");
			if (string.IsNullOrEmpty(y))
				throw new ArgumentException("y cannot be null or empty.");

			if (char.IsLower(x[0]))
				this.X = new PaperVariable(x);
			else
				this.X = new ConcreteType(x);


			if (char.IsLower(y[0]))
				this.Y = new PaperVariable(y);
			else
				this.Y = new ConcreteType(y);
		}

		public EqualCondition(PaperWord x, PaperWord y)
		{
			this.X = x;
			this.Y = y;
		}

		public override Z3.BoolExpr GetExpr(Solver s)
		{
			if (expr == null)
				expr = X.BuildEquality(Y, s);
			return expr;
		}

		public override string ToString()
		{
			return $"{X} == {Y}";
		}
	}

	public class NotCondition : Condition
	{
		private Condition condition;

		public NotCondition(Condition equalCondition)
		{
			this.condition = equalCondition;
		}

		public override Z3.BoolExpr GetExpr(Solver s)
		{
			if (expr == null)
				expr = s.z3Ctx.MkNot(condition.GetExpr(s));

			return expr;
		}

		public override string ToString()
		{
			EqualCondition ec = condition as EqualCondition;
			if (ec != null)
				return $"{ec.X} != {ec.Y}";
			else
				return $"NOT({condition})";
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
