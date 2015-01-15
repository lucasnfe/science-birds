using UnityEngine;
using System.Collections;

public class ABGameObject
{
	public Vector2 Position{ get; set; }
	
	private int _label;
	public virtual int Label
	{ 
		get{ return _label; }
		set{ _label = value; }
	}
	
	public virtual Bounds GetBounds()
	{
		if(Label == GameWorld.Instance.Templates.Length)
			return GameWorld.Instance._pig.renderer.bounds;

		Bounds composedBounds = new Bounds();
		GameObject composedObj = GameWorld.Instance.Templates[Label];
		
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
