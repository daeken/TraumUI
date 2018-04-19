﻿using TraumUI.Widgets;
using static System.Console;

namespace TraumUI.ExampleApp {
	class Program {
		static void Main(string[] args) {
			using(var term = new Tui()) {
				var tree = new SplitPanel(SplitDirection.Horizontal);
				term.Root = tree;
				var left = new Window($"Left window");
				left.Body = new FauxConsole().Do(x => x.SetActive());
				tree.Add(new PercentageDimension(70), left);
				var right = new SplitPanel(SplitDirection.Vertical);
				tree.Add(new PercentageDimension(30), right);
				var ur = new Window("Upper right window");
				var temp = "";
				for(var i = 0; i < 100; ++i)
					temp += $"{i} foo bar\n";
				ur.Body = new ScrollContainer {
					Body = new Text("Testing text goes here and here and here and here and here and scrolls real real nice\n" + temp) { TabIndex = 5 }
				};
				var lr = new Window("Lower right window!");
				var grid = new Grid(rows: 3) { HPadding = 2 };
				grid.Add(0, 0, new Checkbox { TabIndex = 2 });
				grid.Add(1, 0, new Text("Checkbox 1"));
				var text = new Text("Checkbox 2!!!\nAnd some more\nAnd more");
				grid.Add(1, 1, text);
				grid.Add(0, 1, new Checkbox { TabIndex = 3 }.Do(cb => cb.Changed += (_, state) => text.Value = $"Checkbox 2 is {(state ? "checked" : "unchecked")}"));
				grid.Add(1, 2, new Textbox { TabIndex = 4 }.Do(tb => tb.Changed += (_, tt) => WriteLine($"Textbox changed to '{tt}'")));
				lr.Body = grid;
				right.Add(new FixedDimension(25), ur);
				right.Add(new StretchDimension(), lr);
			}
		}
	}
}