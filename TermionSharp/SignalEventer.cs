using System;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace TraumUI {
	public class SignalEventer {
		public event Action<Signum> Triggered;
		readonly Thread EventerThread;

		public SignalEventer(Signum signal) {
			EventerThread = new Thread(() => {
				var sig = new UnixSignal(signal);
				while(true) {
					sig.WaitOne();
					Triggered?.Invoke(signal);
				}
			});
			EventerThread.Start();
		}

		//~SignalEventer() => EventerThread.Abort();

		public static void Hook(Signum signal, Action<Signum> cb) {
			var evt = new SignalEventer(signal);
			evt.Triggered += cb;
		}

		public static void Hook(Signum signal, Action cb) => Hook(signal, _ => cb());
	}
}