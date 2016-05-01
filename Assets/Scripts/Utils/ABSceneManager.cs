using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ABSceneManager : ABSingleton<ABSceneManager> {

	public delegate void ActionBetweenScenes();

	public AudioClip _backgroundMusic;

	private string _lastSceneName;
	public string LastSceneName { get { return _lastSceneName; } }

	private int _lastScene;
	public int LastScene { get { return _lastScene; } }

	private string _currentSceneName;
	public string CurrentSceneName { get { return _currentSceneName; } }

	private int _currentScene;
	public int CurrentScene { get { return _currentScene; } }

	void Start() {

		_backgroundMusic = Resources.Load("Audio/title_theme") as AudioClip;
	}
			
	public void LoadScene (string sceneName, bool showLoadingScreen = true, ActionBetweenScenes actioneBetweenScenes = null) {

		ABSceneManager.Instance.StartCoroutine(SceneSwitchCoroutine(sceneName, showLoadingScreen, actioneBetweenScenes));
	}

	IEnumerator SceneSwitchCoroutine (string sceneName, bool showLoadingScreen, ActionBetweenScenes actioneBetweenScenes) {

		_lastScene = SceneManager.GetActiveScene ().buildIndex;
		_lastSceneName = SceneManager.GetActiveScene ().name;

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

		_currentScene = SceneManager.GetActiveScene ().buildIndex;
		_currentSceneName = SceneManager.GetActiveScene ().name;
	}
}

