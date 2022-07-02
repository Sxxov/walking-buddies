using System.Net.Mime;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEditor;
using UnityEditor.UI;

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
