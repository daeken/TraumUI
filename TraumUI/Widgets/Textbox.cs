﻿using System;
using System.Collections.Generic;

namespace TraumUI.Widgets {
	public class Textbox : FocusableWidget {
		string _Value = "";
		public string Value {
			get => _Value;
			set {
				Tui.RequestRedraw();
				_Value = value ?? "";
				Changed(this, _Value);
			}
		}
		
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
		
		public override (int, int) Size((int, int) maxSpace) => (Math.Min(maxSpace.Item1, Width + 4), 1);

		public override IReadOnlyList<Rope> Render((int, int) maxSpace) {
			Rope inner = Value.Substring(0, Math.Min(Value.Length, Width));
			if(inner.Length < Width)
				inner += new string(' ', Width - inner.Length);
			return new[] { ("[ " + inner.Underline() + " ]").If(Focused, x => x.Bold().If(InputCursor < Math.Min(Width, maxSpace.Item1 - 3), y => y.CursorAt(2 + InputCursor))) };
		}

		public override bool Key(ConsoleKeyInfo key) {
			switch(key.Key) {
				case ConsoleKey.Backspace:
					if(Value.Length != 0)
						Value = Value.Substring(0, Value.Length - 1);
					InputCursor = Value.Length;
					break;
				case ConsoleKey.LeftArrow:
					if(key.Modifiers.HasFlag(ConsoleModifiers.Alt)) return false;
					InputCursor = Math.Max(0, InputCursor - 1);
					break;
				case ConsoleKey.RightArrow:
					if(key.Modifiers.HasFlag(ConsoleModifiers.Alt)) return false;
					InputCursor = Math.Min(Value.Length, InputCursor + 1);
					break;
				case ConsoleKey.Home:
					InputCursor = 0;
					break;
				case ConsoleKey.End:
					InputCursor = Value.Length;
					break;
				default:
					if(key.KeyChar < 0x20)
						return false;
					Value += key.KeyChar;
					InputCursor = Value.Length;
					break;
			}
			return true;
		}
	}
}