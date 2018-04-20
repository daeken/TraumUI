using System;
using System.Collections.Generic;
using System.Linq;
using TraumUI;

namespace TraumUI.Widgets {
	public class Checkbox : FocusableWidget {
		bool _Checked;
		public bool Checked {
			get => _Checked;
			set {
				Tui.Instance.RedrawRequested = true;
				_Checked = value;
				Changed(this, _Checked);
			}
		}

		public event EventHandler<bool> Changed = (_, __) => { };

		public override (int, int) Size((int, int) maxSpace) => (3, 1);
		public override IReadOnlyList<Rope> Render((int, int) maxSpace) => new[] {
			(Checked ? (Rope) "<⁜>" : "< >").If(Focused, x => x.Bold().Underline())
		};
		public override void Click() => this.RedrawWith(() => Checked = !Checked);
	}
}