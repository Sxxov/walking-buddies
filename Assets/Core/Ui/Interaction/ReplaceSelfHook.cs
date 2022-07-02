using System;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public class ReplaceSelfHook : MonoBehaviour
	{
		public GameObject? prefab;

		void Awake()
		{
			if (prefab is null)
			{
				throw new InvalidOperationException(
					"Attempted to construct ReplaceSelfHook without prefab"
				);
			}
		}

		public void ReplaceSelf()
		{
			if (prefab is null)
			{
				throw new InvalidOperationException(
					"Attempted to replace self before component has Awake-ned"
				);
			}

			var prefabObject = Instantiate(prefab);
			prefabObject.transform.SetParent(transform.parent, false);

			Destroy(gameObject);
		}
	}
}
