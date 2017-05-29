using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;

namespace Concurrency
{
	interface IPriority
	{

		int Value { get; }

	}

	public class PriorotyMailbox : UnboundedPriorityMailbox
	{

		public PriorotyMailbox(Settings settings, Config config)
			: base(settings, config) { }

		protected override int PriorityGenerator(object message) {
			var ip = message as IPriority;
			if (ip != null) {
				return ip.Value;
			}
			return int.MaxValue;
		}

	}
}
