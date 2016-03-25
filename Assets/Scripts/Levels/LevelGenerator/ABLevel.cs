using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/** \class ABLevel
 *  \brief  Angry Birds Level class
 *
 *  Contains the level of angry birds game. With all its objects.
 */
public class ABLevel 
{
    /**Total of birds on level*/
	public int birdsAmount;
    /**List of game objects, containing pigs and scenario objects*/
	public List<ABGameObject> gameObjects;
    /**Max amount of birds allowed on one level*/
	public static int BIRDS_MAX_AMOUNT = 5;
    /**Constructor which initializes a list of ABGameObject*/
	public ABLevel()
	{
		gameObjects = new List<ABGameObject>();
	}
    /**
     *  Gets birds frequency, dividing birds amount by BIRDS_MAX_AMOUNT
     *  @return float   Birds frequency on this level.
     */
	public float GetBirdsFrequency()
	{
		return (float)(birdsAmount + 1)/BIRDS_MAX_AMOUNT;
	}
	/**
     *  Gets the frequency of a game object with certain index
     *  Total of objects with this index divided by total of objects
     *  @param[in]  gameObjectIndex Integer containing the desired object's index
     *  @return float   frequency of desired object on total of objects
     */
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