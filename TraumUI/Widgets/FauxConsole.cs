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
		public bool Linewrap = false;

		string[] Buffer = new string[1000];
		(int, int) Position = (0, 0);

		public void SetActive() => Console.SetOut(this);

		public int? TabIndex { get; set; }
		public IReadOnlyList<IWidget> Children => Enumerable.Empty<IWidget>().ToList();
		public (int, int) Size((int, int) maxSpace) => maxSpace;

		public IReadOnlyList<string> Render((int, int) maxSpace) {
			var oarray = new string[Math.Min(maxSpace.Item2, Buffer.Length)];
			var oi = 0;
			for(var i = 0; i < oarray.Length && oi < oarray.Length; ++i) {
				var line = Buffer[i];
				if(line == null) {
					oarray[oi++] = "";
					continue;
				}

				if(!Linewrap || line.AnsiLength() <= maxSpace.Item1)
					oarray[oi++] = line;
				else
					for(var j = 0; j < line.AnsiLength() && oi < oarray.Length; j += maxSpace.Item1)
						oarray[oi++] = line.AnsiSubstring(j, Math.Min(line.AnsiLength() - j, maxSpace.Item1));
			}
			return oarray;
		}

		public void Focus() {}
		public void Unfocus() {}
		public void Click() {}

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

			Tui.Instance.RedrawRequested = true;
		}

		public override Encoding Encoding => Encoding.UTF8;
		public override string NewLine { get => "\n"; set => throw new NotImplementedException(); }
	}
}