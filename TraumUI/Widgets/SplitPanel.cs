using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public enum SplitDirection {
		Horizontal, 
		Vertical
	}
	
	public class SplitPanel : IWidget {
		public SplitDirection Direction { get; }
		readonly List<(Dimension, IWidget)> _Children = new List<(Dimension, IWidget)>();

		public SplitPanel(SplitDirection direction) => Direction = direction;
		
		public void Add(Dimension dimension, IWidget widget) => _Children.Add((dimension, widget.Do(x => x.Parent = this)));

		public IWidget Parent { get; set; }
		public int? TabIndex { get; set; }
		public IReadOnlyList<IWidget> Children => _Children.Select(x => x.Item2).ToList();

		public (int, int) Size((int, int) maxSpace) {
			if(Direction == SplitDirection.Horizontal) {
				var width = 0;
				foreach(var (dim, _) in _Children)
					width += dim.Value(maxSpace.Item1, maxSpace.Item1 - width);
				return (Math.Min(width, maxSpace.Item1), maxSpace.Item2);
			} else {
				var height = 0;
				foreach(var (dim, _) in _Children)
					height += dim.Value(maxSpace.Item2, maxSpace.Item2 - height);
				return (maxSpace.Item1, Math.Min(height, maxSpace.Item2));
			}
		}

		public IReadOnlyList<Rope> Render((int, int) maxSpace) {
			var osize = Size(maxSpace);
			var oarray = Enumerable.Range(0, osize.Item2).Select(x => Rope.Empty).ToArray();

			if(Direction == SplitDirection.Horizontal) {
				var x = 0;
				foreach(var (dim, widget) in _Children) {
					var sw = Math.Min(dim.Value(maxSpace.Item1, maxSpace.Item1 - x), osize.Item1 - x);
					var sub = widget.Render((sw, maxSpace.Item2));
					for(var i = 0; i < Math.Min(maxSpace.Item2, sub.Count); ++i) {
						var row = sub[i] ?? Rope.Empty;
						if(row.Length > sw)
							row = row.Substring(0, sw);
						else if(row.Length < sw)
							row += new string(' ', sw - row.Length);
						oarray[i] += row;
					}

					x += sw;
					if(x >= osize.Item1)
						break;
				}
			} else {
				var y = 0;
				foreach(var (dim, widget) in _Children) {
					var height = dim.Value(maxSpace.Item2, maxSpace.Item2 - y);
					var sub = widget.Render((maxSpace.Item1, height));
					foreach(var t in sub.Take(Math.Min(sub.Count, height))) {
						oarray[y++] = t == null ? new string(' ', maxSpace.Item1) : t.Substring(0, maxSpace.Item1);
						if(y >= osize.Item2)
							break;
					}

					if(y >= osize.Item2)
						break;
				}
			}
			
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