using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class LevelGenerator : MonoBehaviour {

	public GameObject _pig;
	public GameObject []ABTemplates;
	
	public abstract List<ABGameObject> GenerateLevel();

	void Awake()
	{
		List<ABGameObject> gameObjects = GenerateLevel();

		GameObject newGameObject = null;

		foreach(ABGameObject gameObj in gameObjects)
		{
			if(gameObj.Label < ABTemplates.Length)

				newGameObject = (GameObject)Instantiate(ABTemplates[gameObj.Label], gameObj.Position, ABTemplates[gameObj.Label].transform.rotation);
			else
				newGameObject = (GameObject)Instantiate(_pig, gameObj.Position, _pig.transform.rotation);

			newGameObject.transform.parent = transform.FindChild("Level/Blocks");
		}

		//Time.timeScale = 0f;
	}
}
