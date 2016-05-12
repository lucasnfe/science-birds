using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
/**
 *  \struct ImmersionQuestionaryItem
 *  \brief  Struct to hold an item for Immersion Questionary
 *
 *  Struct to hold the question for the Immersion Questionary, 
 *  Its Low Value Meaning and High Value Meaning for scale, and the scale
 */
public struct ImmersionQuestionaryItem {
	/**String containing the question text*/
	public string question;
    /**String containing the low value meaning of the scale*/
    public string scaleLowValueMeaning;
    /**String containing the high value meaning of the scale*/
    public string scaleHighValueMeaning;
    /**Array of strings containing the scale values*/
    public string []scale;
}

/** \class ScaleQuestionary
 *  \brief  Handles the positions and sizes of questions
 *
 *  Manages the questionanry data, scales and positions the questions and boxes,
 *  adds questions to screen, and parses the questionary data
 */
public class ScaleQuestionary : Questionary {
    /**String containing questionary's title*/
	private string _title;
    /**Array fo itens from Immersion Questionary*/
	private ImmersionQuestionaryItem []_items;
	
    /**GameObject of question Prefab*/
	public GameObject _questionPrefab;
    /**GameObject of scale meaning Prefab*/
    public GameObject _scaleMeaningPrefab;
    /**GameObject of scale toggle Prefab*/
    public GameObject _scaleTogglePrefab;
    /**Text Asset for questionary data*/
    public TextAsset _questionaryData;
	
    /**default size for questions*/
	public int _questionSize = 50;
    /**offset of question from title*/
	public int _questionOffsetFromTitle = 15;
    /**offset of toggle box from question*/
	public int _toggleOffsetFromQuestion = 15;
	
