using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class Text : BaseWidget {
		Rope BValue;
		public Rope Value {
			get => BValue;
			set {
				Tui.Instance.RedrawRequested = true;
				BValue = value ?? Rope.Empty;
			}
		}
		
		public Text(Rope value = null) => Value = value;

		public override (int, int) Size((int, int) maxSpace) {
			var lines = Value.Split('\n');
			return (lines.Select(x => x.Length).Aggregate(0, Math.Max, x => x), lines.Length);
		}

		public override IReadOnlyList<Rope> Render((int, int) maxSpace) => Value.Split('\n');
	}
}