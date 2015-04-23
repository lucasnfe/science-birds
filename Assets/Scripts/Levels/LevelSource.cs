using UnityEngine;
using System.Collections;

public abstract class LevelSource : MonoBehaviour {

	protected static int _currentLevel;
	public static int CurrentLevel {
		get{ return _currentLevel; }
	}

	public virtual ABLevel NextLevel() {

		_currentLevel++;
		return null;
	}
}
