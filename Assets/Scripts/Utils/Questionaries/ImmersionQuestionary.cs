using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ImmersionQuestionary : ScaleQuestionary {

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
	
	protected override IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		char questionaryIndex = _questionaryData.name[_questionaryData.name.Length - 1];
		SaveToGameData(questionaryIndex);

		yield return new WaitForSeconds(0.1f);
		ABSceneManager.Instance.LoadScene(sceneToLoadAfterSubmit, false);
	}
}