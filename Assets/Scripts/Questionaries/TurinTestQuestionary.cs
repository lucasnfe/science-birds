using UnityEngine;
using System.Collections;
/** \class TurinTestQuestionary
 *  \brief  Handles the Turing Test Questionary answers and scene transition.
 */
public class TurinTestQuestionary : Questionary {
    
    /**
     *  Save answers of Turing Test Questionary to the GameData Instance, if it has one answer
     */
    private void SaveToGameData() {
		
		string []answers = GetAnswers();
		
		if (answers.Length != 1) 
			throw new UnityException("Profile questionary must contain 1 question.");
		
		GameData.Instance.AnswersTurinTest = answers[0];
	}
    /**
     *  Saves the questionary to the Game Data and Load the next scene after submiting the answer
     *  @param  [in]    sceneToLoadAfterSubmit  String containing the name of the next scene to be loaded
     */
    protected override IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		SaveToGameData();
		yield return new WaitForSeconds(0.1f);
		ABSceneManager.Instance.LoadScene(sceneToLoadAfterSubmit, false);		
	}
}
