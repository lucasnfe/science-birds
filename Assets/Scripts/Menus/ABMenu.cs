using UnityEngine;
using System.Collections;

public class ABMenu : MonoBehaviour {

	public void LoadNextScene(string sceneName) {

		ABSceneManager.Instance.LoadScene(sceneName);
	}

	public void LoadNextScene(string sceneName, bool loadTransition, ABSceneManager.ActionBetweenScenes action) {

		ABSceneManager.Instance.LoadScene(sceneName, loadTransition, action);
	}
}
