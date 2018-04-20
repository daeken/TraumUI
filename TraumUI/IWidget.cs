using System;
using System.Collections.Generic;

namespace TraumUI {
	public interface IWidget {
		IWidget Parent { get; set; }
		int? TabIndex { get; set; }
		IReadOnlyList<IWidget> Children { get; }
		(int, int) Size((int, int) maxSpace);
		IReadOnlyList<Rope> Render((int, int) maxSpace);

		void Focus();
		void Unfocus();
		void Click();
		bool Key(ConsoleKeyInfo key);
	}

	public abstract class BaseWidget : IWidget {
		public IWidget Parent { get; set; }
		public virtual int? TabIndex { get; set; }
		public virtual IReadOnlyList<IWidget> Children => new IWidget[0];
		
		public virtual (int, int) Size((int, int) maxSpace) => maxSpace;
		public abstract IReadOnlyList<Rope> Render((int, int) maxSpace);

		public virtual void Focus() {}
		public virtual void Unfocus() {}
		public virtual void Click() {}
		public virtual bool Key(ConsoleKeyInfo key) => false;
	}

	public abstract class FocusableWidget : BaseWidget {
		public bool Focused { get; protected set; }

		public override void Focus() => this.RedrawWith(() => Focused = true);
		public override void Unfocus() => this.RedrawWith(() => Focused = false);
	}
}