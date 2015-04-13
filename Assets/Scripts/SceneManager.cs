using UnityEngine;
using System.Collections;

public class SceneManager : ABSingleton<SceneManager> {

	public void LoadScene(int sceneIndex, System.Action actioneBetweenScenes = null) {
		
		SceneManager.Instance.StartCoroutine(SceneSwitchCoroutine(sceneIndex, actioneBetweenScenes));
	}
			
	public void LoadScene(string sceneName, System.Action actioneBetweenScenes = null) {

		SceneManager.Instance.StartCoroutine(SceneSwitchCoroutine(sceneName, actioneBetweenScenes));
	}

	IEnumerator SceneSwitchCoroutine(int sceneIndex, System.Action actioneBetweenScenes) {
		Application.LoadLevel("LoadingScene");
		yield return new WaitForSeconds(0.1f);
		if (actioneBetweenScenes != null)
			actioneBetweenScenes();
		
		Application.LoadLevel(sceneIndex);
	}

	IEnumerator SceneSwitchCoroutine(string sceneName, System.Action actioneBetweenScenes) {
		Application.LoadLevel("LoadingScene");
		yield return new WaitForSeconds(0.1f);
		if (actioneBetweenScenes != null)
			actioneBetweenScenes();

		Application.LoadLevel(sceneName);
	}
}

