using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCalculation
{
	public class Bindings
	{
		Dictionary<PaperVariable, PaperWord> dict = new Dictionary<PaperVariable, PaperWord>();

		public void Add(string key, PaperWord value)
		{
			dict.Add(new PaperVariable(key), value);
		}


		public void Add(PaperVariable key, PaperWord value)
		{
			dict.Add(key, value);
		}

		public Dictionary<PaperVariable, PaperWord> GetDict()
		{
			return dict;
		}
	}
}
