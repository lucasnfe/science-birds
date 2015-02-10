using UnityEngine;
using System;
using System.Collections.Generic;

public class ABLevel 
{
	public int birdsAmount;
	
	public float width;
	public float height;
	
	public List<ABGameObject> gameObjects;
}

public abstract class ABLevelGenerator : MonoBehaviour {
		
	public abstract ABLevel GenerateLevel();

	public virtual void Start()
	{
		ABLevel level = GenerateLevel();
		
		if(level.gameObjects != null)
		{
			DecodeLevel(level.gameObjects, level.birdsAmount);
						
			ABGameObject gameObj = level.gameObjects[0];
		
			float levelLeftBound = GameWorld.Instance._ground.transform.position.x - 
				GameWorld.Instance._ground.collider2D.bounds.size.x/2f;
		
			float cameraWidth = Mathf.Abs(gameObj.Position.x - levelLeftBound) + Mathf.Max(level.width, level.height);
			GameWorld.Instance._camera.SetCameraWidth(cameraWidth);	
		}	
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

		//First bird must be in the slingshot
		GameWorld.Instance.AddBird(GameWorld.Instance._bird, GameWorld.Instance._slingSelectPos, 
		                           GameWorld.Instance._bird.transform.rotation, "bird0", true);

		if(birdsAmount < 2)
			return;

		Vector3 birdsPos = GameWorld.Instance._slingshot.transform.position;
		birdsPos.y = GameWorld.Instance._ground.collider2D.bounds.center.y + 
							GameWorld.Instance._ground.collider2D.bounds.size.y/2f;
		
		for(int i = 1; i < birdsAmount; i++)
		{
			birdsPos.x -= GameWorld.Instance._bird.GetComponent<SpriteRenderer>().bounds.size.x *2f;
			GameWorld.Instance.AddBird(GameWorld.Instance._bird, birdsPos, GameWorld.Instance._bird.transform.rotation, "bird" + i);
		}
	}
}
