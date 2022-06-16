using System.Collections;
using System;
using UnityEngine;
using Unity.VisualScripting;

namespace WalkingBuddies.Core.Ui
{
	public class CanvasBehaviour : MonoBehaviour
	{
		[HideInInspector]
		public ToastBehaviour? toastBehaviour;

		void Start()
		{
			// create toast object
			var toastObject = (GameObject)Instantiate(
				Resources.Load("Prefabs/Toast", typeof(GameObject))
			);

			toastObject.transform.SetParent(gameObject.transform, false);

			toastBehaviour = toastObject.GetComponent<ToastBehaviour>();
		}
	}
}
