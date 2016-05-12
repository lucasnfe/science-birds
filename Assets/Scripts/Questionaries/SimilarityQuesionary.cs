using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/** \class SimilarityQuestionary
 *  \brief  Handles the Similarity Questionary behaviours and submits the answers
 *
 *  Manages the questions based on level group order, save them to game data
 *  And submits questionary
 */
public class SimilarityQuesionary : ScaleQuestionary {

    /**Boolean to know if is still waiting confirmation of questionary submission from server*/
	private bool _waitingServerConfirmation;

    /**
     *  Called on initialization of Game Object, starts the base, gets questions for the level group,
     *  changes the quesitons that depend on the level group order and sets them for showing
     */
    protected override void Start() {

		base.Start();

		string []quesionsWithUpdateLevelGroup = GetQuestions();

		// Change quesitons that depend on the level group order
		quesionsWithUpdateLevelGroup[0] = quesionsWithUpdateLevelGroup[0].Replace("Group B", GameData.Instance.GetGeneratedGroupName());
		quesionsWithUpdateLevelGroup[2] = quesionsWithUpdateLevelGroup[2].Replace("Group B", GameData.Instance.GetGeneratedGroupName());

		SetQuestions(quesionsWithUpdateLevelGroup);
	}
    /**
     *  Updates the base if not waiting confirmation from submission to server
     */
	protected override void Update() {

		if(!_waitingServerConfirmation)
			base.Update();
	}
    /**
     *  Save answers of Similarity Questionary to the GameData Instance
     */
    private void SaveToGameData() {

		GameData.Instance.AnswersSimilarityQuestionary = GetAnswers();
	}
    /**
     *  Starts waiting for server confirmation, updates button interactiviy, 
     *  Saves the questionary to the Game Data, creates request to submit questionary
     *  and Load the next scene after submiting the answer, if it worked correctly
     *  @param  [in]    sceneToLoadAfterSubmit  String containing the name of the next scene to be loaded
     */
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
