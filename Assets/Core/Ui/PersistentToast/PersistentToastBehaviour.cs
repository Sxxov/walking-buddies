using Google.MaterialDesign.Icons;
using TMPro;
using UnityEngine;

namespace WalkingBuddies.Core.Ui
{
	public class PersistentToastBehaviour : MonoBehaviour
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

		private TextMeshProUGUI? headingBehaviour;
		private TextMeshProUGUI? paragraphBehaviour;

		private MaterialIcon? iconBehaviour;

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

			headingBehaviour = headingObject!.GetComponent<TextMeshProUGUI>();
			paragraphBehaviour =
				paragraphObject!.GetComponent<TextMeshProUGUI>();
			iconBehaviour = iconObject!.GetComponent<MaterialIcon>();
		}

		public void Hide()
		{
			isShown = false;
		}

		public void Show()
		{
			isShown = true;
		}
	}
}
