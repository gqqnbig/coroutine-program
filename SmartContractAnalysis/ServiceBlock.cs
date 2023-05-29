using System;
using System.Collections.Generic;
using System.Text;

namespace SmartContractAnalysis
{
	class ServiceBlock
	{
		public string Name { get; set; }

		public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
	}
}
