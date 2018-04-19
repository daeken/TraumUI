using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class Text : IWidget {
		Rope BValue;

		public Rope Value {
			get => BValue;
			set {
				Tui.Instance.RedrawRequested = true;
				BValue = value ?? Rope.Empty;
			}
		}
		
		public Text(Rope value = null) => Value = value;

		public int? TabIndex { get; set; }

		public IReadOnlyList<IWidget> Children => Enumerable.Empty<IWidget>().ToList();

		public (int, int) Size((int, int) maxSpace) {
			var lines = Value.Split('\n');
			return (lines.Select(x => x.Length).Aggregate(0, Math.Max, x => x), lines.Length);
		}

		public IReadOnlyList<Rope> Render((int, int) maxSpace) => Value.Split('\n');
		public void Focus() {
			throw new NotImplementedException();
		}

		public void Unfocus() {
			throw new NotImplementedException();
		}
		
		public void Click() {}
		public bool Key(ConsoleKeyInfo key) => false;
	}
}