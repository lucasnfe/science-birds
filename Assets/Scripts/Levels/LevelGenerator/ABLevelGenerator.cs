using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class ABLevelGenerator : LevelSource {
	
	public abstract ABLevel GenerateLevel();

	public override int LevelLimit() {

		unchecked {
			return (int)Mathf.Infinity;
		}
	}

	public override ABLevel NextLevel()
	{
		base.NextLevel();

		return GenerateLevel();
	}
}
