using UnityEngine;
using System.Collections;

public class ABGameObject
{
	public Vector2 Position{ get; set; }
	protected LevelGenerator _levelGenerator;

	private int _label;
	public virtual int Label
	{ 
		get{ return _label; }
		set{ _label = value; }
	}
		
	public ABGameObject()
	{
		_levelGenerator = GameObject.Find("LevelGenerator").GetComponent<LevelGenerator>();
	}
	
	public Bounds GetBounds()
	{
		Bounds composedBounds = new Bounds();
		GameObject composedObj = _levelGenerator.ABTemplates[Label];
		
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
}
