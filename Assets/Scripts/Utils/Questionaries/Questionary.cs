using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Questionary : MonoBehaviour {
	
	public GameObject _questions;

	protected virtual void Update() {
		
		Button button = transform.GetComponentInChildren<Button>();
		if(_questions != null && button != null)
			button.interactable = (GetAnswers().Length == GetNonEmptyAnswersAmount());
	}

	protected void SetQuestions(string []questions) {
		
		GameObject []questionObjs = GameObject.FindGameObjectsWithTag("Question");

		for(int i = 0; i < questionObjs.Length; i++)
			questionObjs[i].GetComponent<Text>().text = questions[i];
	}

	protected string []GetQuestions() {

		GameObject []questionObjs = GameObject.FindGameObjectsWithTag("Question");
		string []questions = new string[questionObjs.Length];

		for(int i = 0; i < questionObjs.Length; i++)
			questions[i] = questionObjs[i].GetComponent<Text>().text;

		return questions;
	}

	protected int GetNonEmptyAnswersAmount() {

		int nonEmptyAnswers = 0;
		ToggleGroup []toggleGroups = _questions.GetComponentsInChildren<ToggleGroup>();
		
		for(int i = 0; i < toggleGroups.Length; i++) {
			
			Toggle []toggles = toggleGroups[i].transform.GetComponentsInChildren<Toggle>();
			
			for(int j = 0; j < toggles.Length; j++) {
				
				if(toggles[j].isOn)
					nonEmptyAnswers++;
			}
		}
		
		return nonEmptyAnswers;
	}
	
	protected string []GetAnswers() {

		ToggleGroup []toggleGroups = _questions.GetComponentsInChildren<ToggleGroup>();
		string []answers = new string[toggleGroups.Length];
		
		for(int i = 0; i < toggleGroups.Length; i++) {

			Toggle []toggles = toggleGroups[i].transform.GetComponentsInChildren<Toggle>();
			
			for(int j = 0; j < toggles.Length; j++) {
				
				if(toggles[j].isOn) {
					answers[i] = toggles[j].GetComponentInChildren<Text>().text;
					break;
				}
			}
		}

		return answers;
	}
	
	public void SubmitQuestionary(string sceneToLoadAfterSubmit) {

		StartCoroutine(SendQuestionary(sceneToLoadAfterSubmit));
	}

	protected virtual IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		yield return new WaitForSeconds(0.1f);
	}
}