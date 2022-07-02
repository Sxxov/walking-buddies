using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WalkingBuddies.Core.Ui
{
	public class LoadSceneHook : MonoBehaviour
	{
		public SceneField? scene;

		void Awake()
		{
			if (scene is null)
			{
				throw new InvalidOperationException(
					"Attempted to construct LoadSceneHook without scene"
				);
			}
		}

		public void LoadScene()
		{
			if (scene is null)
			{
				throw new InvalidOperationException(
					"Attempted to load scene before component has Awake-ned"
				);
			}

			SceneManager.LoadScene(scene);
		}
	}
}
