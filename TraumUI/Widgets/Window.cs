using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class Window : IWidget {
		public string Title;
		public IWidget Body;

		public Window(string title = "") => Title = title;

		public int? TabIndex {
			get => null;
			set => throw new NotImplementedException();
		}
		public IReadOnlyList<IWidget> Children => new [] { Body };
		public (int, int) Size((int, int) maxSpace) => maxSpace;

		public IReadOnlyList<string> Render((int, int) maxSpace) {
			var oarray = new string[Math.Max(maxSpace.Item2, 3)];

			oarray[0] = "┏━ " + Title.AnsiSubstring(0, Math.Max(0, Math.Min(Title.AnsiLength(), maxSpace.Item1 - 5))) + " " + new string('━', Math.Max(0, maxSpace.Item1 - Title.AnsiLength() - 5)) + "┓";

			var inner = Body?.Render((maxSpace.Item1 - 4, maxSpace.Item2 - 2)) ?? Enumerable.Empty<string>().ToList();
			if(inner.Count != maxSpace.Item2 - 2)
				inner = inner.Concat(Enumerable.Range(0, maxSpace.Item2 - 2 - inner.Count).Select(x => "")).ToList();
			var y = 1;
			foreach(var _line in inner) {
				var line = _line ?? " ";
				oarray[y++] = "┃ " + line.AnsiSubstring(0, Math.Min(line.AnsiLength(), maxSpace.Item1 - 3)) +
				              new string(' ', Math.Max(0, maxSpace.Item1 - line.AnsiLength() - 4)) +
				              (line.AnsiLength() <= maxSpace.Item1 - 4 ? " " : "") + "┃";
			}

			oarray[y] = "┗" + new string('━', Math.Max(maxSpace.Item1 - 2, 0)) + "┛";
			
			return oarray;
		}

		public void Focus() {
			throw new NotImplementedException();
		}

		public void Unfocus() {
			throw new NotImplementedException();
		}
		
		public void Click() {}
	}
}