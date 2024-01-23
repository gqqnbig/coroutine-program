
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Z3 = Microsoft.Z3;

namespace GeneratorCalculation
{
	class Z3Helper
	{
		private static readonly ILogger logger = ApplicationLogging.LoggerFactory.CreateLogger(nameof(Z3Helper));

		public static Dictionary<PaperVariable, PaperWord> GetAssignments(Z3.Solver solver)
		{
			Dictionary<PaperVariable, PaperWord> conditions = new Dictionary<PaperVariable, PaperWord>();

			//solver.Push() does not save the Model.
			var model = solver.Model;
			foreach (var exp in model.Consts)
			{
				var intp = model.ConstInterp(exp.Key);
				if (intp == null)
					continue;

				var c = solver.Context.MkConst(exp.Key.Name, exp.Key.Range);
				if (solver.Check(solver.Context.MkNot(solver.Context.MkEq(c, intp))) == Z3.Status.SATISFIABLE)
				{

					intp = FindEqualtyAssignment(solver.Assertions, exp.Key.Name);
					if (intp != null)
					{
						//logger messages don't support numbered (or repeated) placeholders.
						logger.LogInformation(string.Format("The value of {0} is not unique, but one condition specifies {0} = {1}.", exp.Key.Name, intp));
					}
					else
					{
						logger.LogInformation("The value of {0} is not unique.", exp.Key.Name);
						//We just leave the variable there.
						//throw new NotSupportedException();
					}
				}
				else
				{
					logger.LogInformation("{0} = {1}.", exp.Key.Name, intp);
				}


				if (intp is Z3.IntNum intpInt)
				{
					conditions.Add(new PaperVariable(exp.Key.Name.ToString()), new PaperInt { Value = intpInt.Int });
				}
				else if (intp != null)
				{
					if (char.IsLower(intp.ToString()[0]))
						conditions.Add(new PaperVariable(exp.Key.Name.ToString()), new PaperVariable(intp.ToString()));
					else
						conditions.Add(new PaperVariable(exp.Key.Name.ToString()), new ConcreteType(intp.ToString()));
				}
			}
			return conditions;
		}

		static Z3.Expr FindEqualtyAssignment(Z3.BoolExpr[] andExprs, Z3.Symbol leftHandName)
		{
			foreach (var exp in andExprs)
			{
				if (exp.IsEq)
				{
					if (exp.Arg(0).ToString().Equals(leftHandName.ToString()))
					{
						return exp.Arg(1);
					}
					else if (exp.Arg(1).ToString().Equals(leftHandName.ToString()))
					{
						return exp.Arg(0);
					}
				}
				else if (exp.IsAnd)
				{
					var res = FindEqualtyAssignment(exp.Args.Cast<Z3.BoolExpr>().ToArray(), leftHandName);
					if (res != null)
						return res;
				}
			}
			return null;
		}
	}
}
