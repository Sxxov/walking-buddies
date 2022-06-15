using System;
using System.Collections;
using System.Collections.Generic;
using WalkingBuddies.Core.Card;

namespace WalkingBuddies.Core.Level
{
	public struct BuddiesStore<T> : IEnumerable
	{
		public T player;
		public T turtle;
		public T bird;

		public T Get(BuddyCardKinds buddy) =>
			buddy switch
			{
				BuddyCardKinds.TURTLE => turtle,
				BuddyCardKinds.BIRD => bird,
				_
					=> throw new ArgumentException(
						"Unknown buddy",
						nameof(buddy)
					),
			};

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<T> GetEnumerator()
		{
			yield return turtle;
			yield return player;
			yield return bird;
		}

		public override string ToString() =>
			$"turtle = {turtle}, player = {player}, bird = {bird}";
	}
}
