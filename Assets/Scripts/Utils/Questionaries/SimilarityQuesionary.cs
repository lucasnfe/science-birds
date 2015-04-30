using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimilarityQuesionary : ScaleQuestionary {

	protected override void Start() {

		base.Start();

		string []quesionsWithUpdateLevelGroup = GetQuestions();

		// Change quesitons that depend on the level group order
		quesionsWithUpdateLevelGroup[0] = quesionsWithUpdateLevelGroup[0].Replace("Group B", GameData.Instance.GetGeneratedGroupName());
		quesionsWithUpdateLevelGroup[2] = quesionsWithUpdateLevelGroup[2].Replace("Group B", GameData.Instance.GetGeneratedGroupName());

		SetQuestions(quesionsWithUpdateLevelGroup);
	}

	private void SaveToGameData() {

		GameData.Instance.AnswersSimilarityQuestionary = GetAnswers();
	}
	
	protected override IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		SaveToGameData();

		WWW www = GameData.Instance.CreateRequest();
		yield return www;
		
		if (www.error == null) {
			
			Debug.Log("Game data submited: " + www.text);
			SceneManager.Instance.LoadScene(sceneToLoadAfterSubmit, false);
		}
		else 
			Debug.Log("Error submiting the Questionary: could not reach the server.");
	}
}
