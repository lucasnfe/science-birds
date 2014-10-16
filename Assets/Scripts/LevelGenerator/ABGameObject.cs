using UnityEngine;
using System.Collections;

public class ABGameObject
{
	protected LevelGenerator _levelGenerator;

	public Vector2 Position{ get; set; }
	
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
	
	public virtual Bounds GetBounds()
	{
		if(Label == _levelGenerator.ABTemplates.Length)
			return _levelGenerator._pig.renderer.bounds;

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
