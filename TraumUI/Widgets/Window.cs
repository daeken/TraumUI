using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class Window : IWidget {
		public Rope Title;
		IWidget _Body;

		public IWidget Body {
			get => _Body;
			set {
				if(_Body != null) _Body.Parent = null;
				_Body = value;
				_Body.Parent = this;
			}
		}

		public Window(Rope title = null) => Title = title ?? Rope.Empty;

		public IWidget Parent { get; set; }
		public int? TabIndex {
			get => null;
			set => throw new NotImplementedException();
		}
		public IReadOnlyList<IWidget> Children => new [] { Body };
		public (int, int) Size((int, int) maxSpace) => maxSpace;

		public IReadOnlyList<Rope> Render((int, int) maxSpace) {
			var oarray = new Rope[Math.Max(maxSpace.Item2, 3)];

			oarray[0] = "┏━ " + Title.Substring(0, Math.Max(0, Math.Min(Title.Length, maxSpace.Item1 - 5))) + " " + new string('━', Math.Max(0, maxSpace.Item1 - Title.Length - 5)) + "┓";

			var inner = Body?.Render((maxSpace.Item1 - 4, maxSpace.Item2 - 2)) ?? Enumerable.Empty<Rope>().ToList();
			if(inner.Count > maxSpace.Item2 - 2)
				inner = inner.Take(maxSpace.Item2 - 2).ToList();
			else if(inner.Count < maxSpace.Item2 - 2)
				inner = inner.Concat(Enumerable.Range(0, maxSpace.Item2 - 2 - inner.Count).Select(x => Rope.Empty)).ToList();
			var y = 1;
			foreach(var _line in inner) {
				var line = _line ?? Rope.Empty;
				var len = line.Length;
				oarray[y++] = "┃ " + line.Substring(0, maxSpace.Item1 - 3) +
				              new string(' ', Math.Max(0, maxSpace.Item1 - len - 4)) +
				              (len <= maxSpace.Item1 - 4 ? " " : "") + "┃";
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
		
		public bool Key(ConsoleKeyInfo key) => false;
	}
}