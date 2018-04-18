﻿using System;
using System.Linq;
using System.Threading;
using TermionSharp;

namespace TraumUI {
	public class Tui : IDisposable {
		public static Tui Instance { get; private set; }
		
		bool Disposed, Exiting;
		readonly Terminal Term = new Terminal();
		public IWidget Root;

		int CurrentTabIndex = -1;
		IWidget Focused;
		
		bool _RedrawRequested;
		public bool RedrawRequested {
			get => _RedrawRequested;
			set => _RedrawRequested = _RedrawRequested || value;
		}

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
				for(var y = 0; y < data.Count; ++y) {
					Term.CursorPosition = (0, y);
					Term.Write(data[y]);
				}
			}
		}

		void Run() {
			Term.Resize += Redraw;
			TabCycle();
			Redraw();
			
			while(!Disposed && !Exiting) {
				if(RedrawRequested)
					Redraw();
				var key = Term.ReadKey();
				switch(key.Key) {
					case ConsoleKey.Tab:
						TabCycle((key.Modifiers & ConsoleModifiers.Shift) != 0);
						break;
					case ConsoleKey.Spacebar:
						Focused?.Click();
						break;
					case ConsoleKey.Q:
						Exit();
						break;
				}
				Thread.Sleep(16);
			}
		}

		void TabCycle(bool backwards = false) {
			var elems = Root.WalkTabIndex().ToList();
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

		public void Dispose() {
			lock(this)
				if(Disposed) return;

			Run();

			lock(this) {
				Disposed = true;
				Term?.Dispose();
			}
		}
	}
}