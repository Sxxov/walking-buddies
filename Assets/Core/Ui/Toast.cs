using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public delegate void OnToastVisibilityChange(bool isShown);

	public class Toast
	{
		public event OnToastVisibilityChange? OnVisibilityChange;
		private readonly CanvasBehaviour canvas;
		public float durationSeconds = float.PositiveInfinity;
		private static readonly ConditionalWeakTable<
			CanvasBehaviour,
			Coroutine?
		> canvasBehaviourToCoroutine = new();

		public Toast(CanvasBehaviour canvas)
		{
			this.canvas = canvas;

			if (canvas.toastBehaviour is null)
			{
				throw new InvalidOperationException(
					"Attempted to create toast before unity Start-ed"
				);
			}
		}

		public Toast SetHeading(string v)
		{
			canvas.toastBehaviour!.heading = v;

			return this;
		}

		public Toast SetParagraph(string v)
		{
			canvas.toastBehaviour!.paragraph = v;

			return this;
		}

		public Toast SetIcon(string v)
		{
			canvas.toastBehaviour!.icon = v;

			return this;
		}

		public Toast SetDurationSeconds(float v)
		{
			durationSeconds = v;

			return this;
		}

		public Toast SetButtonIcon(string v)
		{
			canvas.toastBehaviour!.buttonIcon = v;

			return this;
		}

		public Toast SetButtonText(string v)
		{
			canvas.toastBehaviour!.buttonText = v;

			return this;
		}

		public Toast Show()
		{
			AssertCanvasToastBehaviour();

			var hideAfterCoroutine = canvasBehaviourToCoroutine.GetValue(
				canvas,
				(_) => null
			);

			if (hideAfterCoroutine is not null)
			{
				canvas.StopCoroutine(hideAfterCoroutine);
				canvasBehaviourToCoroutine.Remove(canvas);
			}

			canvas.toastBehaviour!.isHidden = true;

			if (!float.IsInfinity(durationSeconds))
			{
				canvasBehaviourToCoroutine.Add(
					canvas,
					canvas.StartCoroutine(HideAfter(durationSeconds))
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

		public Toast Hide()
		{
			AssertCanvasToastBehaviour();

			var hideAfterCoroutine = canvasBehaviourToCoroutine.GetValue(
				canvas,
				(_) => null
			);

			if (hideAfterCoroutine is not null)
			{
				canvas.StopCoroutine(hideAfterCoroutine);
				canvasBehaviourToCoroutine.Remove(canvas);
			}

			canvas.toastBehaviour!.isHidden = false;

			OnVisibilityChange?.Invoke(false);

			return this;
		}

		private void AssertCanvasToastBehaviour()
		{
			if (canvas.toastBehaviour is null)
			{
				throw new InvalidOperationException(
					"Attempted to show toast before game has Start-ed"
				);
			}
		}
	}
}
