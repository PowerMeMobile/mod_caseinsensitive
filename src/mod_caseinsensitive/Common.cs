
using System;

namespace mod_caseinsensitive
{
	internal class Common {
		public static void Log(string fmt, params object[] args) {
			if (Config.Get().Debug) {
				Console.WriteLine("mod_case, " + fmt, args);
			}
		}
	}
}

