using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class ScrollContainer : BaseWidget {
		IWidget _Body;
		public IWidget Body {
			get => _Body;
			set => _Body = value.SetParent(this, previous: Body);
		}

		int ScrollX, ScrollY;
		(int, int) LastMax;
		
		public override IReadOnlyList<IWidget> Children => new[] { Body };
		public override (int, int) Size((int, int) maxSpace) => maxSpace;

		public override IReadOnlyList<Rope> Render((int, int) maxSpace) {
			LastMax = maxSpace;
			var ren = Body?.Render(maxSpace);
			if(ren == null) return new Rope[0];

			ScrollY = Math.Min(ScrollY, Math.Max(0, ren.Count - maxSpace.Item2 - 1));
			var longestLine = ren.Select(x => x.Length).Max();
			if(ScrollY != 0)
				ren = ren.Skip(ScrollY).ToList();
			if(ren.Count > maxSpace.Item2)
				ren = ren.Take(maxSpace.Item2).ToList();
			if(ScrollX == -1)
				ScrollX = longestLine;
			ScrollX = Math.Min(ScrollX, Math.Max(0, longestLine - maxSpace.Item1 - 1));
			if(ScrollX != 0)
				ren = ren.Select(x => x.Substring(ScrollX)).ToList();

			return ren;
		}

		public override bool Key(ConsoleKeyInfo key) {
			switch(key.Key) {
				case ConsoleKey.UpArrow:
					ScrollY = Math.Max(0, ScrollY - 1);
					break;
				case ConsoleKey.DownArrow:
					ScrollY += 1;
					break;
				case ConsoleKey.LeftArrow:
					ScrollX = Math.Max(0, ScrollX - 1);
					break;
				case ConsoleKey.RightArrow:
					ScrollX += 1;
					break;
				case ConsoleKey.PageUp:
					ScrollY = Math.Max(0, ScrollY - LastMax.Item2);
					break;
				case ConsoleKey.PageDown:
					ScrollY += LastMax.Item2;
					break;
				case ConsoleKey.Home:
					ScrollX = 0;
					break;
				case ConsoleKey.End:
					ScrollX = -1;
					break;
				default:
					return false;
			}
			Tui.Instance.RedrawRequested = true;
			return true;
		}
	}
}