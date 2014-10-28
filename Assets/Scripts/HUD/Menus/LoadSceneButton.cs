using UnityEngine;
using System.Collections;

public class LoadSceneButton : HUDButton {

	public string _sceneName;

	// Use this for initialization
	public override void ButtonTouchUp() {

		base.ButtonTouchUp();
		Application.LoadLevel(_sceneName);
	}
}
