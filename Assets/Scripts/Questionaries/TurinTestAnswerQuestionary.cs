using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TurinTestAnswerQuestionary : Questionary {

	// Use this for initialization
	void Start () {

		transform.FindChild("GroupName").GetComponent<Text>().text = GameData.Instance.GetGeneratedGroupName();
	}
}
