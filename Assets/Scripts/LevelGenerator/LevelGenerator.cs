using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public abstract class LevelGenerator : MonoBehaviour {

	public GameObject []ABTemplates;

	public class ABGameObject
	{
		public int label;
		public Vector2 position;
	}

	public abstract List<ABGameObject> GenerateLevel();

	protected Bounds CalcObjBounds(GameObject composedObj)
	{
		Bounds composedBounds = new Bounds();

		if(composedObj.transform.childCount > 0)
		{
			SpriteRenderer[] allRenderers = composedObj.GetComponentsInChildren<SpriteRenderer>(true);

			foreach (SpriteRenderer rend in allRenderers) {

				composedBounds.Encapsulate(rend.bounds);
			}
		}
		else
		{
			SpriteRenderer objCollider = composedObj.GetComponent<SpriteRenderer>();
			composedBounds = objCollider.bounds;
		}

		return composedBounds;
	}

	void SortABTemplates()
	{
		Array.Sort(ABTemplates, delegate(GameObject x, GameObject y)
		{
			Bounds xBounds = CalcObjBounds(x);
			Bounds yBounds = CalcObjBounds(y);

			if(xBounds.size.x < yBounds.size.x)
				return -1;

			if(xBounds.size.x > yBounds.size.x)
				return 1;

			return 0;
		});
	}

	void Awake()
	{
		List<ABGameObject> gameObjects = GenerateLevel();

		foreach(ABGameObject gameObj in gameObjects)
		{
			GameObject newGameObject = (GameObject)Instantiate(ABTemplates[gameObj.label], gameObj.position, Quaternion.identity);
			newGameObject.transform.parent = transform.FindChild("Level/Blocks");
		}

		//Time.timeScale = 0f;
	}
}
