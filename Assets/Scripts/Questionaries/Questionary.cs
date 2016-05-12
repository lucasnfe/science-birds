using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/** \class Questionary
 *  \brief  Handles the general Questionary behaviours
 *
 *  Manages the questions, updates the button status, sets the questions text, gets the questions text 
 *  Check quantity of non answered questions and gets answers from questions
 */
public class Questionary : MonoBehaviour {
	
    /**GameObject for the questions*/
	public GameObject _questions;
    /**
     *  Updates button status at every Update cicle, making it interactable only if all answers are already given and the game object _questions is not null
     */
	protected virtual void Update() {
		
		Button button = transform.GetComponentInChildren<Button>();
		if(_questions != null && button != null)
			button.interactable = (GetAnswers().Length == GetNonEmptyAnswersAmount());
	}
    /**
     *  Set the text components of GameObjects with tag "Question" to the corresponding question text
     *  @param questions    array of strings with the text of each question
     */
	protected void SetQuestions(string []questions) {
		
		GameObject []questionObjs = GameObject.FindGameObjectsWithTag("Question");

		for(int i = 0; i < questionObjs.Length; i++)
			questionObjs[i].GetComponent<Text>().text = questions[i];
	}
    /**
     *  Get the question texts from Game Objects with the tag "Question"
     *  @return string[]    Array with strings containing the text of each question
     */
	protected string []GetQuestions() {

		GameObject []questionObjs = GameObject.FindGameObjectsWithTag("Question");
		string []questions = new string[questionObjs.Length];

		for(int i = 0; i < questionObjs.Length; i++)
			questions[i] = questionObjs[i].GetComponent<Text>().text;

		return questions;
	}
    /**
     *  Gets the amount of answers that have been already answered by checking Toggle components from _questions Game Object ToggleGroup components
     *  @return int Number of answers already answered (Non Empty)
     */
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
	/**
     *  Get text from all the answers by accessing Text Component from Toggle component in ToggleGroups components of Game Object _questions
     *  @return string[]    Array of strings containing the text of each answer
     */
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
	/**
     *  Submit the questionary passing the next scene to be loaded, starts the coroutine SendQuestionary
     *  @param[in]  sceneToLoadAfterSubmit  String with the name of the next scene.
     */
	public void SubmitQuestionary(string sceneToLoadAfterSubmit) {

		StartCoroutine(SendQuestionary(sceneToLoadAfterSubmit));
	}
    /**
     *  Sends the questionary passing the next scene to be loaded
     *  @param[in]  sceneToLoadAfterSubmit  String with the name of next Scene
     */
	protected virtual IEnumerator SendQuestionary(string sceneToLoadAfterSubmit) {
		
		yield return new WaitForSeconds(0.1f);
	}
}