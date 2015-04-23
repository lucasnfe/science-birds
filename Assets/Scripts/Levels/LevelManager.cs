using UnityEngine;
using System.Collections;

public class LevelManager : LevelSource {

	static ABLevel []_levels;

	// Use this for initialization
	public override ABLevel NextLevel() {
	
		if(_currentLevel == 0)
			_levels = LevelLoader.LoadAllLevels();

		if(_currentLevel > _levels.Length - 1)
			return null;

		ABLevel nextLevel = _levels[_currentLevel];
		base.NextLevel();

		return nextLevel;
	}
}
