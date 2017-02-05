using UnityEngine;
using System.Collections;

public class ABBirdBlack : ABBird {

	public float _timeToExplode = 1f;
	public float _explosionArea = 1f;
	public float _explosionPower = 1f;

	void SpecialAttack() {

		_animator.Play ("explode");
	}

	// Called via frame event
	void Explode() {

		Collider2D[] colliders = Physics2D.OverlapCircleAll (transform.position, _explosionArea);

		foreach (Collider2D coll in colliders) {

			Vector2 direction = (coll.transform.position - transform.position).normalized;
			if(coll.attachedRigidbody)
				coll.attachedRigidbody.AddForce (direction * _explosionPower, ForceMode2D.Impulse);
		}
			
		_destroyEffect.ShootParticle ();

		Die ();
	}

	public override void OnCollisionEnter2D(Collision2D collision) {
		
		_animator.Play ("explode");
	}
}
