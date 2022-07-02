using System.Reflection;
using UnityEngine;

namespace WalkingBuddies.Core.Behaviour
{
	public class AutoMonoBehaviour : MonoBehaviour
	{
		public static Component? instance { get; private set; }

		[RuntimeInitializeOnLoadMethod(
			RuntimeInitializeLoadType.BeforeSceneLoad
		)]
		private static void Initialise()
		{
			if (instance == null)
			{
				instance = new GameObject().AddComponent(
					MethodBase.GetCurrentMethod().DeclaringType
				);
				DontDestroyOnLoad(instance.gameObject);
			}
		}
	}
}
