using UnityEngine;
using System.Collections;

public class SharedContent : MonoBehaviour {

	private int _lastLevel;

	void Awake() {

		DontDestroyOnLoad(transform.gameObject);
	}

	void OnLevelWasLoaded(int level)
	{
		if(_lastLevel == 1)
		{
			// Delete content from main menu
			GameObject sharedContent = GameObject.Find ("SharedContent");
			Destroy(sharedContent);
		}

		_lastLevel = level;
	}
}
