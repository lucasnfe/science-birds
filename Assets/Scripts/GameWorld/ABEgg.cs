using UnityEngine;
using System.Collections;

public class ABEgg : ABGameObject {

	public float _explosionArea = 1f;
	public float _explosionPower = 1f;

	public override void OnCollisionEnter2D(Collision2D collision) {

		ABTNT.Explode (transform.position, _explosionArea, _explosionPower);
		Die ();
	}
	
}
