using UnityEngine;
using System.Collections;

public abstract class LevelSource : MonoBehaviour {

	public static int CurrentLevel { get; set; }

	public virtual int LevelLimit() {

		return 0;
	}

	public virtual ABLevel NextLevel() {

		CurrentLevel = CurrentLevel + 1;
		return null;
	}
}
