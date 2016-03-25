using UnityEngine;
using System.Collections;

/** \class ABSceneManager
 *  \brief  Controller of Scenes
 *
 *  Manages the background music of the scenes, places loading screen during scenes transition and load new scenes
 */
public class ABSceneManager : ABSingleton<ABSceneManager> {
    /**
     * Background Music to be played at the current level
     */
	public AudioClip _backgroundMusic;

    /**
     *  index of last loaded scene
     */
	private int _lastScene;
    /**
     *  Accessor for _lastScene, index os last loaded scene 
     */
	public int LastScene {
		get { return _lastScene; }
	}

    /**
     *  Starts background music for the game
     */
	void Start() {

		_backgroundMusic = Resources.Load("title_theme") as AudioClip;
	}

    /**
     *  Load new scene, calls the SceneSwitchCoroutine
     *  @param [in] sceneName   string containing the name of the new scene
     *  @param [in] showLoadingScreen boolean to decide if loading screen should be loaded. Default true
     *  @param [in] actioneBetweenScenes function pointer (delegate) to desired system action. Default null
     */
    public void LoadScene(string sceneName, bool showLoadingScreen = true, System.Action actioneBetweenScenes = null) {

		ABSceneManager.Instance.StartCoroutine(SceneSwitchCoroutine(sceneName, showLoadingScreen, actioneBetweenScenes));
	}

    /**
     *  Coroutine to switch scenes. Saves last scene, shows loading screen if needed, calls for action between scenes if needed
     *  and then loads the next level. If the new level is the questionary, plays the background music, 
     *  if not, stops the music
     *  @param [in] sceneName   string containing the name of the new scene
     *  @param [in] showLoadingScreen boolean to decide if loading screen should be loaded
     *  @param [in] actioneBetweenScenes function pointer (delegate) to desired system action
     */
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

