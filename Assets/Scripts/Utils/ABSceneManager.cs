using UnityEngine;
using System.Collections;

public class ABSceneManager : ABSingleton<ABSceneManager> {

	public AudioClip _backgroundMusic;

	private int _lastScene;
	public int LastScene {
		get { return _lastScene; }
	}

	void Start() {

		_backgroundMusic = Resources.Load("title_theme") as AudioClip;
	}
			
	public void LoadScene(string sceneName, bool showLoadingScreen = true, System.Action actioneBetweenScenes = null) {

		ABSceneManager.Instance.StartCoroutine(SceneSwitchCoroutine(sceneName, showLoadingScreen, actioneBetweenScenes));
	}

	IEnumerator SceneSwitchCoroutine(string sceneName, bool showLoadingScreen, System.Action actioneBetweenScenes) {
		_lastScene = Application.loadedLevel;
		if(showLoadingScreen) 
			Application.LoadLevel("LoadingScene");

		yield return new WaitForSeconds(0.1f);

		if (actioneBetweenScenes != null)
			actioneBetweenScenes();

		Application.LoadLevel(sceneName);

		if (sceneName.StartsWith("Questionary")) {
			if(!ABAudioController.Instance.IsPlayingMusic(_backgroundMusic))
				ABAudioController.Instance.PlayMusic(_backgroundMusic);
		}
		else
			ABAudioController.Instance.StopMusic();
	}
}

