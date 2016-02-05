using UnityEngine;
using System.Collections;

public class LevelList : ABSingleton<LevelList> {

	private ABLevel[]   _levels;

	public int CurrentIndex;

	public ABLevel GetCurrentLevel() { 

		if (_levels == null)
			return null;

		if(CurrentIndex > _levels.Length - 1)
			return null;

		return _levels [CurrentIndex]; 
	}

	public void LoadLevelsFromSource(TextAsset[] levelSource, bool shuffle = false) {

		CurrentIndex = 0;

		_levels = new ABLevel[levelSource.Length];

		if(shuffle)
			ABArrayUtils.Shuffle(levelSource);

		for(int i = 0; i < levelSource.Length; i++)
			_levels[i] = LevelLoader.LoadXmlLevel(levelSource[i].text);
	}

	// Use this for initialization
	public ABLevel NextLevel() {

		if(CurrentIndex == _levels.Length - 1)
			return null;

		ABLevel level = _levels [CurrentIndex];
		CurrentIndex++;

		return level;
	}
}
