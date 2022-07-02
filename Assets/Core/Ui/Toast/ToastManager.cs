using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public delegate void OnToastVisibilityChange(bool isShown);

	public class ToastManager
	{
		public event OnToastVisibilityChange? OnVisibilityChange;

		private readonly ToastCanvasBehaviour canvas;

		public float durationSeconds = float.PositiveInfinity;

		private static readonly ConditionalWeakTable<
			ToastCanvasBehaviour,
			Coroutine?
		> canvasBehaviourToHideAfterCoroutine = new();

		// unity can't use null propogation
		// this code is equivalent to canvas.toastBehaviour?.isShown ?? false;
		public bool isShown =>
			canvas.toastBehaviour != null && canvas.toastBehaviour.isShown;

		public ToastManager(ToastCanvasBehaviour toastCanvasBehaviour)
		{
			if (toastCanvasBehaviour.toastBehaviour is null)
			{
				throw new InvalidOperationException(
					"Attempted to create toast before unity Start-ed"
				);
			}

			canvas = toastCanvasBehaviour;
		}

		public ToastManager SetHeading(string v)
		{
			canvas.toastBehaviour!.heading = v;

			return this;
		}

		public ToastManager SetParagraph(string v)
		{
			canvas.toastBehaviour!.paragraph = v;

			return this;
		}

		public ToastManager SetIcon(string v)
		{
			canvas.toastBehaviour!.icon = v;

			return this;
		}

		public ToastManager SetDurationSeconds(float v)
		{
			durationSeconds = v;

			return this;
		}

		public ToastManager SetButtonIcon(string v)
		{
			canvas.toastBehaviour!.buttonIcon = v;

			return this;
		}

		public ToastManager SetButtonText(string v)
		{
			canvas.toastBehaviour!.buttonText = v;

			return this;
		}

		public ToastManager SetIsCloseFabActive(bool v)
		{
			canvas.toastBehaviour!.isCloseFabActive = v;

			return this;
		}

		public ToastManager Show()
		{
			var hideAfterCoroutine =
				canvasBehaviourToHideAfterCoroutine.GetValue(
					canvas,
					(_) => null
				);

			if (hideAfterCoroutine is not null)
			{
				canvas.StopCoroutine(hideAfterCoroutine);
				canvasBehaviourToHideAfterCoroutine.Remove(canvas);
			}

			canvas.toastBehaviour!.Show();

			if (!float.IsInfinity(durationSeconds))
			{
				canvasBehaviourToHideAfterCoroutine.Add(
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

		public ToastManager Hide()
		{
			var hideAfterCoroutine =
				canvasBehaviourToHideAfterCoroutine.GetValue(
					canvas,
					(_) => null
				);

			if (hideAfterCoroutine is not null)
			{
				canvas.StopCoroutine(hideAfterCoroutine);
				canvasBehaviourToHideAfterCoroutine.Remove(canvas);
			}

			canvas.toastBehaviour!.isShown = false;

			OnVisibilityChange?.Invoke(false);

			return this;
		}

		public async Task ResolveOnHide()
		{
			var source = new TaskCompletionSource<object?>();

			void onVisibilityChange(bool isVisible)
			{
				if (!isVisible)
				{
					OnVisibilityChange -= onVisibilityChange;

					source.SetResult(null);
				}
			}

			OnVisibilityChange += onVisibilityChange;

			await source.Task;
		}

		public async Task ResolveOnShown()
		{
			var source = new TaskCompletionSource<object?>();

			void onVisibilityChange(bool isVisible)
			{
				if (isVisible)
				{
					OnVisibilityChange -= onVisibilityChange;

					source.SetResult(null);
				}
			}

			OnVisibilityChange += onVisibilityChange;

			await source.Task;
		}
	}
}
