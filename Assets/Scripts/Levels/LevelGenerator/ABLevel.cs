using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ABLevel 
{
	public int birdsAmount;
	public List<ABGameObject> gameObjects;

	public static int BIRDS_MAX_AMOUNT = 5;

	public ABLevel()
	{
		gameObjects = new List<ABGameObject>();
	}

	public float GetBirdsFrequency()
	{
		return (float)(birdsAmount + 1)/BIRDS_MAX_AMOUNT;
	}
	
	public float GetABGameObjectFrequency(int gameObjectIndex)
	{
		int gameObjectAmount = 0;
		
		foreach(ABGameObject gameObj in gameObjects)
		{
			if(gameObj.Label == gameObjectIndex)
				gameObjectAmount++;
		}
		
		return (float)gameObjectAmount/gameObjects.Count;
	}
}