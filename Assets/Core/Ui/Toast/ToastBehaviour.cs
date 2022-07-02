using Google.MaterialDesign.Icons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WalkingBuddies.Core.Ui
{
	public class ToastBehaviour : MonoBehaviour
	{
		public bool isShown
		{
			get => gameObject.activeSelf;
			set { gameObject.SetActive(value); }
		}

		public string heading
		{
			get => headingBehaviour!.text;
			set { headingBehaviour!.text = value; }
		}

		public string paragraph
		{
			get => paragraphBehaviour!.text;
			set { paragraphBehaviour!.text = value; }
		}

		public string icon
		{
			get => iconBehaviour!.text;
			set { iconBehaviour!.text = value; }
		}

		public string buttonIcon
		{
			get => buttonIconBehaviour!.text;
			set { buttonIconBehaviour!.text = value; }
		}

		public string buttonText
		{
			get => buttonTextBehaviour!.text;
			set { buttonTextBehaviour!.text = value; }
		}

		public bool isCloseFabActive
		{
			get => closeFabObject!.activeSelf;
			set { closeFabObject!.SetActive(value); }
		}

		private TextMeshProUGUI? headingBehaviour;
		private TextMeshProUGUI? paragraphBehaviour;

		private MaterialIcon? iconBehaviour;
		private MaterialIcon? buttonIconBehaviour;
		private TextMeshProUGUI? buttonTextBehaviour;
		private GameObject? closeFabObject;

		void Awake()
		{
			isShown = false;

			var headingObject = transform
				.Find("@Background/@Heading")
				.gameObject;
			var paragraphObject = transform
				.Find("@Background/@Paragraph")
				.gameObject;
			var iconObject = transform.Find("@TopFab/@Icon").gameObject;
			var buttonIconObject = transform
				.Find("@Background/@Button/@Layout/@Icon")
				.gameObject;
			var buttonTextObject = transform
				.Find("@Background/@Button/@Layout/@Text")
				.gameObject;
			closeFabObject = transform
				.Find("@Background/@CloseFabLayout/@CloseFab")
				.gameObject;

			headingBehaviour = headingObject!.GetComponent<TextMeshProUGUI>();
			paragraphBehaviour =
				paragraphObject!.GetComponent<TextMeshProUGUI>();
			iconBehaviour = iconObject!.GetComponent<MaterialIcon>();
			buttonIconBehaviour =
				buttonIconObject!.GetComponent<MaterialIcon>();
			buttonTextBehaviour =
				buttonTextObject!.GetComponent<TextMeshProUGUI>();
		}

		public void Hide()
		{
			isShown = false;
		}

		public void Show()
		{
			isShown = true;

			// call it twice due to the use of content size fitter
			// https://forum.unity.com/threads/content-size-fitter-refresh-problem.498536/#post-4455325
			LayoutRebuilder.ForceRebuildLayoutImmediate(
				(RectTransform)transform
			);
			LayoutRebuilder.ForceRebuildLayoutImmediate(
				(RectTransform)transform
			);
		}
	}
}
