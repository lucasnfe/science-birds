using UnityEngine;
using System.Collections;

public class GameWorld : MonoBehaviour {
		
	// Update is called once per frame
	void Update () {

		int pigsAmount = 0;
		Transform blocks = transform.Find("Blocks");

		foreach(Transform block in blocks)
		{
			if(block.GetComponent<Pig>() != null)
				pigsAmount++;
		}

		if(pigsAmount == 0)
		{
			Application.LoadLevel(Application.loadedLevel);
			return;
		}
	}
}
