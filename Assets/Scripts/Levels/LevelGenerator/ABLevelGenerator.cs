using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class ABLevelGenerator : LevelSource {
	
	public abstract ABLevel GenerateLevel();

	public override int LevelLimit() {

		unchecked {
			return (int)Mathf.Infinity;
		}
	}

	public override ABLevel NextLevel()
	{
		base.NextLevel();

		return GenerateLevel();
	}

	public static ABLevel GameObjectsToABLevel(GameObject []gameObjs)
	{
		ABLevel level = new ABLevel();

		for(int i = 0; i < gameObjs.Length; i++)
		{
			if(gameObjs[i].tag == "Bird")
			{
				level.birdsAmount++;
			}
			else
			{
				ABGameObject abGameObj = new ABGameObject();
				abGameObj.Position = gameObjs[i].transform.position;
				abGameObj.Label = GameWorld.Instance.GetTemplateIndex(gameObjs[i]);
				level.gameObjects.Add(abGameObj);
			}
		}

		return level;
	}
}
