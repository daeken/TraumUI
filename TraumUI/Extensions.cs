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

		public static string Foreground(this string text, Color color) => $"{Terminal.Foreground(color)}{text}";
		public static string Background(this string text, Color color) => $"{Terminal.Background(color)}{text}";
		public static string Bold(this string text) => text.Foreground(Color.Bold).Reset();
		public static string Underline(this string text) => text.Foreground(Color.Underline).Reset();
		public static string Reset(this string text) {
			var reset = Terminal.Foreground(Color.Reset);
			return text.EndsWith(reset) ? text : text + reset;
		}

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

		public static int AnsiLength(this string str) {
			var length = 0;
			for(var i = 0; i < str.Length; ++i) {
				if(str[i] == '\x1b') {
					i++;
					if(str.Length == i || str[i] != '[') continue;
					i++;
					for(; i < str.Length; ++i)
						if(str[i] < 0x30 || str[i] > 0x3f)
							break;
					for(; i < str.Length; ++i)
						if(str[i] < 0x20 || str[i] > 0x2f)
							break;
					Debug.Assert(str[i] >= 0x40 && str[i] <= 0x7e);
				} else
					length++;
			}
			return length;
		}

		public static string AnsiSubstring(this string str, int offset, int length) {
			var start = str.FindAnsiOffset(offset);
			var end = str.FindAnsiOffset(length, start);
			var ret = end == -1 || end >= str.Length ? str.Substring(start) : str.Substring(start, end - start);
			return ret.Contains("\x1b") ? ret + Terminal.Foreground(Color.Reset) : ret;
		}

		public static int FindAnsiOffset(this string str, int offset, int? start = null) {
			if(offset == 0) return start ?? 0;
			var length = 0;
			for(var i = start ?? 0; i < str.Length; ++i) {
				if(str[i] == '\x1b') {
					var si = i;
					if(str.Length == ++i || str[i++] != '[') continue;
					for(; i < str.Length; ++i)
						if(str[i] < '0' || str[i] > '?')
							break;
					for(; i < str.Length; ++i)
						if(str[i] < ' ' || str[i] > '/')
							break;
					Debug.Assert(str[i] >= '@' && str[i] <= '~');
				} else {
					if(i != start)
						length++;
					if(length == offset)
						return (start ?? 0) + i + 1;
				}
			}
			return -1;
		}
	}
}