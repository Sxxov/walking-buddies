using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WalkingBuddies.Core.Behaviour;

namespace WalkingBuddies.Core.Threading
{
	delegate void OnMainThreadQueueDone();

	public class MainThreadDispatcher : AutoMonoBehaviour
	{
		private static volatile bool queued = false;
		private static List<Action> queue = new();

		private static event OnMainThreadQueueDone? OnQueueDone;

		void Update()
		{
			if (queued)
			{
				List<Action> stack;
				lock (queue)
				{
					stack = queue;
					queue = new();
					queued = false;
				}

				foreach (var action in stack)
				{
					action();
				}

				OnQueueDone?.Invoke();
			}
		}

		public static void ScheduleRunOnMainThread(Action action)
		{
			lock (queue)
			{
				queue.Add(action);
				queued = true;
			}
		}

		public static async Task RunOnMainThreadAsync(Action action)
		{
			var source = new TaskCompletionSource<object?>();

			lock (queue)
			{
				queue.Add(action);
				queued = true;
			}

			OnQueueDone += onQueueDone;

			void onQueueDone()
			{
				OnQueueDone -= onQueueDone;
				source!.SetResult(null);
			}

			await source.Task;
		}
	}
}
