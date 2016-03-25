using UnityEngine;
using System.Collections;

/** \class ABSplashScreen
 *  \brief  Loads Questionary Introduction Screen.
 */
public class ABSplashScreen : MonoBehaviour {

	/** 
     *  Loads the Questionary_Introduction scene at initialization
     *  Use this for initialization
     */
	void Start () {
	
		ABSceneManager.Instance.LoadScene("Questionary_Introduction");
	}
}
