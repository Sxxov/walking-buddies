using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using Vuforia;
using WalkingBuddies.Core.Card;

namespace WalkingBuddies.Core.Ar
{
	public class TargetThing : MonoBehaviour
	{
		[SerializeField]
		private TargetController controller = null!;

		public CardKinds kind;

		void Start()
		{
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
				if (status.Status == Status.TRACKED)
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
			yield return null;
			controller.Add(this);
		}

		private IEnumerator OnTargetLost()
		{
			yield return null;
			controller.Remove(this);
		}
	}
}
