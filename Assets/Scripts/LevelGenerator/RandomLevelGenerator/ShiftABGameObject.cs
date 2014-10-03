using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShiftABGameObject : ABGameObject
{
	LinkedList<ShiftABGameObject> _insideObjects;

	public ShiftABGameObject HoldingObject{ get; set;}

	public float PosShift{ get; set; }
	public bool IsDouble{ get; set; }

	protected RandomLG _randomLG;
	public ShiftABGameObject()
	{
		_randomLG = GameObject.Find("LevelGenerator").GetComponent<RandomLG>();
		_insideObjects = new LinkedList<ShiftABGameObject>();
	}

	private int _type;
	public int Type 
	{ 
		get{ return _type; } 
	}

	private float _underObjectsHeight;
	public float UnderObjectsHeight 
	{ 
		get{ return _underObjectsHeight; } 
	}
	
	public override int Label
	{
		get { return base.Label; }
		set
		{ 
			base.Label = value;
			if(value < _levelGenerator.ABTemplates.Length)
				_type = _randomLG.GetTypeByTag(_levelGenerator.ABTemplates[value].tag); 
		}
	}
	
	public virtual Bounds GetBounds()
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
		_insideObjects.AddLast(obj);
		_underObjectsHeight += obj.GetBounds().size.y;
	}

	public ShiftABGameObject LastObjectInside()
	{
		if(_insideObjects.Count == 0)
			return null;

		return _insideObjects.Last.Value;
	}
}