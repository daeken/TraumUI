using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TraumUI.Widgets {
	public class FauxConsole : TextWriter, IWidget {
		public int BufferLines {
			get => Buffer.Length;
			set {
				if(value == Buffer.Length) return;
				if(value < Buffer.Length)
					Buffer = Buffer.Skip(Buffer.Length - value).Take(value).ToArray();
				else
					Buffer = Enumerable.Range(0, value - Buffer.Length).Select(x => (string) null).Concat(Buffer).ToArray();
			}
		}
		public bool Linewrap = true;

		string[] Buffer = new string[1000];
		(int, int) Position = (0, 0);

		public void SetActive() => Console.SetOut(this);

		public IWidget Parent { get; set; }
		public int? TabIndex { get; set; }
		public IReadOnlyList<IWidget> Children => Enumerable.Empty<IWidget>().ToList();
		public (int, int) Size((int, int) maxSpace) => maxSpace;

		public IReadOnlyList<Rope> Render((int, int) maxSpace) {
			var oarray = new List<Rope>();
			for(var i = 0; i < Buffer.Length; ++i) {
				var line = Buffer[i];
				if(line == null)
					break;

				if(!Linewrap || line.Length <= maxSpace.Item1)
					oarray.Add(line);
				else
					for(var j = 0; j < line.Length; j += maxSpace.Item1)
						oarray.Add(line.Substring(j, Math.Min(line.Length - j, maxSpace.Item1)));
			}
			return oarray;
		}

		public void Focus() {}
		public void Unfocus() {}
		public void Click() {}
		public bool Key(ConsoleKeyInfo key) => false;

		public override void Write(char value) {
			if(value == '\r') return;
			if(value == '\n') {
				Position = (0, Position.Item2 + 1);
				if(Position.Item2 != Buffer.Length)
					return;
				Position = (0, Position.Item2 - 1);
				Buffer = Buffer.Skip(1).Concat(new[] { "" }).ToArray();
			} else
				Buffer[Position.Item2] = (Buffer[Position.Item2] ?? "") + value;

			Tui.RequestRedraw();
		}

		public override Encoding Encoding => Encoding.UTF8;
		public override string NewLine { get => "\n"; set => throw new NotImplementedException(); }
	}
}