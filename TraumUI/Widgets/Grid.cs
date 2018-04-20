using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class Grid : BaseWidget {
		public int Columns {
			get => Rows == 0 ? 0 : _Children[0].Length;
			set {
				if(Columns == value) return;
				if(Columns > value)
					_Children = _Children.Select(row => row.Take(value).ToArray()).ToArray();
				else
					_Children = _Children
						.Select(row => row.Concat(Enumerable.Range(0, value - Columns).Select(x => (IWidget) null)).ToArray()).ToArray();
			}
		}

		public int Rows {
			get => _Children.Length;
			set {
				if(Rows == value) return;
				if(Rows > value)
					_Children = _Children.Take(value).ToArray();
				else
					_Children = _Children.Concat(Enumerable.Range(0, value - Rows).Select(x => Enumerable.Range(0, Columns).Select(_ => (IWidget) null).ToArray())).ToArray();
			}
		}

		public int HPadding = 1, VPadding = 0;

		IWidget[][] _Children;
		public override IReadOnlyList<IWidget> Children => _Children.SelectMany(x => x).Where(x => x != null).ToList();
		public void Add(int column, int row, IWidget widget) => _Children[row][column] = widget.SetParent(this, previous: _Children[row][column]);

		public Grid(int columns = 2, int rows = 2) =>
			_Children = Enumerable.Range(0, rows).Select(y => Enumerable.Range(0, columns).Select(x => (IWidget) null).ToArray()).ToArray();

		(int[] ColumnWidths, int[] RowHeights) CellSizes((int, int) maxSpace) {
			if(Rows == 0 || Columns == 0) return (new int[0], new int[0]);
			var columnWidths = new int[Columns];
			var twidth = 0;
			for(var column = 0; column < Columns; ++column) {
				var cur = 0;
				for(var row = 0; row < Rows; ++row)
					if(_Children[row][column] != null)
						cur = Math.Max(_Children[row][column].Size((maxSpace.Item1 - twidth, maxSpace.Item2)).Item1, cur);

				if(twidth + cur >= maxSpace.Item1)
					cur = maxSpace.Item1 - twidth;
				columnWidths[column] = cur;
				twidth += cur;
				if(twidth == maxSpace.Item1)
					break;
			}
			
			var rowHeights = new int[Rows];
			var theight = 0;
			for(var row = 0; row < Rows; ++row) {
				var cur = 0;
				for(var column = 0; column < Columns; ++column)
					if(_Children[row][column] != null)
						cur = Math.Max(_Children[row][column].Size((columnWidths[column], maxSpace.Item2 - theight)).Item2, cur);
				if(theight + cur >= maxSpace.Item2)
					cur = maxSpace.Item2 - theight;
				rowHeights[row] = cur;
				theight += cur;
				if(theight == maxSpace.Item2)
					break;
			}

			return (columnWidths, rowHeights);
		}

		public override (int, int) Size((int, int) maxSpace) {
			if(Rows == 0 || Columns == 0) return (0, 0);
			var (columnWidths, rowHeights) = CellSizes(maxSpace);
			return (columnWidths.Sum() + HPadding * (Columns - 1), rowHeights.Sum() + VPadding * (Rows - 1));
		}

		public override IReadOnlyList<Rope> Render((int, int) maxSpace) {
			var (columnWidths, rowHeights) = CellSizes(maxSpace);
			var olines = new Rope[rowHeights.Sum() + VPadding * (Rows - 1)];
			var voff = 0;
			var hs = new string(' ', HPadding);
			for(var row = 0; row < Rows; ++row) {
				var height = rowHeights[row];
				for(var column = 0; column < Columns; ++column) {
					var child = _Children[row][column];
					var width = columnWidths[column];
					var ren = child?.Render((width, height)) ?? new Rope[height];
					if(ren.Count > height)
						ren = ren.Take(height).ToList();
					else if(ren.Count < height)
						ren = ren.Concat(Enumerable.Range(0, height - ren.Count).Select(x => Rope.Empty)).ToList();

					for(var i = 0; i < height; ++i) {
						var line = ren[i] ?? Rope.Empty;
						var al = line.Length;
						if(al > width)
							line = line.Substring(0, width);
						else if(al < width)
							line += new string(' ', width - al);
						line += hs;
						olines[voff + i] += line;
					}
				}

				voff += height + VPadding;
			}
			return olines;
		}
	}
}