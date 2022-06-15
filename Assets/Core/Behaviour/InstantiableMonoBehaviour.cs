using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

namespace WalkingBuddies.Core.Behaviour
{
	public class InstantiableMonoBehaviour : MonoBehaviour
	{
		public InstantiableMonoBehaviour()
		{
			new GameObject().AddComponent(
				MethodBase.GetCurrentMethod().DeclaringType
			);
			DontDestroyOnLoad(gameObject);
		}
	}
}
