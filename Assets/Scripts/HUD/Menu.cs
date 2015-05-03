using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
	
	public void LoadCurrentLevelGroupA() 
	{
		GameData.Instance.CurrentQuestionary = "Questionary_GroupA";
		LevelSource.CurrentLevel = 0;
		ABSceneManager.Instance.LoadScene(GameData.Instance.LevelGroupA, true);
	}

	public void LoadCurrentLevelGroupB() 
	{
		GameData.Instance.CurrentQuestionary = "Questionary_GroupB";
		LevelSource.CurrentLevel = 0;
		ABSceneManager.Instance.LoadScene(GameData.Instance.LevelGroupB, true);
	}

	public void LoadLevelWithLoadingScreen(string sceneName)
	{
		ABSceneManager.Instance.LoadScene(sceneName, true);
	}

	public void LoadLevel(string sceneName)
	{
		ABSceneManager.Instance.LoadScene(sceneName, false);
	}
}
