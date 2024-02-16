using GeneratorCalculation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Go
{
	class FuncInfo
	{
		public CoroutineDefinitionType CoroutineType { get; set; }
		public string ChannelType { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is FuncInfo objF)
				return object.Equals(CoroutineType, objF.CoroutineType) && object.Equals(ChannelType, objF.ChannelType);

			return false;
		}
	}
}
