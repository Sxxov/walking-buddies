using System.Collections;
using System;
using UnityEngine;
using Vuforia;
using WalkingBuddies.Core.Card;

namespace WalkingBuddies.Core.Ar
{
	public class TargetBehaviour : MonoBehaviour
	{
		[SerializeField]
		private TargetController? controller;

		public CardKinds kind;

		private GameObject? outlineObject;

		private bool isFound;

		void Awake()
		{
			if (controller is null)
			{
				throw new InvalidOperationException(
					"Attempted to construct TargetBehaviour without target controller"
				);
			}

			GetComponent<ObserverBehaviour>().OnTargetStatusChanged +=
				OnTargetStatusChanged;

			kind = CardTokeniser.ConvertNameToKind(
				GetComponent<ImageTargetBehaviour>().TargetName
			);
		}

		private void OnTargetStatusChanged(
			ObserverBehaviour observer,
			TargetStatus status
		)
		{
			if (isActiveAndEnabled)
			{
				if (
					status.Status == Status.TRACKED
					|| status.Status == Status.EXTENDED_TRACKED
				)
				{
					StartCoroutine(OnTargetFound());
				}
				else
				{
					StartCoroutine(OnTargetLost());
				}
			}
		}

		private IEnumerator OnTargetFound()
		{
			if (isFound)
			{
				yield break;
			}
			isFound = true;

			yield return null;

			outlineObject = (GameObject)Instantiate(
				Resources.Load(
					"@Prefabs/@Ui/@TargetOutline",
					typeof(GameObject)
				)
			);
			outlineObject.transform.SetParent(transform, false);

			controller!.Add(this);
		}

		private IEnumerator OnTargetLost()
		{
			if (!isFound)
			{
				yield break;
			}
			isFound = false;

			yield return null;

			if (outlineObject is not null)
			{
				Destroy(outlineObject);
			}

			controller!.Remove(this);
		}
	}
}
