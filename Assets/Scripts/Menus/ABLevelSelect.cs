using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ABLevelSelect : ABMenu {

	public GameObject _levelSelector;
	public GameObject _canvas;

	public int _lines = 5;

	public Vector2 _startPos;
	public Vector2 _buttonSize;

	private int _clickedButton;

	// Use this for initialization
	void Start () {
	
		Object[] levelSet = Resources.LoadAll ("Levels");

		// Transform loaded list into text assets
		TextAsset[] levelTexts = new TextAsset[levelSet.Length];

		for(int i = 0; i < levelSet.Length; i++)
			levelTexts[i] = (TextAsset)levelSet[i];	

		LevelList.Instance.LoadLevelsFromSource (levelTexts);

		int j = 0;

		for(int i = 0; i < levelSet.Length; i++) {

			Vector2 pos = _startPos + new Vector2 ((i % _lines) * _buttonSize.x, j * _buttonSize.y);

			GameObject obj = Instantiate (_levelSelector, pos, Quaternion.identity) as GameObject;
			obj.transform.SetParent(_canvas.transform);

			ABLevelSelector sel = obj.AddComponent<ABLevelSelector> ();
			sel.LevelIndex = i;

			Button selectButton = obj.GetComponent<Button> ();

			selectButton.onClick.AddListener (delegate { 
				
				LoadNextScene("GameWorld", true, sel.UpdateLevelList); });

			Text selectText = selectButton.GetComponentInChildren<Text> ();
			selectText.text = "" + (i + 1);

			if ((i + 1) % _lines == 0)
				j--;
		}
	}
}
