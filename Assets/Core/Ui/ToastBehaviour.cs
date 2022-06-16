using Google.MaterialDesign.Icons;
using TMPro;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public class ToastBehaviour : MonoBehaviour
	{
		public bool isHidden
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

		private TextMeshProUGUI? headingBehaviour;
		private TextMeshProUGUI? paragraphBehaviour;

		private MaterialIcon? iconBehaviour;
		private MaterialIcon? buttonIconBehaviour;
		private TextMeshProUGUI? buttonTextBehaviour;
		private GameObject? iconObject;

		private GameObject? buttonIconObject;

		public void Start()
		{
			isHidden = false;

			var headingObject = gameObject.transform
				.Find("@Background/@Heading")
				.gameObject;
			var paragraphObject = gameObject.transform
				.Find("@Background/@Paragraph")
				.gameObject;
			iconObject = gameObject.transform.Find("@TopFab/@Icon").gameObject;
			buttonIconObject = gameObject.transform
				.Find("@Background/@Button/@Layout/@Icon")
				.gameObject;
			var buttonTextObject = gameObject.transform
				.Find("@Background/@Button/@Layout/@Text")
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
			isHidden = false;
		}

		public void Show()
		{
			isHidden = true;
		}
	}
}
