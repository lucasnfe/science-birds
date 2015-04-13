using UnityEngine;
using System.Collections;

public class LevelManager : LevelSource {

	static int _currentLevel;
	static ABLevel []_levels;

	// Use this for initialization
	public override ABLevel NextLevel() {
	
		Debug.Log (_currentLevel);

		if(_currentLevel == 0)
			_levels = LevelLoader.LoadAllLevels();

		if(_currentLevel > _levels.Length - 1)
			return null;

		ABLevel nextLevel = _levels[_currentLevel];
		_currentLevel++;

		return nextLevel;
	}
}
