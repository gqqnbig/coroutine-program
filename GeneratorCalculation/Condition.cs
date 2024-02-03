﻿using System;
using System.Collections.Generic;
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

		public abstract void GetConcreteTypes(HashSet<string> set);

		public override bool Equals(object obj)
		{
			if (obj == this)
				return true;

			if (obj is Condition objC)
				throw new NotSupportedException("Equality of Condition cannot be tested.");

			return false;
		}

		public override int GetHashCode()
		{
			throw new NotSupportedException(nameof(Condition) + " cannot be used as key in a hashtable or a dictionary.");
			//return 0;
		}
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

		public override void GetConcreteTypes(HashSet<string> set)
		{
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

		public override void GetConcreteTypes(HashSet<string> set)
		{
			if (X is PaperType xt)
			{
				var c1 = new ConcreteTypeCollector(new Dictionary<PaperVariable, PaperWord>());
				c1.Visit(xt);
				set.UnionWith(c1.concreteTypes);
			}

			if (Y is PaperType yt)
			{
				var c1 = new ConcreteTypeCollector(new Dictionary<PaperVariable, PaperWord>());
				c1.Visit(yt);
				set.UnionWith(c1.concreteTypes);
			}
		}
	}

	public class NotCondition : Condition
	{
		private Condition condition;

		public NotCondition(Condition equalCondition)
		{
			this.condition = equalCondition;
		}

		public override void GetConcreteTypes(HashSet<string> set)
		{
			condition.GetConcreteTypes(set);
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


	public class AndCondition : Condition
	{
		public Condition Condition1;
		public Condition Condition2;

		public override void GetConcreteTypes(HashSet<string> set)
		{
			Condition1.GetConcreteTypes(set);
			Condition2.GetConcreteTypes(set);
		}

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
