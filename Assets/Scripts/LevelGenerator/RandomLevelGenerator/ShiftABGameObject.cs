using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShiftABGameObject : ABGameObject
{
	protected RandomLG _randomLG;
	public ShiftABGameObject()
	{
		_randomLG = GameObject.Find("LevelGenerator").GetComponent<RandomLG>();
	}
	
	public bool IsDouble{ get; set; }

	private int _type;
	public int Type 
	{ 
		get{ return _type; } 
	}

	private float _underObjectsHeight;
	public float UnderObjectsHeight 
	{ 
		get{ return _underObjectsHeight; } 
		set{ _underObjectsHeight = value; }
	}
	
	public override int Label
	{
		get { return base.Label; }
		set
		{ 
			base.Label = value;
			
			if(value < GameWorld.Instance.Templates.Length)
				_type = _randomLG.GetTypeByTag(GameWorld.Instance.Templates[value].tag); 
			else if(value == GameWorld.Instance.Templates.Length)
				_type = _randomLG.GetTypeByTag(GameWorld.Instance._pig.tag);
			else	
				 throw new System.InvalidOperationException("Invalid label.");
		}
	}
	
	public Vector2 GetEmptyScapeInside()
	{		
		if(Label < GameWorld.Instance.Templates.Length)
		{
			BoxCollider2D emptyScape = GameWorld.Instance.Templates[Label].GetComponent<BoxCollider2D>();
			
			if(emptyScape != null && emptyScape.isTrigger)
				return emptyScape.size;
		}
		
		return Vector2.zero;
	}

	public override Bounds GetBounds()
	{
		Bounds baseBounds = base.GetBounds();
		
		if(IsDouble)
		{
			Vector2 doubleSize = baseBounds.size;
			doubleSize.x *= 2f;
			baseBounds.size = doubleSize;
		}
		
		return baseBounds;
	}

	public void AddObjectInside(ShiftABGameObject obj)
	{
		_underObjectsHeight += obj.GetBounds().size.y;
	}
	
	public override int GetHashCode()
	{
	    const int prime = 17;				
		
		int hash = 1;
		
		hash = hash * prime + Label;
		hash = hash * prime + Type;
		hash = hash * prime + (IsDouble ? 1 : 0);
	
		return hash;
	}
	
    public bool Equals(ShiftABGameObject otherGen)
   	{		
        // if (ReferenceEquals(null, otherGen)) return false;
        // if (ReferenceEquals(this, otherGen)) return true;

		return (Label == otherGen.Label && IsDouble == otherGen.IsDouble && Type == otherGen.Type);
   }
 
	public override bool Equals(object obj)
	{
	    // Since our other Equals() method already compares guys, we'll just call it.
	    // if (!(obj is ShiftABGameObject)) return false;
	    return Equals((ShiftABGameObject) obj);
	}
}