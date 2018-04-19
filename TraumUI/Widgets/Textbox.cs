using System;
using System.Collections.Generic;

namespace TraumUI.Widgets {
	public class Textbox : IWidget {
		string _Value = "";
		public string Value {
			get => _Value;
			set {
				Tui.Instance.RedrawRequested = true;
				_Value = value ?? "";
				Changed(this, _Value);
			}
		}
		
		public bool Focused;
		bool _Password;
		public bool Password {
			get => _Password;
			set => this.RedrawWith(() => _Password = value);
		}

		int _Width = 16;
		public int Width {
			get => _Width;
			set => this.RedrawWith(() => _Width = value);
		}

		int _InputCursor;
		public int InputCursor {
			get => _InputCursor;
			set => this.RedrawWith(() => _InputCursor = Math.Min(value, _Value.Length));
		}

		public event EventHandler<string> Changed = (_, __) => { };
		
		public int? TabIndex { get; set; }
		public IReadOnlyList<IWidget> Children => new IWidget[0];
		public (int, int) Size((int, int) maxSpace) => (Math.Min(maxSpace.Item1, Width + 4), 1);

		public IReadOnlyList<Rope> Render((int, int) maxSpace) {
			Rope inner = Value.Substring(0, Math.Min(Value.Length, Width));
			if(inner.Length < Width)
				inner += new string(' ', Width - inner.Length);

			return new[] { ("[ " + inner.Underline() + " ]").If(Focused, x => x.Bold()) };
		}

		public void Focus() => this.RedrawWith(() => Focused = true);
		public void Unfocus() => this.RedrawWith(() => Focused = false);
		public void Click() => throw new NotImplementedException();

		public bool Key(ConsoleKeyInfo key) {
			switch(key.Key) {
				case ConsoleKey.Backspace:
					Value = Value.Length == 0 ? "" : Value.Substring(0, Value.Length - 1);
					InputCursor = Value.Length;
					break;
				case ConsoleKey.LeftArrow:
					InputCursor = Math.Max(0, InputCursor - 1);
					break;
				case ConsoleKey.RightArrow:
					InputCursor = Math.Min(Value.Length, InputCursor + 1);
					break;
				default:
					if(key.KeyChar < 0x20)
						break;
					Value += key.KeyChar;
					InputCursor = Value.Length;
					break;
			}
			return true;
		}
	}
}