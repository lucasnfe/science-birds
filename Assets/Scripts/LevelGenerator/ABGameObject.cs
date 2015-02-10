using UnityEngine;
using System.Collections;

public class ABGameObject
{
	private Vector2 _position;
	public virtual Vector2 Position
	{ 
		get{ return _position; }
		set{ _position = value; }
	}
	
	private int _label;
	public virtual int Label
	{ 
		get{ return _label; }
		set{ _label = value; }
	}

	public bool IsPig() 
	{ 
		return (Label == GameWorld.Instance.Templates.Length); 
	}
	
	public float GetArea()
	{
		return GetBounds().size.x * GetBounds().size.y;
	}

	public virtual Bounds GetBounds()
	{
		Bounds composedBounds = new Bounds();

		if(IsPig())
		{
			composedBounds = GameWorld.Instance._pig.renderer.bounds;
		}
		else
		{
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
		}

		composedBounds.center = Position;

		return composedBounds;
	}
}
