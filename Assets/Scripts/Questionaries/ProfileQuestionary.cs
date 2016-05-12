using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/** \class ProfileQuestionary
 *  \brief  Handles the Profile Questionary answers and scene transition.
 */
public class ProfileQuestionary : Questionary {

    /**
     *  Save answers of Profile Questionary of Age, Gender, Game Experience, Angry Birds Experience and Educational level to GameData Instance
     *  Also throws exception if answer vector size is differente than expected (5)
     */
    private void SaveToGameData() {
		
		string []answers = GetAnswers();
		
		if (answers.Length != 5) 
			throw new UnityException("Profile questionary must contain 5 questions.");

		GameData.Instance.Age = answers[0];
		GameData.Instance.Gender = answers[1];
		GameData.Instance.GameXP = answers[2];
		GameData.Instance.AngryBirdsXP = answers[3];
		GameData.Instance.Education = answers[4].Substring(0, answers[4].Length - 1);
	}

    /**
     *  Saves the questionary to the Game Data, Shuffle the Level Groups, choosing randomly with equal chances if Group A will contain 
     *  Generated or Rovio Levels. Group B will have the other one. Also Load the next scene after submiting the answer
     *  @param  [in]    sceneToLoadAfterSubmit  String containing the name of the next scene to be loaded
     */
    protected override IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		SaveToGameData();
		yield return new WaitForSeconds(0.1f);

		GameData.Instance.ShuffleLevelGroups();
		ABSceneManager.Instance.LoadScene(sceneToLoadAfterSubmit, false);		
	}
}
