using UnityEngine;
using System.Collections;

public class ABLevelSelector : MonoBehaviour {

	public int LevelIndex;

	public void UpdateLevelList() {

		LevelList.Instance.CurrentIndex = LevelIndex;	
	}
}
