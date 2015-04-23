using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public struct QuestionaryItem {
	
	public string question;
	public string []scale;
}

public class Questionary : MonoBehaviour {
	
	private string _title;
	private QuestionaryItem []_items;

	public GameObject _questionPrefab;
	public GameObject _scaleTogglePrefab;
	public TextAsset _questionaryData;

	public int _questionSize = 50;
	public int _questionOffsetFromTitle = 15;
	public int _toggleOffsetFromQuestion = 15;
	public string _url;

	// Use this for initialization
	void Start () {

		if(_questionaryData != null) {

			ParseQuestionaryData(_questionaryData.text);
	
			Text title = transform.FindChild("Title").GetComponent<Text>();
			title.text = _title;

			Transform scrollView = transform.FindChild("ScrollView");
			RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
			RectTransform panelRect = scrollView.FindChild("Panel").GetComponent<RectTransform>();

			float newPanelSize = ((float)_items.Length + 0.5f) * _questionSize;

			if(newPanelSize > scrollViewRect.sizeDelta.y)
				panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, newPanelSize);

			float posX = title.rectTransform.position.x;
			float posY = scrollViewRect.position.y - _questionSize/2f;

			for(int i = 0; i < _items.Length; i++) {

				Vector2 questionPos = new Vector2(posX, posY - i * _questionSize);
				AddQuestion(_items[i].question, questionPos);

				float toggleWidth = _scaleTogglePrefab.GetComponent<RectTransform>().sizeDelta.x;
				float toggleGroupHalfWidth = (_items[i].scale.Length * toggleWidth)/2f;
				Vector2 scalePos = new Vector2(posX - toggleGroupHalfWidth, questionPos.y - _toggleOffsetFromQuestion);
				AddScale(_items[i].scale, scalePos, toggleWidth);
			}

			scrollView.GetComponent<ScrollRect>().content = panelRect;
		}
	}

	void AddQuestion(string question, Vector2 position) {
		
		GameObject questionObj = Instantiate(_questionPrefab) as GameObject;
		questionObj.transform.parent = transform.FindChild("ScrollView").FindChild("Panel");
		questionObj.name = "Question";
		
		Text questionText = questionObj.GetComponent<Text>();
		questionText.text = question;

		RectTransform rectTransform = questionObj.GetComponent<RectTransform>();
		rectTransform.position = position;
	}

	void AddScale(string []scale, Vector2 position, float toggleWidth) {
			
		GameObject toggleGroup = new GameObject();
		toggleGroup.transform.parent = transform.FindChild("ScrollView").FindChild("Panel");
		toggleGroup.name = "Scale Toggle Group";
		toggleGroup.AddComponent<ToggleGroup>();

		for(int i = 0; i < scale.Length; i++) {

			GameObject toggleObj = Instantiate(_scaleTogglePrefab) as GameObject;
			toggleObj.transform.parent = toggleGroup.transform;
			toggleObj.name = "Scale Toggle";

			Transform toggleLabel = toggleObj.transform.FindChild("Label");
			Text toggleText = toggleLabel.GetComponent<Text>();
			toggleText.text = scale[i];

			// Setting toggle group
			toggleObj.GetComponent<Toggle>().group = toggleGroup.GetComponent<ToggleGroup>();

			RectTransform rectTransform = toggleObj.GetComponent<RectTransform>();
			rectTransform.position = new Vector2(position.x + (i + 0.5f) * toggleWidth, position.y);
		}
	}

	void ParseQuestionaryData(string questionaryData) {

		string []lines = questionaryData.Split('\n');

		if (lines.Length % 2 != 0) 
			throw new UnityException("Questionary file must have an even number of lines.");

		// First line is the questionary title
		_title = "Level " + LevelSource.CurrentLevel.ToString();
		_items = new QuestionaryItem[lines.Length/2];

		// From now on, each pair of lines represents a question an its scale
		for(int i = 0; i < lines.Length; i += 2) {

			QuestionaryItem item = new QuestionaryItem();

			item.question = lines[i];
			item.scale = lines[i + 1].Split(' ');

			_items[i/2] = item;
		}
	}

	string []GetAnswers() {

		Transform panel = transform.FindChild("ScrollView/Panel");
		ToggleGroup []toggleGroups = panel.GetComponentsInChildren<ToggleGroup>();
		
		string []answers = new string[toggleGroups.Length];
		
		for(int i = 0; i < toggleGroups.Length; i++) {
			
			int selectedGroup = 0;
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

	private WWW CreateRequest(string url) {

		string []answers = GetAnswers();

		string envelopeAnswers = "";
		foreach(string ans in answers)
			envelopeAnswers += "<intf:answers>" + ans + "</intf:answers>";

		string envelope = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>" +
			"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
			"xmlns:apachesoap=\"http://xml.apache.org/xml-soap\" " +
			"xmlns:impl=\"http://questionaryserver.angrybirdsendless.org\" " +
			"xmlns:intf=\"http://questionaryserver.angrybirdsendless.org\" " +
			"xmlns:wsdl=\"http://schemas.xmlsoap.org/wsdl/\" " +
			"xmlns:wsdlsoap=\"http://schemas.xmlsoap.org/wsdl/soap/\" " +
			"xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
			"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
			"<SOAP-ENV:Body><intf:saveQuestionary xmlns:intf=\"http://questionaryserver.angrybirdsendless.org\">" +
			"<intf:levelName>" + _title + "</intf:levelName>" +
			envelopeAnswers +
			"</intf:saveQuestionary>" +
			"</SOAP-ENV:Body>" +
			"</SOAP-ENV:Envelope>";

		Hashtable headers = new Hashtable();

		//headers["Host"] = "52.0.243.131";
		headers["Content-Type"] = "text/xml; charset=utf-8";
		//headers["Content-Length"] = envelope.Length.ToString();
		headers["SOAPAction"] = "\"http://52.0.243.131:8080/ABQuestionaryServer/services/QuestionaryServer/saveQuestionary\"";

		return new WWW (url, System.Text.Encoding.UTF8.GetBytes(envelope), headers);
	}

	public void SubmitQuestionary() {

		StartCoroutine("SendQuestionary");
	}

	public IEnumerator SendQuestionary() {
		
		WWW www = CreateRequest(_url);
		yield return www;

		if (www.error == null) {

			SceneManager.Instance.LoadScene("Level4");
		}
		else 
			Debug.Log("Error submiting the Questionary: could not reach the server.");
	}
}