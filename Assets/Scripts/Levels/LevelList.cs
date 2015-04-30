using UnityEngine;
using System.Collections;

public class LevelList : LevelSource {

	static ABLevel []_levels;
	public TextAsset []_levelsSource;

	public override int LevelLimit() {

		return _levelsSource.Length;
	}

	// Use this for initialization
	public override ABLevel NextLevel() {
	
		if(CurrentLevel == 0) {
			_levels = new ABLevel[_levelsSource.Length];

			for(int i = 0; i < _levelsSource.Length; i++)
				_levels[i] = LevelLoader.LoadXmlLevel(_levelsSource[i].text);
		}

		if(CurrentLevel > _levels.Length - 1)
			return null;

		ABLevel nextLevel = _levels[CurrentLevel];
		base.NextLevel();

		return nextLevel;
	}
}
