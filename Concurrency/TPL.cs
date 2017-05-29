using System.Collections.Generic;
using System.Threading.Tasks;

namespace Concurrency
{
	class TPL
	{
		private static readonly Dictionary<string, object> Lockers = new Dictionary<string, object>();
		private static readonly object _locker = new object();
		public static void RunTPL(int number) {
			Task.Run(() => {
				string key = Program.GetKey(number, "tpl");
				lock (_locker) {
					if (!Lockers.ContainsKey(key)) {
						Lockers[key] = new object();
					}
				}
				lock (Lockers[key]) {
					while (Program.LockExists(key)) {
						//wait
					}
					Program.SetLock(key);
				}
				Program.DoWork();
				Program.ReleaseLock(key);
			});
		}

	}
}