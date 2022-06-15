using System;
using System.Linq;
using UnityEngine;

namespace WalkingBuddies.Core.Utilty
{
	public class AverageUtility
	{
		public static Vector3 AveragePosition(params Vector3[] positions) =>
			positions.Aggregate(Vector3.zero, (prev, curr) => prev + curr)
			/ positions.Length;

		public static Quaternion AverageQuaternion(
			params Quaternion[] rotations
		)
		{
			if (rotations.Length <= 0)
			{
				throw new ArgumentException(
					"Empty rotations array",
					nameof(rotations)
				);
			}

			if (rotations.Length == 1)
			{
				return rotations[0];
			}

			var cum = new Vector4();
			var i = 0;
			Quaternion? result = null;

			foreach (var rotation in rotations)
			{
				result = AverageQuaternion(
					ref cum,
					rotation,
					rotations[0],
					i++
				);
			}

			return (Quaternion)result!;
		}

		// stolen wholesale from https://forum.unity.com/threads/average-quaternions.86898/#post-1053611

		// Get an average (mean) from more than two quaternions (with two, slerp would be used).
		// Note: this only works if all the quaternions are relatively close together.
		// Usage:
		// - Cumulative is an external Vector4 which holds all the added x y z and w components.
		// - newRotation is the next rotation to be added to the average pool
		// - firstRotation is the first quaternion of the array to be averaged
		// - i holds the total number of quaternions which are currently added
		// This function returns the current average quaternion
		private static Quaternion AverageQuaternion(
			ref Vector4 cumulative,
			Quaternion newRotation,
			Quaternion firstRotation,
			int i
		)
		{
			// Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
			// q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
			if (!AreQuaternionsClose(newRotation, firstRotation))
			{
				newRotation = InverseSignQuaternion(newRotation);
			}

			// Average the values
			var addDet = 1f / i;
			cumulative.w += newRotation.w;
			var w = cumulative.w * addDet;
			cumulative.x += newRotation.x;
			var x = cumulative.x * addDet;
			cumulative.y += newRotation.y;
			var y = cumulative.y * addDet;
			cumulative.z += newRotation.z;
			var z = cumulative.z * addDet;

			// note: if speed is an issue, you can skip the normalization step
			return NormalizeQuaternion(x, y, z, w);
		}

		private static Quaternion NormalizeQuaternion(
			float x,
			float y,
			float z,
			float w
		)
		{
			var lengthD = 1f / (w * w + x * x + y * y + z * z);

			w *= lengthD;
			x *= lengthD;
			y *= lengthD;
			z *= lengthD;

			return new Quaternion(x, y, z, w);
		}

		// Changes the sign of the quaternion components. This is not the same as the inverse.
		private static Quaternion InverseSignQuaternion(Quaternion q) =>
			new(-q.x, -q.y, -q.z, -q.w);

		// Returns true if the two input quaternions are close to each other. This can
		// be used to check whether or not one of two quaternions which are supposed to
		// be very similar but has its component signs reversed (q has the same rotation as
		// -q)
		private static bool AreQuaternionsClose(Quaternion q1, Quaternion q2) =>
			Quaternion.Dot(q1, q2) >= 0f;
	}
}
