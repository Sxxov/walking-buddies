using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public class OverlayLayerBehaviour : MonoBehaviour
	{
		void Start()
		{
			SetLayerRecursively(gameObject, LayerMask.NameToLayer("@Overlay"));
		}

		private static void SetLayerRecursively(
			GameObject gameObject,
			int layer
		)
		{
			gameObject.layer = layer;

			foreach (Transform child in gameObject.transform)
			{
				SetLayerRecursively(child.gameObject, layer);
			}
		}
	}
}
