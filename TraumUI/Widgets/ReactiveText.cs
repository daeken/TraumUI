using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class ReactiveText : BaseWidget {
		Func<ReactiveText, Rope> BFunc;
		public Func<ReactiveText, Rope> Func {
			get => BFunc;
			set {
				Tui.RequestRedraw();
				BFunc = value;
			}
		}

		public ReactiveText(Func<ReactiveText, Rope> func) => BFunc = func;
		public ReactiveText(Func<Rope> func) => BFunc = _ => func();
		public ReactiveText(Func<string> func) => BFunc = _ => func();

		public override (int, int) Size((int, int) maxSpace) {
			var lines = BFunc(this).Split('\n');
			return (lines.Select(x => x.Length).Aggregate(0, Math.Max, x => x), lines.Length);
		}

		public override IReadOnlyList<Rope> Render((int, int) maxSpace) => BFunc(this).Split('\n');
		
		public static explicit operator ReactiveText(Func<ReactiveText, Rope> func) => new ReactiveText(func);
		public static explicit operator ReactiveText(Func<Rope> func) => new ReactiveText(func);
		public static explicit operator ReactiveText(Func<string> func) => new ReactiveText(func);
	}
}