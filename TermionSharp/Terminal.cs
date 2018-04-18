using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TermionSharp {
	[Flags]
	public enum Color : byte {
		Black, 
		Red, 
		Green, 
		Yellow, 
		Blue, 
		Purple, 
		Cyan, 
		White, 
		Bold=32, 
		Bright=64, 
		Underline=128, 
		Reset=255
	}
	
	public class Terminal : IDisposable {
		readonly IPlatformBits Platform;
		readonly Termios OldTermios;

		public (int W, int H) Size {
			get {
				var psize = Platform.Size;
				return (psize.Cols, psize.Rows);
			}
		}

		public event Action Resize;

		public static string Csi(string x) => "\x1B[" + x;

		public static string Foreground(Color x) {
			if(x == Color.Reset) return Csi("0m");
			var ac = (int) x & 0xF;
			var prefix = "0;3";
			if((x & Color.Bold) != 0) {
				if((x & Color.Bright) != 0)
					prefix = "1;9";
				else
					prefix = "1;3";
			} else if((x & Color.Bright) != 0)
				prefix = "0;9";
			else if((x & Color.Underline) != 0)
				prefix = "4;3";

			return Csi($"{prefix}{ac}m");
		}

		public static string Background(Color x) {
			if(x == Color.Reset) return Csi("0m");
			var ac = (byte) x & 0xF;
			var prefix = "4";
			if((x & Color.Bright) != 0)
				prefix = "0;10";

			return Csi($"{prefix}{ac}m");
		}

		public Terminal() {
			if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				Platform = new MacPlatform();
			else
				throw new NotImplementedException();

			Platform.Resize += () => Resize?.Invoke();

			Console.Write("");
			OldTermios = Platform.Termios;

			var rt = Platform.Termios;
			Platform.MakeRaw(ref rt);
			Platform.Termios = rt;
		}

		~Terminal() => ReleaseUnmanagedResources();

		void ReleaseUnmanagedResources() => Platform.Termios = OldTermios;

		public void Dispose() {
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}

		public void Write(string s) => Platform.Write(s);
		public void Write(object x) => Write((x ?? "[null]").ToString());
		public void WriteLine(string s) => Platform.Write(s + "\n");
		public void WriteLine(object x) => WriteLine((x ?? "[null]").ToString());
		public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

		public void WriteCsi(string x) => Write(Csi(x));

		public bool CursorVisiblity {
			set => WriteCsi(value ? "?25h" : "?25l");
		}

		public (int X, int Y) CursorPosition {
			get {
				WriteCsi("6n");
				while(true) {
					if(ReadKey().KeyChar == '\x1b')
						break;
				}
				var data = "";
				while(true) {
					var c = ReadKey().KeyChar;
					if(c == 'R')
						break;
					data += c;
				}

				var s = data.Split(';');
				return (int.Parse(s[1]) - 1, int.Parse(s[0]) - 1);
			}
			set => WriteCsi($"{value.Y + 1};{value.X + 1}H");
		}

		public void MoveCursorLeft(uint count = 1) => WriteCsi($"{count}D");
		public void MoveCursorRight(uint count = 1) => WriteCsi($"{count}C");
		public void MoveCursorUp(uint count = 1) => WriteCsi($"{count}A");
		public void MoveCursorDown(uint count = 1) => WriteCsi($"{count}B");

		public void SaveCursor() => WriteCsi("s");
		public void RestoreCursor() => WriteCsi("u");

		public void Clear() => WriteCsi("2J");
	}
}