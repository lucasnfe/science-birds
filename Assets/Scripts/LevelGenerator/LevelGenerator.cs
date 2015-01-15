using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class LevelGenerator : MonoBehaviour {
		
	public abstract int DefineBirdsAmount();
	public abstract List<ABGameObject> GenerateLevel();

	public virtual void Start()
	{
		List<ABGameObject> gameObjects = GenerateLevel();
		int birdsAmount = DefineBirdsAmount();

		DecodeLevel(gameObjects, birdsAmount);
	}

	protected void DecodeLevel(List<ABGameObject> gameObjects, int birdsAmount) 
	{
		foreach(ABGameObject gameObj in gameObjects)
		{
			if(gameObj.Label < GameWorld.Instance.Templates.Length)
				
				GameWorld.Instance.AddBlock(GameWorld.Instance.Templates[gameObj.Label], gameObj.Position, 
				                            GameWorld.Instance.Templates[gameObj.Label].transform.rotation);
			else
				GameWorld.Instance.AddPig(GameWorld.Instance._pig, gameObj.Position, 
				                          GameWorld.Instance._pig.transform.rotation);
		}

		Transform ground = GameWorld.Instance.transform.Find("Ground");
		BoxCollider2D groundCollider = ground.GetComponent<BoxCollider2D>();

		//First bird must be in the slingshot
		GameWorld.Instance.AddBird(GameWorld.Instance._bird, GameWorld.Instance._slingSelectPos, 
		                           GameWorld.Instance._bird.transform.rotation, "bird0", true);

		if(birdsAmount < 2)
			return;

		Vector3 birdsPos = GameWorld.Instance._slingshot.transform.position;
		birdsPos.y = ground.position.y + groundCollider.size.y/2.4f;
		
		for(int i = 1; i < birdsAmount; i++)
		{
			birdsPos.x -= GameWorld.Instance._bird.GetComponent<SpriteRenderer>().bounds.size.x *2f;
			GameWorld.Instance.AddBird(GameWorld.Instance._bird, birdsPos, GameWorld.Instance._bird.transform.rotation, "bird" + i);
		}
	}
}
