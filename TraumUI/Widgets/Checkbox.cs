﻿using System;
using System.Collections.Generic;
using System.Linq;
using TraumUI;

namespace TraumUI.Widgets {
	public class Checkbox : IWidget {
		bool _Checked;
		public bool Checked {
			get => _Checked;
			set {
				_Checked = value;
				Changed(this, _Checked);
			}
		}
		public bool Focused;

		public event EventHandler<bool> Changed = (_, __) => { };

		public int? TabIndex { get; set; }
		public IReadOnlyList<IWidget> Children => Enumerable.Empty<IWidget>().ToList();
		public (int, int) Size((int, int) maxSpace) => (3, 1);
		public IReadOnlyList<string> Render((int, int) maxSpace) => new[] {
			(Checked ? "<⁜>" : "< >").If(Focused, x => x.Bold().Underline())
		};
		public void Focus() => this.RedrawWith(() => Focused = true);
		public void Unfocus() => this.RedrawWith(() => Focused = false);
		public void Click() => this.RedrawWith(() => Checked = !Checked);
	}
}