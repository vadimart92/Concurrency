using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrency
{
	class Program
	{

		private static readonly ConcurrentDictionary<string, bool> _locks = new ConcurrentDictionary<string, bool>();

		private static ConcurrentBag<long> _results = new ConcurrentBag<long>();
		private static int _iterationsIvoked = 0;

		static void Main(string[] args) {
			ThreadPool.SetMaxThreads(3, 1);
			MeasureAction(TPL.RunTPL, 100, "RunTPL");
			MeasureAction(Akka.Run, 100, "RunAkka");
			Console.ReadKey(true);
		}

		public static bool LockExists(string key) {
			return _locks.ContainsKey(key) && _locks[key];
		}
		public static void SetLock(string key) {
			_locks[key] = true;
		}
		public static void ReleaseLock(string key) {
			_locks[key] = false;
		}
		private static void SetIterationFinished() {
			Interlocked.Increment(ref _iterationsIvoked);
		}

		public static void DoWork() {
			var result = Utils.FindPrimeNumber(20000);
			Thread.Sleep(10);
			_results.Add(result);
			SetIterationFinished();
		}

		private static void MeasureAction(Action<int> action, int iterations, string name) {
			_iterationsIvoked = 0;
			_results = new ConcurrentBag<long>();
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < iterations; i++) {
				action(i);
			}
			while (_iterationsIvoked != iterations) {
				//wait
			}
			sw.Stop();
			Console.WriteLine("Action: {0}. Total time: {1}.", name, sw.Elapsed);
		}

		public static string GetKey(int number, string name) {
			return name + number % 2;
		}

	}
}
