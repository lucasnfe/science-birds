using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ABSceneManager : ABSingleton<ABSceneManager> {

	public delegate void ActionBetweenScenes();

	public AudioClip _backgroundMusic;

	private int _lastScene;

	public int LastScene { get { return _lastScene; } }

	void Start() {

		_backgroundMusic = Resources.Load("title_theme") as AudioClip;
	}
			
	public void LoadScene (string sceneName, bool showLoadingScreen = true, ActionBetweenScenes actioneBetweenScenes = null) {

		ABSceneManager.Instance.StartCoroutine(SceneSwitchCoroutine(sceneName, showLoadingScreen, actioneBetweenScenes));
	}

	IEnumerator SceneSwitchCoroutine (string sceneName, bool showLoadingScreen, ActionBetweenScenes actioneBetweenScenes) {

		_lastScene = SceneManager.GetActiveScene ().buildIndex;

		if(showLoadingScreen) 
			SceneManager.LoadScene("LoadingScene");

		yield return new WaitForSeconds(0.1f);

		if (actioneBetweenScenes != null)
			actioneBetweenScenes();

		SceneManager.LoadScene(sceneName);

		if (sceneName.EndsWith("Menu")) {
			if(!ABAudioController.Instance.IsPlayingMusic(_backgroundMusic))
				ABAudioController.Instance.PlayMusic(_backgroundMusic);
		}
		else
			ABAudioController.Instance.StopMusic();
	}
}

