using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WalkingBuddies.Core.Tweening
{
	public class Tween : IEnumerable<Vector3>
	{
		private Vector3 p1;
		private Vector3 p2;

		private readonly float durationSeconds;

		private readonly Bezier? bezier;

		public Tween(
			Vector3 p1,
			Vector3 p2,
			float durationSeconds,
			Bezier? bezier = null
		) : base()
		{
			this.p1 = p1;
			this.p2 = p2;
			this.durationSeconds = durationSeconds;
			this.bezier = bezier;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<Vector3> GetEnumerator() => Run();

		public IEnumerator<Vector3> Run() =>
			Run(p1, p2, durationSeconds, bezier);

		public static IEnumerator<Vector3> Run(
			Vector3 p1,
			Vector3 p2,
			float durationSeconds,
			Bezier? bezier = null
		)
		{
			var startSeconds = Time.time;

			while (Time.time - startSeconds <= durationSeconds)
			{
				var weight = (float)(
					bezier is null
						? (Time.time - startSeconds) / durationSeconds
						: bezier.At(
							(Time.time - startSeconds) / durationSeconds
						)
				);

				yield return Vector3.Lerp(p1, p2, weight);
			}
		}
	}
}
