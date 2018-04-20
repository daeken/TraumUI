using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TermionSharp;

namespace TraumUI {
	public static class Extensions {
		public static RetT Pass<ParamT, RetT>(this ParamT param, Func<ParamT, RetT> func) => func(param);
		public static void Pass<ParamT>(this ParamT param, Action<ParamT> func) => func(param);
		public static ParamT If<ParamT>(this ParamT param, bool condition, Func<ParamT, ParamT> func) =>
			condition ? func(param) : param;

		public static ParamT Do<ParamT>(this ParamT param, Action<ParamT> func) {
			func(param);
			return param;
		}

		public static IEnumerable<ParamT> Reversed<ParamT>(this IEnumerable<ParamT> list) => list.AsEnumerable().Reverse();

		public static void RedrawWith(this IWidget widget, Action func) {
			func();
			Tui.Instance.RedrawRequested = true;
		}

		public static IEnumerable<IWidget> AndDescendents(this IWidget widget) {
			yield return widget;
			foreach(var child in widget.Children)
				foreach(var elem in child.AndDescendents())
					yield return elem;
		}

		public static IEnumerable<(int, IWidget)> WalkTabIndex(this IWidget widget, int prev = 1) {
			var cur = prev * 100 + widget.TabIndex ?? 1;
			if(widget.TabIndex != null)
				yield return (cur, widget);
			foreach(var child in widget.Children)
				foreach(var elem in child.WalkTabIndex(cur))
					yield return elem;
		}

		public static IWidget SetParent(this IWidget widget, IWidget parent, IWidget previous = null) {
			if(previous != null) previous.Parent = null;
			widget.Parent = parent;
			return widget;
		}
	}
}