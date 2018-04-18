using TermionSharp;
using TraumUI;
using TraumUI.Widgets;
using static System.Console;

namespace XppEmu {
	class Program {
		static void Main(string[] args) {
			using(var term = new Tui()) {
				var tree = new SplitPanel(SplitDirection.Horizontal);
				term.Root = tree;
				var left = new Window($"Left {Terminal.Foreground(Color.Purple | Color.Bright)}window");
				left.Body = new FauxConsole().Do(x => x.SetActive());
				tree.Add(new PercentageDimension(70), left);
				var right = new SplitPanel(SplitDirection.Vertical);
				tree.Add(new PercentageDimension(30), right);
				var ur = new Window("Upper right window");
				ur.Body = new Text("Testing text goes here and here and here and here and here and\neven more here");
				var lr = new Window("Lower right window!");
				var grid = new Grid(rows: 2) { HPadding = 2 };
				grid.Add(0, 0, new Checkbox { TabIndex = 2 });
				grid.Add(1, 0, new Text("Checkbox 1\nAnd more text here\nand more!"));
				var text = new Text("Checkbox 2!!!");
				grid.Add(1, 1, text);
				grid.Add(0, 1, new Checkbox { TabIndex = 3 }.Do(cb => cb.Changed += (_, state) => text.Value = $"Checkbox 2 is {(state ? "checked" : "unchecked")}"));
				lr.Body = grid;
				right.Add(new FixedDimension(25), ur);
				right.Add(new StretchDimension(), lr);
			}
		}
	}
}