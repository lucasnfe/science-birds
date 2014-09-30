using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public abstract class LevelGenerator : MonoBehaviour {

	public GameObject []ABTemplates;
	
	public abstract List<ABGameObject> GenerateLevel();

	void Awake()
	{
		List<ABGameObject> gameObjects = GenerateLevel();

		foreach(ABGameObject gameObj in gameObjects)
		{
			GameObject newGameObject = (GameObject)Instantiate(ABTemplates[gameObj.Label], gameObj.Position, Quaternion.identity);
			newGameObject.transform.parent = transform.FindChild("Level/Blocks");
		}

		//Time.timeScale = 0f;
	}
}
