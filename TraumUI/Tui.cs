using System;
using System.Linq;
using System.Threading;
using TermionSharp;

namespace TraumUI {
	public class Tui {
		public const char CursorPlaceholder = '\x06';
		public static Tui Instance { get; private set; }
		
		bool Exiting;
		readonly Terminal Term = new Terminal();
		public IWidget Root;

		int CurrentTabIndex = -1;
		IWidget Focused;
		
		bool _RedrawRequested;
		public bool RedrawRequested {
			get => _RedrawRequested;
			set => _RedrawRequested = _RedrawRequested || value;
		}
		public static void RequestRedraw() => Instance.RedrawRequested = true;

		public Tui() {
			if(Instance != null)
				throw new NotImplementedException();
			Instance = this;
			Term.CursorVisiblity = false;
		}

		public void Redraw() {
			lock(this) {
				_RedrawRequested = false;
				if(Root == null) return;
				var data = Root.Render(Term.Size);
				Term.Clear();
				Term.CursorVisiblity = false;
				(int, int)? cursorAt = null;
				for(var y = 0; y < data.Count; ++y) {
					Term.CursorPosition = (0, y);
					var line = data[y];
					var offset = 0;
					foreach(var piece in line) {
						if((piece.Decorations & Decoration.CursorAtStart) != 0) {
							cursorAt = (offset, y);
							break;
						}
						offset += piece.Length;
					}
					Term.Write(line.ToAnsiString());
				}

				if(cursorAt != null) {
					Term.CursorPosition = cursorAt.Value;
					Term.CursorVisiblity = true;
				}
			}
		}

		public void Run() {
			Term.Resize += Redraw;
			TabCycle();
			Redraw();
			
			while(!Exiting) {
				if(RedrawRequested)
					Redraw();
				var key = Term.ReadKey();
				switch(key.Key) {
					case ConsoleKey.Tab:
						TabCycle((key.Modifiers & ConsoleModifiers.Shift) != 0);
						break;
					case ConsoleKey.Spacebar:
						if(!Bubble(x => x.Key(key)))
							Focused?.Click();
						break;
					case ConsoleKey.Q:
						if(!Bubble(x => x.Key(key)))
							Exit();
						break;
					default:
						Bubble(x => x.Key(key));
						break;
				}
				Thread.Sleep(16);
			}
			
			Term?.Dispose();
		}

		bool Bubble(Func<IWidget, bool> func) {
			var cur = Focused;
			while(cur != null) {
				if(func(cur))
					return true;
				cur = cur.Parent;
			}

			return false;
		}

		void TabCycle(bool backwards = false) {
			var elems = Root.WalkTabIndex().OrderBy(x => x.Item1).ToList();
			if(elems.Count == 0) {
				if(Focused != null) {
					Focused.Unfocus();
					Focused = null;
					CurrentTabIndex = -1;
				}

				return;
			}

			if(backwards)
				elems.Reverse();
			var next = elems.First();
			foreach(var (index, widget) in elems) {
				if((backwards && index < CurrentTabIndex) || (!backwards && index > CurrentTabIndex)) {
					next = (index, widget);
					break;
				}
			}
			CurrentTabIndex = next.Item1;
			if(Focused == next.Item2) return;
			Focused?.Unfocus();
			Focused = next.Item2;
			Focused.Focus();
		}

		public void Exit() {
			Exiting = true;
			Term.CursorPosition = (0, Term.Size.H - 1);
			Term.Write("\n");
			Term?.Dispose();
			Environment.Exit(0);
		}
	}
}