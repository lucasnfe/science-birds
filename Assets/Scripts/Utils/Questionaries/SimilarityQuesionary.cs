using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimilarityQuesionary : ScaleQuestionary {

	private bool _waitingServerConfirmation;

	protected override void Start() {

		base.Start();

		string []quesionsWithUpdateLevelGroup = GetQuestions();

		// Change quesitons that depend on the level group order
		quesionsWithUpdateLevelGroup[0] = quesionsWithUpdateLevelGroup[0].Replace("Group B", GameData.Instance.GetGeneratedGroupName());
		quesionsWithUpdateLevelGroup[2] = quesionsWithUpdateLevelGroup[2].Replace("Group B", GameData.Instance.GetGeneratedGroupName());

		SetQuestions(quesionsWithUpdateLevelGroup);
	}

	protected override void Update() {

		if(!_waitingServerConfirmation)
			base.Update();
	}

	private void SaveToGameData() {

		GameData.Instance.AnswersSimilarityQuestionary = GetAnswers();
	}
	
	protected override IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {

		_waitingServerConfirmation = true;

		Button button = transform.GetComponentInChildren<Button>();
		if(button != null)
			button.interactable = false;

		SaveToGameData();

		WWW www = GameData.Instance.CreateRequest();
		yield return www;
		
		if (www.error == null) 
		{	
			Debug.Log("Game data submited: " + www.text);
			ABSceneManager.Instance.LoadScene(sceneToLoadAfterSubmit, false);
		}
		else 
		{
			_waitingServerConfirmation = false;
			button.interactable = true;
			Debug.Log("Error submiting the Questionary: could not reach the server.");
		}
	}
}
