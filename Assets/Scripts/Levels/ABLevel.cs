using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct OBjData {

	public string type;
	public float x, y;
}

public class ABLevel 
{
	public int birdsAmount;

	public List<OBjData> pigs;
	public List<OBjData> blocks;
	public List<OBjData> platforms;

	public static readonly int BIRDS_MAX_AMOUNT = 5;

	public ABLevel() {
		
		pigs = new List<OBjData>();
		blocks = new List<OBjData>();
	}
}