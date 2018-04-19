using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;

namespace TraumUI {
	public enum Color {
		Black, 
		Red, 
		Green, 
		Yellow, 
		Blue, 
		Purple, 
		Cyan, 
		White
	}

	[Flags]
	public enum Decoration {
		Bold = 1, 
		Underline = 2, 
		Bright = 4, 
		BrightBackground = 8, 
		Blinking = 16, 
		CursorAtStart = 32
	}

	public class Rope : List<TextPiece> {
		public static readonly Rope Empty = new Rope(new TextPiece(""));
		public int Length => this.Select(x => x.Length).Sum();
		
		public Rope() {}
		public Rope(TextPiece text) => Add(text);
		public Rope(IEnumerable<TextPiece> e) : base(e) {}
		
		public static implicit operator Rope(string text) => string.IsNullOrEmpty(text) ? Empty : new Rope(text);
		public static implicit operator Rope(TextPiece text) => new Rope(text);
		public static Rope operator +(Rope left, Rope right) {
			if(left == null || left == Empty) return right;
			if(right == null || right == Empty) return left;
			return left.Concat(right).ToRope();
		}
		public static Rope operator +(string left, Rope right) => (Rope) left + right;

		public Rope Substring(int start) => Substring(start, Length - start);
		public Rope Substring(int start, int count) {
			if(start == 0 && count >= Length) return this;
			var sc = FindChunkForIndex(start);
			var ec = FindChunkForIndex(start + count);
			if(ec.Chunk == -1) ec = (Count - 1, this.Last().Length);

			Rope ret;

			if(sc.Offset == 0 && sc.Chunk != ec.Chunk) {
				var sub = this[sc.Chunk];
				if(count != sub.Length)
					sub = sub.Substring(0, count);
				ret = sub;
			} else if(sc.Chunk != ec.Chunk)
				ret = this[sc.Chunk].Substring(sc.Offset);
			else // Only one chunk in play
				return this[sc.Chunk].Substring(sc.Offset, count);

			for(var i = sc.Chunk + 1; i < ec.Chunk; ++i)
				ret += this[i];

			if(ec.Chunk < Count) {
				var sub = this[ec.Chunk];
				ret += sub.Length == ec.Offset ? sub : sub.Substring(0, ec.Offset);
			}

			return ret;
		}

		public int IndexOf(string data, int offset = 0) {
			var all = this.Select(x => x.Text).Aggregate((a, x) => a + x);
			return all.IndexOf(data, offset, StringComparison.Ordinal);
		}

		(int Chunk, int Offset) FindChunkForIndex(int offset) {
			if(offset == 0) return (0, 0);

			var seen = 0;
			for(var i = 0; i < Count; ++i) {
				if(seen <= offset && seen + this[i].Length > offset)
					return (i, offset - seen);
				seen += this[i].Length;
			}
			return (-1, -1);
		}

		public Rope[] Split(char on) => Split(on.ToString());
		public Rope[] Split(string on) {
			var ret = new List<Rope>();
			var offset = 0;
			while(true) {
				var next = IndexOf(on, offset);
				if(next == -1) {
					if(offset != 0)
						ret.Add(Substring(offset));
					else
						ret.Add(this);
					break;
				}
				ret.Add(Substring(offset, next - offset));
				offset = next + on.Length;
			}
			return ret.ToArray();
		}

		public Rope Bold() => this.Select(x => x.Bold()).ToRope();
		public Rope Underline() => this.Select(x => x.Underline()).ToRope();

		public string ToAnsiString() => string.Join("", this.Select(x => x.ToAnsiString()));

		public void Debug() {
			Console.Error.WriteLine($"Rope with length {Length} across {Count} chunks:");
			foreach(var chunk in this)
				Console.Error.WriteLine($"- length {chunk.Length} - '{chunk.Text}'");
		}
	}
	
	public struct TextPiece {
		public static readonly TextPiece Empty = "";
		
		public readonly string Text;
		public readonly Color Foreground, Background;
		public readonly Decoration Decorations;

		public int Length => Text.Length;

		public TextPiece(string text, Color foreground = 0, Color background = 0, Decoration decorations = 0) {
			Text = text;
			Foreground = foreground;
			Background = background;
			Decorations = decorations;
		}

		public static implicit operator TextPiece(string text) => new TextPiece(text);
		
		public static Rope operator +(TextPiece left, TextPiece right) => new Rope { left, right };
		public static Rope operator +(Rope left, TextPiece right) =>
			left.Concat(new Rope { right }).ToRope();

		public TextPiece Repeat(int count) {
			var text = Text;
			return new TextPiece(
				string.Join("", Enumerable.Range(0, count).Select(_ => text)), 
				Foreground, Background, 
				Decorations
			);
		}

		public TextPiece Substring(int start) => Substring(start, Length - start);
		public TextPiece Substring(int start, int count) =>
			start + count >= Length && start == 0 ? this : count < 0 ? Empty : new TextPiece(Text.Substring(start, count), Foreground, Background, Decorations);
		
		public TextPiece Bold() => new TextPiece(Text, Foreground, Background, Decorations | Decoration.Bold);
		public TextPiece Underline() => new TextPiece(Text, Foreground, Background, Decorations | Decoration.Underline);

		public static TextPiece operator *(TextPiece piece, int count) => piece.Repeat(count);

		public string ToAnsiString() {
			if(Decorations == 0 && Foreground == 0 && Background == 0)
				return Text;
			var fg = (TermionSharp.Color) Foreground;
			var bg = (TermionSharp.Color) Background;
			if((Decorations & Decoration.Bold) != 0) fg |= TermionSharp.Color.Bold;
			if((Decorations & Decoration.Underline) != 0) fg |= TermionSharp.Color.Underline;
			if((Decorations & Decoration.Bright) != 0) fg |= TermionSharp.Color.Bright;
			if((Decorations & Decoration.BrightBackground) != 0) bg |= TermionSharp.Color.Bright;
			var ret = Text;
			if(bg != 0) ret = TermionSharp.Terminal.Background(bg) + ret;
			if(fg != 0) ret = TermionSharp.Terminal.Foreground(fg) + ret;
			ret += TermionSharp.Terminal.Foreground(TermionSharp.Color.Reset);
			return ret;
		}
	}

	public static class TextPieceExtensions {
		public static Rope ToRope(this IEnumerable<TextPiece> e) => new Rope(e);
	}
}