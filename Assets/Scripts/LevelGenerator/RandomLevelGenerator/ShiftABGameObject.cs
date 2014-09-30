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

	public int PosShift{ get; set; }
	public bool IsDouble{ get; set; }
	public ShiftABGameObject HoldingObject{ get; set;}

	protected RandomLG _randomLG;

	public ShiftABGameObject()
	{
		_randomLG = GameObject.Find("LevelGenerator").GetComponent<RandomLG>();
	}
}