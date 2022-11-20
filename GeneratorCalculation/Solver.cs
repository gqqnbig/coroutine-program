using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GeneratorCalculation
{
	public class Solver
	{

		public static Dictionary<PaperVariable, PaperWord> JoinConditions(List<Dictionary<PaperVariable, PaperWord>> conditions)
		{
			var c = new Dictionary<PaperVariable, PaperWord>();

			for (int i = 0; i < conditions.Count; i++)
			{
				c = JoinConditions(c, conditions[i]);
				if (c == null)
					return null;

			}
			return c;
		}


		public static Dictionary<PaperVariable, PaperWord> JoinConditions(Dictionary<PaperVariable, PaperWord> x, Dictionary<PaperVariable, PaperWord> y)
		{
			var k1 = new List<PaperVariable>(x.Keys);
			var k2 = new List<PaperVariable>(y.Keys);

			var duplicateKeys = k1.Intersect(k2).ToList();
			if (duplicateKeys.Count == 0)
			{
				//no potential conflicting keys
				return x.Concat(y).ToDictionary(d => d.Key, d => d.Value);
			}


			List<Dictionary<PaperVariable, PaperWord>> conditions = new List<Dictionary<PaperVariable, PaperWord>>();
			foreach (var key in duplicateKeys)
			{
				var c = x[key].IsCompatibleTo(y[key]);
				if (c == null)
					c = y[key].IsCompatibleTo(x[key]);

				conditions.Add(c);
			}

			var solveDuplication = JoinConditions(conditions);
			if (solveDuplication == null)
				return null;

			foreach (var key in duplicateKeys)
			{
				x.Remove(key);
				y.Remove(key);
			}

			var c3 = JoinConditions(x, y);
			return JoinConditions(c3, solveDuplication);
		}


		public static string FormatCondition(KeyValuePair<PaperVariable, PaperWord> p)
		{
			return $"{p.Key}/{p.Value}";
		}
	}
}
