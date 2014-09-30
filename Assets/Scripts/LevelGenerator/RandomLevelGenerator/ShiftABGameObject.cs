using UnityEngine;
using System.Collections;

public class ShiftABGameObject : ABGameObject
{
	private int _type;
	public int Type 
	{ 
		get{ return _type; } 
	}
	
	public override int Label
	{
		get{ return base.Label; }
		set{ base.Label = value;
			 _type = _randomLG.GetTypeByTag(_levelGenerator.ABTemplates[value].tag); }
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

	public int PosShift{ get; set; }
	public bool IsDouble{ get; set; }
	public ShiftABGameObject HoldingObject{ get; set;}

	protected RandomLG _randomLG;

	public ShiftABGameObject()
	{
		_randomLG = GameObject.Find("LevelGenerator").GetComponent<RandomLG>();
	}
}