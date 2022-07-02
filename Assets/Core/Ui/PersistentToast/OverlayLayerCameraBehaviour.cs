using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public class OverlayLayerCameraBehaviour : MonoBehaviour
	{
		private Camera? parentCamera;

		private Camera? currCamera;

		void Awake()
		{
			var parent = transform.parent.gameObject;

			if (parent is null)
			{
				throw new InvalidOperationException(
					"Attempted to start OverlayCameraBehaviour on game object with no parent"
				);
			}

			parentCamera = parent.GetComponent<Camera>();

			if (parentCamera is null)
			{
				throw new InvalidOperationException(
					"Attempted to start OverlayCameraBehaviour on game object with parent that is not a camera"
				);
			}

			currCamera = GetComponent<Camera>();

			if (parentCamera is null)
			{
				throw new InvalidOperationException(
					"Attempted to start OverlayCameraBehaviour on game object that is not a camera"
				);
			}
		}

		void Update()
		{
			currCamera!.fieldOfView = parentCamera!.fieldOfView;
		}
	}
}
