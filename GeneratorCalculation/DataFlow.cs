using System;
using System.Diagnostics.CodeAnalysis;

namespace GeneratorCalculation
{

	public class DataFlow : IEquatable<DataFlow>
	{
		public DataFlow(Direction direction, PaperType type)
		{
			Direction = direction;
			Type = type;
		}

		public Direction Direction { get; }

		public PaperType Type { get; }


		// override object.Equals
		public override bool Equals(object obj)
		{
			if (obj is DataFlow objD)
				return Equals(objD);
			return false;
		}

		public bool Equals([AllowNull] DataFlow other)
		{
			return other.Direction == Direction && other.Type.Equals(Type);
		}

		public override int GetHashCode()
		{
			throw new NotSupportedException(nameof(DataFlow) + " cannot be used as key in a hashtable or a dictionary.");
			//return 0;
		}
	}

	public enum Direction
	{
		Yielding,
		Resuming
	}
}
