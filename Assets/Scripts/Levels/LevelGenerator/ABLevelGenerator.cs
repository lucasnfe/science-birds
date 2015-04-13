using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class ABLevelGenerator : LevelSource {
	
	public abstract ABLevel GenerateLevel();

	public override ABLevel NextLevel()
	{
		return GenerateLevel();
	}
}
