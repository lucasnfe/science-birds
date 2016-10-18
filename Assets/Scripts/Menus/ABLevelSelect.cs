// SCIENCE BIRDS: A clone version of the Angry Birds game used for 
// research purposes
// 
// Copyright (C) 2016 - Lucas N. Ferreira - lucasnfe@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
//

﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Linq;

public class ABLevelSelect : ABMenu {

	public int _lines = 5;

	public GameObject _levelSelector;
	public GameObject _canvas;

	public Vector2 _startPos;
	public Vector2 _buttonSize;

	private int _clickedButton;

	// Use this for initialization
	void Start () {

		// Load levels in the resources folder
		TextAsset []levelsData = Resources.LoadAll<TextAsset>(ABConstants.DEFAULT_LEVELS_FOLDER);

		string[] resourcesXml = new string[levelsData.Length];
		for (int i = 0; i < levelsData.Length; i++)
			resourcesXml [i] = levelsData[i].text;
			

#if UNITY_WEBGL && !UNITY_EDITOR

		// WebGL builds does not load local files
		string[] streamingXml = new string[0];

#else
		// Load levels in the streaming folder
		string   levelsPath = Application.dataPath + ABConstants.CUSTOM_LEVELS_FOLDER;
		string[] levelFiles = Directory.GetFiles (levelsPath, "*.xml");

		string[] streamingXml = new string[levelFiles.Length];
		for (int i = 0; i < levelFiles.Length; i++)
			streamingXml [i] = File.ReadAllText (levelFiles [i]);

#endif

		// Combine the two sources of levels
		string[] allXmlFiles = new string[resourcesXml.Length + streamingXml.Length];
		resourcesXml.CopyTo(allXmlFiles, 0);
		streamingXml.CopyTo(allXmlFiles, resourcesXml.Length);

		_startPos.x = Mathf.Clamp (_startPos.x, 0, 1f) * Screen.width;
		_startPos.y = Mathf.Clamp (_startPos.y, 0, 1f) * Screen.height;

		LevelList.Instance.LoadLevelsFromSource (allXmlFiles);

		int j = 0;

		for(int i = 0; i < allXmlFiles.Length; i++) {

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
