using UnityEngine;
using System.Collections;

public class ABBirdBlack : ABBird {

	public float _explosionArea = 1f;
	public float _explosionPower = 1f;

	void SpecialAttack() {

		Explode ();
	}

	// Called via frame event
	void Explode() {

		ABTNT.Explode (transform.position, _explosionArea, _explosionPower);
		Die ();
	}

	public override void OnCollisionEnter2D(Collision2D collision) {

		if(OutOfSlingShot)
			_animator.Play ("explode");
	}
}
