using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public class ToastCanvasBehaviour : MonoBehaviour
	{
		[HideInInspector]
		public ToastBehaviour? toastBehaviour;

		void Awake()
		{
			// create toast object
			var toastObject = (GameObject)Instantiate(
				Resources.Load("@Prefabs/@Ui/@Toast", typeof(GameObject))
			);

			toastObject.transform.SetParent(gameObject.transform, false);

			toastBehaviour = toastObject.GetComponent<ToastBehaviour>();
		}
	}
}
