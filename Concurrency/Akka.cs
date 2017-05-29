using System.Collections.Generic;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;

namespace Concurrency
{
	public class Akka
	{

		private static readonly ActorSystem _actorSystem;
		private static readonly IActorRef _actor;
		private static readonly IActorRef _lockActor;
		static Akka() {
			_actorSystem = ActorSystem.Create("helloWorld");
			_lockActor = _actorSystem.ActorOf<LockActor>();
			_actor = _actorSystem.ActorOf(Props.Create(()=>new WorkerActor(_lockActor)).WithRouter(FromConfig.Instance), "worker");
		}

		public static void Init() {
			
		}

		public static void Run(int number) {
			_actor.Tell(new WorkerActor.RunInfo{MsgId = number});
		}
	}

	public class LockActor : ReceiveActor
	{

		private readonly Dictionary<string, IActorRef> _lockOwners = new Dictionary<string, IActorRef>();
		public LockActor() {
			Receive<TryGetLock>(m => {
				var response = new TryGetLockResponse {Key = m.Key};
				if (Program.LockExists(m.Key)) {
					response.Locked = true;
					_lockOwners[m.Key].Tell(new NotifyOnComplete{ActorRef = Sender});
				} else {
					_lockOwners[m.Key] = Sender;
					Program.SetLock(m.Key);
				}
				Sender.Tell(response);
			});
			Receive<ReleaseLock>(m => {
				_lockOwners[m.Key] = null;
				Program.ReleaseLock(m.Key);
			});
		}

		#region Messages

		public class TryGetLock
		{

			public string Key { get; set; }

		}
		public class TryGetLockResponse
		{

			public bool Locked { get; set; }
			public string Key { get; set; }
		}
		public class NotifyOnComplete : IPriority
		{

			public IActorRef ActorRef { get; set; }

			public int Value { get; set; }

		}
		public class ReleaseLock
		{

			public string Key { get; set; }

		}

		#endregion
		
	}

	public class WorkerActor:ReceiveActor
	{

		#region Messages

		public class RunInfo
		{

			public int MsgId { get; set; }

		}
		public class LockFree
		{

		}


		#endregion

		private readonly IActorRef _lockActor;

		public WorkerActor(IActorRef lockActor) {
			_lockActor = lockActor;
			Become(Ready);
		}

		private void Working() {
			Receive<LockActor.TryGetLockResponse>(r => {
				if (!r.Locked) {
					Program.DoWork();
					_lockActor.Tell(new LockActor.ReleaseLock {
						Key = r.Key
					});
					Become(Ready);
				}
			});
			Receive<LockFree>(m => {
				Become(Ready);
				Self.Tell(_runInfo);
			});
		}

		private RunInfo _runInfo;
		private void Ready() {
			Receive<RunInfo>(msg => {
				_runInfo = msg;
				string key =Program.GetKey(msg.MsgId, "akka");
				BecomeStacked(Working);
				_lockActor.Tell(new LockActor.TryGetLock {Key = key });
			});
			Receive<LockActor.NotifyOnComplete>(m => m.ActorRef.Tell(new LockFree()));
		}

	}
}
