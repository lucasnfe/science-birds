using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProfileQuestionary : Questionary {

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

	protected override IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		SaveToGameData();
		yield return new WaitForSeconds(0.1f);

		GameData.Instance.ShuffleLevelGroups();
		ABSceneManager.Instance.LoadScene(sceneToLoadAfterSubmit, false);		
	}
}
