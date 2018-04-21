using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TraumUI.Widgets {
	public class TableRow : IEnumerable<IWidget> {
		public readonly IWidget[] Elements;
		public event Action<TableRow> Clicked = _ => { };

		public IWidget this[int idx] {
			get => Elements[idx];
			set => Elements[idx] = value;
		}

		public TableRow(params IWidget[] elements) => Elements = elements;
		public TableRow(params Rope[] elements) => Elements = elements.Select(x => new Text(x)).ToArray();

		public void Click() => Clicked(this);
		
		public static implicit operator TableRow(IWidget[] elems) => new TableRow(elems);
		public IEnumerator<IWidget> GetEnumerator() => ((IReadOnlyList<IWidget>) Elements).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();
	}
	
	public class Table : FocusableWidget, IEnumerable<TableRow> {
		public int HPadding = 1, VPadding;
		int Scroll, Selected, PageHeight;

		readonly IReadOnlyList<(Dimension Width, Rope Name)> Columns;
		readonly List<TableRow> Rows = new List<TableRow>();
		public override IReadOnlyList<IWidget> Children => Rows.SelectMany(x => x.Elements).Where(x => x != null).ToList();
		public int Count => Rows.Count;
		public IEnumerator<TableRow> GetEnumerator() => Rows.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(TableRow row) => this.RedrawWith(() => Rows.Add(row));

		public Table(params (Dimension Width, Rope Name)[] columns) => Columns = columns;

		public override (int, int) Size((int, int) maxSpace) {
			var total = 0;
			foreach(var col in Columns)
				total += col.Width.Value(maxSpace.Item1, maxSpace.Item1 - total) + HPadding;
			return (total - (Columns.Count == 0 ? 0 : HPadding), maxSpace.Item2);
		}

		public override IReadOnlyList<Rope> Render((int, int) maxSpace) {
			var colWidths = new int[Columns.Count];
			var total = 0;
			for(var i = 0; i < Columns.Count; ++i)
				total += (colWidths[i] = Columns[i].Width.Value(maxSpace.Item1, maxSpace.Item1 - total)) + HPadding;
			var colEmpty = colWidths.Select(x => new Rope(" ") * x).ToList();
			
			var headerRope = Rope.Empty;
			var hp = new TextPiece(" ") * HPadding;
			foreach(var (i, col) in Columns.Enumerate()) {
				headerRope += col.Name.Bold().Underline().ToFixed(colWidths[i]);
				if(HPadding != 0 && i != Columns.Count - 1)
					headerRope += hp;
			}

			var lines = new List<Rope> { headerRope };
			Selected = Math.Max(0, Math.Min(Selected, Count - 1));

			var scrollOff = 0;
			var heights = new List<int>();
			var hitSelected = false;
			var lastOne = false;
			if(Scroll > Selected)
				Scroll = Selected;
			foreach(var (_rowI, row) in this.Skip(Scroll).Enumerate()) {
				var rowI = _rowI + Scroll;
				for(var i = 0; i < VPadding; ++i)
					lines.Add(Rope.Empty);
				var rendered = row.Enumerate().Select(x => x.Value.Render((colWidths[x.I], maxSpace.Item2 - 1 - VPadding))).ToList();
				var height = rendered.Select(x => x.Count).Max();

				if(lines.Count + height > maxSpace.Item2) {
					if(rowI == Selected) {
						var linesOff = lines.Count + height - maxSpace.Item2;
						var temp = 0;
						foreach(var pheight in heights.Reversed()) {
							if(temp >= linesOff)
								break;
							temp += VPadding + pheight;
							scrollOff++;
						}

						lines = lines.Take(1 + VPadding).Concat(lines.Skip(1 + VPadding + temp)).ToList();
						hitSelected = true;
					} else if(hitSelected)
						lastOne = true;
				} else if(rowI == Selected)
					hitSelected = true;
				heights.Add(height);
				
				for(var i = 0; i < height; ++i) {
					var line = Rope.Empty;
					foreach(var (j, col) in rendered.Enumerate()) {
						if(i >= col.Count)
							line += colEmpty[j] + hp;
						else
							line += col[i].ToFixed(colWidths[j]) + hp;
					}
					lines.Add(rowI == Selected ? line.ForegroundColor(Color.White, bright: !Focused).BackgroundColor(Color.Black, bright: !Focused) : line);
				}

				if(lastOne)
					break;
			}
			Scroll = Math.Max(0, Scroll + scrollOff);
			PageHeight = heights.Count - scrollOff - (lastOne ? 1 : 0);

			return lines;
		}
		
		public override void Click() {
			if(Selected >= 0 && Selected < Count)
				Rows[Selected]?.Click();
		}

		public override bool Key(ConsoleKeyInfo key) {
			switch(key.Key) {
				case ConsoleKey.UpArrow:
					Selected = Math.Max(Selected - 1, 0);
					Scroll = Math.Min(Scroll, Selected);
					break;
				case ConsoleKey.DownArrow:
					Selected++;
					break;
				case ConsoleKey.PageUp:
					Selected = Math.Max(Selected - PageHeight / 2, 0);
					break;
				case ConsoleKey.PageDown:
					Selected = Math.Min(Selected + PageHeight / 2, Count - 1);
					break;
				case ConsoleKey.Home:
					Selected = 0;
					break;
				case ConsoleKey.End:
					Selected = Count - 1;
					break;
				default:
					return false;
			}

			Tui.Instance.RedrawRequested = true;

			return true;
		}
	}
}