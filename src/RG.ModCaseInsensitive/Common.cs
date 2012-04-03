
using System;

namespace RG.ModCaseInsensitive {
	internal class Common {
		public static void Log(string fmt, params object[] args) {
			if (Config.Get().Debug) {
				Console.WriteLine(fmt, args);
			}
		}
	}
}

