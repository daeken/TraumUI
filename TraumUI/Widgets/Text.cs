using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class Text : IWidget {
		string BValue;

		public string Value {
			get => BValue;
			set {
				Tui.Instance.RedrawRequested = true;
				BValue = value ?? "";
			}
		}
		
		public Text(string value = "") => Value = value;

		public int? TabIndex { get; set; }

		public IReadOnlyList<IWidget> Children => Enumerable.Empty<IWidget>().ToList();

		public (int, int) Size((int, int) maxSpace) {
			var lines = (Value ?? "").Split('\n');
			return (lines.Select(x => x.AnsiLength()).Aggregate(0, Math.Max, x => x), lines.Length);
		}

		public IReadOnlyList<string> Render((int, int) maxSpace) => (Value ?? "").Split('\n');
		public void Focus() {
			throw new NotImplementedException();
		}

		public void Unfocus() {
			throw new NotImplementedException();
		}
		
		public void Click() {}
	}
}