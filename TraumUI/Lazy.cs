using System;

namespace TraumUI {
	public class Lazy {
		public static Lazy<T> Make<T>(Func<T> func) => new Lazy<T>(func);
		public static Lazy<T> lazy<T>(Func<T> func) => new Lazy<T>(func);
	}
	
	public class Lazy<T> {
		bool HasValue;
		T IValue;
		readonly Func<T> Func;

		public T Value {
			get {
				if(HasValue) return IValue;
				IValue = Func();
				HasValue = true;
				return IValue;
			}
		}
		
		public Lazy(Func<T> func) => Func = func;
		public static implicit operator T(Lazy<T> obj) => obj.Value;
		public static implicit operator Lazy<T>(Func<T> func) => new Lazy<T>(func);
	}
}