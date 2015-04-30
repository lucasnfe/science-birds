using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
	
	public void LoadCurrentLevelGroupA() 
	{
		GameData.Instance.CurrentQuestionary = "QuestionaryA";
		LevelSource.CurrentLevel = 0;
		SceneManager.Instance.LoadScene(GameData.Instance.LevelGroupA, true);
	}

	public void LoadCurrentLevelGroupB() 
	{
		GameData.Instance.CurrentQuestionary = "QuestionaryB";
		LevelSource.CurrentLevel = 0;
		SceneManager.Instance.LoadScene(GameData.Instance.LevelGroupB, true);
	}

	public void LoadLevelWithLoadingScreen(string sceneName)
	{
		SceneManager.Instance.LoadScene(sceneName, true);
	}

	public void LoadLevel(string sceneName)
	{
		SceneManager.Instance.LoadScene(sceneName, false);
	}
}
