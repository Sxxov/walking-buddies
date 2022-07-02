using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public class FacingCameraBehaviour : MonoBehaviour
	{
		void Update()
		{
			var camera = Camera.main;

			transform.LookAt(
				transform.position
					+ camera.transform.rotation * Vector3.forward,
				camera.transform.rotation * Vector3.up
			);
		}
	}
}
