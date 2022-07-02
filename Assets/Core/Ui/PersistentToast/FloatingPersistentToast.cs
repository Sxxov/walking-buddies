using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public delegate void OnPersistentToastVisibilityChange(bool isShown);

	public class FloatingPersistentToast
	{
		public event OnPersistentToastVisibilityChange? OnVisibilityChange;

		public PersistentToastBehaviour behaviour;

		public GameObject canvasObject;

		public GameObject toastObject;

		private float durationSeconds = float.PositiveInfinity;

		private Coroutine? hideAfterCoroutine;

		public FloatingPersistentToast()
		{
			canvasObject = (GameObject)
				UnityEngine.Object.Instantiate(
					Resources.Load(
						"@Prefabs/@Ui/@FacingCameraFloatingCanvas",
						typeof(GameObject)
					)
				);
			toastObject = (GameObject)
				UnityEngine.Object.Instantiate(
					Resources.Load(
						"@Prefabs/@Ui/@PersistentToast",
						typeof(GameObject)
					)
				);

			toastObject.transform.SetParent(canvasObject.transform, false);

			behaviour = toastObject.GetComponent<PersistentToastBehaviour>();
		}

		public void Destroy()
		{
			UnityEngine.Object.Destroy(canvasObject);
			UnityEngine.Object.Destroy(toastObject);
		}

		public FloatingPersistentToast SetHeading(string v)
		{
			behaviour.heading = v;

			return this;
		}

		public FloatingPersistentToast SetParagraph(string v)
		{
			behaviour.paragraph = v;

			return this;
		}

		public FloatingPersistentToast SetIcon(string v)
		{
			behaviour.icon = v;

			return this;
		}

		public FloatingPersistentToast SetDurationSeconds(float v)
		{
			durationSeconds = v;

			return this;
		}

		public FloatingPersistentToast Show()
		{
			if (hideAfterCoroutine is not null)
			{
				behaviour.StopCoroutine(hideAfterCoroutine);
				hideAfterCoroutine = null;
			}

			behaviour.Show();

			if (!float.IsInfinity(durationSeconds))
			{
				hideAfterCoroutine = behaviour.StartCoroutine(
					HideAfter(durationSeconds)
				);
			}

			OnVisibilityChange?.Invoke(true);

			return this;
		}

		private IEnumerator HideAfter(float seconds)
		{
			yield return new WaitForSeconds(seconds);

			Hide();
		}

		public FloatingPersistentToast Hide()
		{
			if (hideAfterCoroutine is not null)
			{
				behaviour.StopCoroutine(hideAfterCoroutine);
				hideAfterCoroutine = null;
			}

			behaviour.Hide();

			OnVisibilityChange?.Invoke(false);

			return this;
		}
	}
}
