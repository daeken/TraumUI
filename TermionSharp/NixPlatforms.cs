using System;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Unix.Native;
using TraumUI;

namespace TermionSharp {
	internal abstract class NixPlatform : IPlatformBits {
		public FileStream TTY => File.Open("/dev/tty", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		public readonly StreamWriter Stdout;
		public abstract Termios Termios { get; set; }

		public abstract void MakeRaw(ref Termios term);

		public event Action Resize;

		public abstract (ushort Rows, ushort Cols, ushort X, ushort Y) Size { get; }
		public abstract bool IsTTY(FileStream file);
		public void Write(string s) {
			Stdout.Write(s);
			Stdout.Flush();
		}

		internal NixPlatform() {
			SignalEventer.Hook(Signum.SIGWINCH, () => Resize?.Invoke());
			Stdout = new StreamWriter(File.Open("/dev/stdout", FileMode.Open, FileAccess.Write, FileShare.ReadWrite));
		}
	}

	internal class MacPlatform : NixPlatform {
		[DllImport("libc.dylib")]
		static extern bool isatty(IntPtr fd);

		[StructLayout(LayoutKind.Sequential)]
		struct TermSize {
			internal ushort Rows, Cols, X, Y;
		}
		[DllImport("libc.dylib")]
		static extern int ioctl(int fileno, uint request, out TermSize size);
		
		[DllImport("libc.dylib")]
		static extern int tcgetattr(int fd, ref Termios termios);
		[DllImport("libc.dylib")]
		static extern int tcsetattr(int fd, int opt, ref Termios termios);
		
		public override (ushort Rows, ushort Cols, ushort X, ushort Y) Size {
			get {
				ioctl(1, 0x40087468, out var size);
				return (size.Rows, size.Cols, size.X, size.Y);
			}
		}

		public override Termios Termios {
			get {
				var term = new Termios();
				tcgetattr(0, ref term);
				return term;
			}
			set => tcsetattr(0, 0, ref value);
		}

		[DllImport("libc.dylib")]
		static extern void cfmakeraw(ref Termios termios);

		public override void MakeRaw(ref Termios term) =>
			cfmakeraw(ref term);

		public override bool IsTTY(FileStream file) => isatty(file.SafeFileHandle.DangerousGetHandle());
	}
}