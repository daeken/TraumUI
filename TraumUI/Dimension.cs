using System;

namespace TraumUI {
	public abstract class Dimension {
		public abstract int Value(int parent, int max);
	}

	public class FixedDimension : Dimension {
		readonly int BValue;

		public FixedDimension(int value) => BValue = value;
		public override int Value(int parent, int max) => BValue;
	}

	public class PercentageDimension : Dimension {
		readonly int Percentage;

		public PercentageDimension(int percentage) => Percentage = percentage;
		public override int Value(int parent, int max) => (int) Math.Round(Percentage / 100.0 * parent);
	}

	public class StretchDimension : Dimension {
		public override int Value(int parent, int max) => max;
	}
}