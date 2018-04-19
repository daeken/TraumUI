using System;
using System.Collections.Generic;

namespace TraumUI {
	public interface IWidget {
		int? TabIndex { get; set; }
		IReadOnlyList<IWidget> Children { get; }
		(int, int) Size((int, int) maxSpace);
		IReadOnlyList<Rope> Render((int, int) maxSpace);

		void Focus();
		void Unfocus();
		void Click();
		bool Key(ConsoleKeyInfo key);
	}
}