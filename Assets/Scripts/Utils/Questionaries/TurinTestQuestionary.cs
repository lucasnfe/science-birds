using UnityEngine;
using System.Collections;

public class TurinTestQuestionary : Questionary {

	private void SaveToGameData() {
		
		string []answers = GetAnswers();
		
		if (answers.Length != 1) 
			throw new UnityException("Profile questionary must contain 1 question.");
		
		GameData.Instance.AnswersTurinTest = answers[0];
	}
	
	protected override IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		SaveToGameData();
		yield return new WaitForSeconds(0.1f);
		ABSceneManager.Instance.LoadScene(sceneToLoadAfterSubmit, false);		
	}
}
