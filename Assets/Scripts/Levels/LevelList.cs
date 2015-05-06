using UnityEngine;
using System.Collections;

public class LevelList : LevelSource {

	static ABLevel []_levels;
	public int _listLimit;
	public bool _shuffle;
	public TextAsset []_levelsSource;

	public override int LevelLimit() {

		return _listLimit;
	}

	// Use this for initialization
	public override ABLevel NextLevel() {
	
		if(CurrentLevel == 0) {
			_levels = new ABLevel[_levelsSource.Length];

			if(_shuffle)
				ABArrayUtils.Shuffle(_levelsSource);

			for(int i = 0; i < _levelsSource.Length; i++)
				_levels[i] = LevelLoader.LoadXmlLevel(_levelsSource[i].text);
		}

		if(CurrentLevel > _listLimit - 1)
			return null;

		ABLevel nextLevel = _levels[CurrentLevel];
		base.NextLevel();

		return nextLevel;
	}
}
