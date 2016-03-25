using UnityEngine;
using System.Collections;
/** \class Menu
 *  \brief  Handles the Loading of screens from menu
 *
 *  Loads current level for group A or B and calls to load scene with or without loading screen
 */
public class Menu : MonoBehaviour {
    /**
     *  Loads first Level for Group A
     */
    public void LoadCurrentLevelGroupA() 
	{
		GameData.Instance.CurrentQuestionary = "Questionary_GroupA";
		LevelSource.CurrentLevel = 0;
		ABSceneManager.Instance.LoadScene(GameData.Instance.LevelGroupA, true);
	}
    /**
     *  Loads first Level for Group B
     */
    public void LoadCurrentLevelGroupB() 
	{
		GameData.Instance.CurrentQuestionary = "Questionary_GroupB";
		LevelSource.CurrentLevel = 0;
		ABSceneManager.Instance.LoadScene(GameData.Instance.LevelGroupB, true);
	}
    /**
     *  Calls to Load Scene with Loading Screen set to true
     *  @param[in]  sceneName   Name of scene to be loaded
     */
    public void LoadLevelWithLoadingScreen(string sceneName)
	{
		ABSceneManager.Instance.LoadScene(sceneName, true);
	}
    /**
     *  Calls to Load Scene with Loading Screen set to false
     *  @param[in]  sceneName   Name of scene to be loaded
     */
    public void LoadLevel(string sceneName)
	{
		ABSceneManager.Instance.LoadScene(sceneName, false);
	}
}
