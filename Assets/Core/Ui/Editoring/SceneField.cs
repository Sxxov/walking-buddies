using System;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WalkingBuddies.Core.Ui
{
	[Serializable]
	public class SceneField
	{
		[SerializeField]
		private UnityEngine.Object? sceneAsset;

		[FormerlySerializedAs("sceneName")]
		[SerializeField]
		private string sceneNameBacking = "";
		public string sceneName
		{
			get { return sceneNameBacking; }
		}

		// makes it work with the existing Unity methods (LoadLevel/LoadScene)
		public static implicit operator string(SceneField sceneField)
		{
			return sceneField.sceneName;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(SceneField))]
	public class SceneFieldPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(
			Rect position,
			SerializedProperty property,
			GUIContent label
		)
		{
			EditorGUI.BeginProperty(position, GUIContent.none, property);

			var sceneAsset = property.FindPropertyRelative("sceneAsset");
			var sceneName = property.FindPropertyRelative("sceneNameBacking");

			var labelPosition = EditorGUI.PrefixLabel(
				position,
				GUIUtility.GetControlID(FocusType.Passive),
				label
			);

			if (sceneAsset != null)
			{
				EditorGUI.BeginChangeCheck();

				var value = EditorGUI.ObjectField(
					labelPosition,
					sceneAsset.objectReferenceValue,
					typeof(SceneAsset),
					false
				);

				if (EditorGUI.EndChangeCheck())
				{
					sceneAsset.objectReferenceValue = value;

					if (sceneAsset.objectReferenceValue != null)
					{
						sceneName.stringValue = (
							(SceneAsset)sceneAsset.objectReferenceValue
						).name;
					}
				}
			}

			EditorGUI.EndProperty();
		}
	}
#endif
}
