using UnityEngine;
using UnityEngine.UI;
using System.IO;
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

		string[] levelFiles = Directory.GetFiles (Application.dataPath + ABConstants.LEVELS_FOLDER, "*.xml");
		string[] levelXml = new string[levelFiles.Length];

		for (int i = 0; i < levelFiles.Length; i++)
			levelXml [i] = File.ReadAllText (levelFiles [i]);

		_startPos.x = Mathf.Clamp (_startPos.x, 0, 1f) * Screen.width;
		_startPos.y = Mathf.Clamp (_startPos.y, 0, 1f) * Screen.height;

		LevelList.Instance.LoadLevelsFromSource (levelXml);

		int j = 0;

		for(int i = 0; i < levelXml.Length; i++) {

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
