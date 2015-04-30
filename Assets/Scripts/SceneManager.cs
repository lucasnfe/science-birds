using UnityEngine;
using System.Collections;

public class SceneManager : ABSingleton<SceneManager> {

	private int _lastScene;
	public int LastScene {
		get { return _lastScene; }
	}

	public void LoadScene(int sceneIndex, bool showLoadingScreen = true, System.Action actioneBetweenScenes = null) {
		
		SceneManager.Instance.StartCoroutine(SceneSwitchCoroutine(sceneIndex, showLoadingScreen, actioneBetweenScenes));
	}
			
	public void LoadScene(string sceneName, bool showLoadingScreen = true, System.Action actioneBetweenScenes = null) {

		SceneManager.Instance.StartCoroutine(SceneSwitchCoroutine(sceneName, showLoadingScreen, actioneBetweenScenes));
	}

	IEnumerator SceneSwitchCoroutine(int sceneIndex, bool showLoadingScreen, System.Action actioneBetweenScenes) {
		_lastScene = Application.loadedLevel;
		if(showLoadingScreen) 
			Application.LoadLevel("LoadingScene");
		
		yield return new WaitForSeconds(0.1f);
		
		if (actioneBetweenScenes != null)
			actioneBetweenScenes();

		Application.LoadLevel(sceneIndex);
	}

	IEnumerator SceneSwitchCoroutine(string sceneName, bool showLoadingScreen, System.Action actioneBetweenScenes) {
		_lastScene = Application.loadedLevel;
		if(showLoadingScreen) 
			Application.LoadLevel("LoadingScene");

		yield return new WaitForSeconds(0.1f);

		if (actioneBetweenScenes != null)
			actioneBetweenScenes();

		Application.LoadLevel(sceneName);
	}
}

