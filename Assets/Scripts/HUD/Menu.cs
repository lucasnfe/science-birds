using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public void LoadLevel(string sceneName)
	{
		SceneManager.Instance.LoadScene(sceneName);
	}
}