	/**
     *  At the initilization of the script, parse questionary data, gets its components, 
     *  Calculates sizes and positions on screen for questions, toggle boxes ad header
     */
	protected virtual void Start () {
		
		if(_questionaryData != null) {
			
			ParseQuestionaryData(_questionaryData.text);
			
			Text title = transform.FindChild("Title").GetComponent<Text>();
			title.text = _title;
			
			Transform scrollView = transform.FindChild("ScrollView");
			RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
			RectTransform panelRect = _questions.GetComponent<RectTransform>();
			
			float newPanelSize = ((float)_items.Length + 0.5f) * _questionSize;
			
			if(newPanelSize > scrollViewRect.sizeDelta.y)
				panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, newPanelSize);
			
			float posX = title.rectTransform.position.x;
			float posY = scrollViewRect.position.y - _questionSize/2f - _questionOffsetFromTitle;
			
			for(int i = 0; i < _items.Length; i++) {
				
				Vector2 questionPos = new Vector2(posX, posY - i * _questionSize);
				AddQuestion(_items[i].question, questionPos);
				
				float toggleWidth = _scaleTogglePrefab.GetComponent<RectTransform>().sizeDelta.x;
				float toggleGroupHalfWidth = (_items[i].scale.Length * toggleWidth)/2f;
				Vector2 scalePos = new Vector2(posX - toggleGroupHalfWidth, questionPos.y - _toggleOffsetFromQuestion);
				AddScale(_items[i].scale, _items[i].scaleLowValueMeaning, _items[i].scaleHighValueMeaning, scalePos, toggleWidth);
			}
			
			scrollView.GetComponent<ScrollRect>().content = panelRect;
		}
	}
    /**
     *  Adds a question game object to game with its desired position.
     *  @param[in]  question    String with question text
     *  @param[in]  position    X, Y coordinate for the position of the question
     */
	void AddQuestion(string question, Vector2 position) {
		
		GameObject questionObj = Instantiate(_questionPrefab) as GameObject;
		questionObj.transform.SetParent(_questions.transform);
		questionObj.name = "Question";
		
		Text questionText = questionObj.GetComponent<Text>();
		questionText.text = question;
		
		RectTransform rectTransform = questionObj.GetComponent<RectTransform>();
		rectTransform.position = position;
	}

    /**
     *  Creates Game Objects for toggle button group, scale low and high meaning,
     *  Instantiates these objects, adds their texts components, anchors them on screen and calculates their positions
     *  @param[in]  scale   Array of strings with texts for toggle buttons.
     *  @param[in]  scaleLowValueMeaning    String containing Low Value Meaning of Scale
     *  @param[in]  scaleHighValueMeaning    String containing High Value Meaning of Scale
     *  @param[in]  position    Vector2 contianing X and Y coordinates to place the toggle buttons
     *  @param[in]  toggleWidth Float value containing the width desired for the toggle button
     */
    void AddScale(string []scale, string scaleLowValueMeaning, string scaleHighValueMeaning, Vector2 position, float toggleWidth) {
		
		GameObject toggleGroup = new GameObject();
		toggleGroup.transform.SetParent(_questions.transform);
		toggleGroup.name = "Scale Toggle Group";
		toggleGroup.AddComponent<ToggleGroup>();
		
		// Add scale low value meaning label
		GameObject scaleLowMeaning = Instantiate(_scaleMeaningPrefab) as GameObject;
		scaleLowMeaning.transform.SetParent(toggleGroup.transform.parent);
		Text scaleLowValueMeaningText = scaleLowMeaning.GetComponent<Text>();
		scaleLowValueMeaningText.text = scaleLowValueMeaning;
		scaleLowValueMeaningText.alignment = TextAnchor.MiddleRight;
		
		RectTransform rectTransform = scaleLowMeaning.GetComponent<RectTransform>();
		rectTransform.position = new Vector2(position.x - rectTransform.sizeDelta.x/1.5f, position.y);
		
		// Add scale toggles
		for(int i = 0; i < scale.Length; i++) {
			
			GameObject toggleObj = Instantiate(_scaleTogglePrefab) as GameObject;
			toggleObj.transform.SetParent (toggleGroup.transform);
			toggleObj.name = "Scale Toggle";
			
			Transform toggleLabel = toggleObj.transform.FindChild("Label");
			Text toggleText = toggleLabel.GetComponent<Text>();
			toggleText.text = scale[i];
			
			// Setting toggle group
			toggleObj.GetComponent<Toggle>().group = toggleGroup.GetComponent<ToggleGroup>();
			
			rectTransform = toggleObj.GetComponent<RectTransform>();
			rectTransform.position = new Vector2(position.x + (i + 0.5f) * toggleWidth, position.y);
		}
		
		// Add scale high value meaning label
		GameObject scaleHighMeaning = Instantiate(_scaleMeaningPrefab) as GameObject;
		scaleHighMeaning.transform.SetParent(toggleGroup.transform.parent);
		Text scaleHighValueMeaningText = scaleHighMeaning.GetComponent<Text>();
		scaleHighValueMeaningText.text = scaleHighValueMeaning;
		scaleHighValueMeaningText.alignment = TextAnchor.MiddleLeft;
		
		rectTransform = scaleHighMeaning.GetComponent<RectTransform>();
		rectTransform.position = new Vector2(position.x + toggleWidth * (scale.Length + 0.5f) + rectTransform.sizeDelta.x/2.5f, position.y);
	}
	/**
     *  Splits questionary data string by line and parses the data of each line accordingly
     *  @param[in]  questionaryData String containing the full questionary data
     */
	void ParseQuestionaryData(string questionaryData) {
		
		string []lines = questionaryData.Split('\n');
		
		if (lines.Length % 2 == 0) 
			throw new UnityException("Questionary file must have an odd number of lines.");
		
		// First line is the questionary title
		_title = lines[0];
		_items = new ImmersionQuestionaryItem[(lines.Length - 1)/4];
		
		// From now on, each pair of lines represents a question an its scale
		for(int i = 1; i < lines.Length; i += 4) {
			
			ImmersionQuestionaryItem item = new ImmersionQuestionaryItem();
			
			item.question = lines[i];
			item.scaleLowValueMeaning = lines[i + 1];
			item.scale = lines[i + 2].Split(' ');
			item.scaleHighValueMeaning = lines[i + 3];
			
			_items[(i - 1)/4] = item;
		}
	}
}
