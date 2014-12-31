using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class LevelGenerator : MonoBehaviour {

	public GameObject _pig;
	public GameObject _bird;
	public GameObject []ABTemplates;
	
	public abstract int DefineBirdsAmount();
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


		int birdsAmount = DefineBirdsAmount();

		Transform ground = transform.Find("Level/Ground");
		BoxCollider2D groundCollider = ground.GetComponent<BoxCollider2D>();
		
		Vector3 birdsPos = GameWorld.Instance._slingshot.transform.position; 
		birdsPos.y = ground.position.y + groundCollider.size.y/2.4f;

		for(int i = 0; i < birdsAmount; i++)
		{
			birdsPos.x -= _bird.GetComponent<SpriteRenderer>().bounds.size.x *2f;

			newGameObject = (GameObject)Instantiate(_bird, birdsPos, _bird.transform.rotation);

			Bird bird = newGameObject.GetComponent<Bird>();
			bird.name = "bird" + (i + 1);
			bird.transform.parent = transform.FindChild("Level/Birds");
		}

		//Time.timeScale = 0f;
	}
}
