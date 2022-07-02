using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace WalkingBuddies.Core.Ui
{
	public class UrlButton : Button
	{
		public string url = "";

		public UrlButton() : base()
		{
			onClick.AddListener(() =>
			{
				Goto(url);
			});
		}

		public static void Goto(string url)
		{
			Application.OpenURL(url);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(UrlButton))]
	public class UrlButtonEditor : ButtonEditor
	{
		public override void OnInspectorGUI()
		{
			((UrlButton)target).url = EditorGUILayout.TextField(
				"Url",
				((UrlButton)target).url
			);
			EditorGUILayout.Separator();

			base.OnInspectorGUI();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
			}

			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
		}
	}
#endif
}
