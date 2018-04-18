using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TermionSharp {
	[StructLayout(LayoutKind.Sequential)]
	public struct Termios {
		public uint c_iflag, c_oflag, c_cflag, c_lflag;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
		public byte[] c_cc; // NCCS=20
		public int __ispeed, __ospeed;
	}
	
	public interface IPlatformBits {
		(ushort Rows, ushort Cols, ushort X, ushort Y) Size { get; }
		FileStream TTY { get; }
		Termios Termios { get; set; }
		void MakeRaw(ref Termios term);

		event Action Resize;

		bool IsTTY(FileStream file);
		void Write(string s);
	}
}