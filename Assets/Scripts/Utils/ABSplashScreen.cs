using UnityEngine;
using System.Collections;

public class ABSplashScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
		ABSceneManager.Instance.LoadScene("Questionary_Introduction");
	}
}
