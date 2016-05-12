using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/** \class ImmersionQuestionary
 *  \brief  Handles the Immersion Questionary answers and scene transition.
 */
public class ImmersionQuestionary : ScaleQuestionary {

    /**
     *  Save answers of Immersion Questionary of each Group (A or B) to the GameData Instance
     *  @param  [in]    questionaryIndex    Index of the questionary ('A' or 'B')
     */
	private void SaveToGameData(char questionaryIndex) {

		switch(questionaryIndex) {

			case 'A':
				GameData.Instance.AnswersImmersionQuestionaryA = GetAnswers();
				break;

			case 'B':
				GameData.Instance.AnswersImmersionQuestionaryB = GetAnswers();
				break;
		}
	}

	/**
     *  Saves the questionary to the Game Data and Load the next scene after submiting the answer
     *  @param  [in]    sceneToLoadAfterSubmit  String containing the name of the next scene to be loaded
     */
	protected override IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		char questionaryIndex = _questionaryData.name[_questionaryData.name.Length - 1];
		SaveToGameData(questionaryIndex);

		yield return new WaitForSeconds(0.1f);
		ABSceneManager.Instance.LoadScene(sceneToLoadAfterSubmit, false);
	}
}