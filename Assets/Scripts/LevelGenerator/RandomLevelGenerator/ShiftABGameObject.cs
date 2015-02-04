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
	
	public bool  IsDouble{ get; set; }

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
		}
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
}