using UnityEngine;
using System.Collections;

public class Pig : Character {

	public float _forceToDie;
	public GameObject dustEffect;

	public override void Die()
	{
		// Create dust effect
		if(dustEffect)
			Instantiate(dustEffect, transform.position, Quaternion.identity);

		Destroy(gameObject);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.relativeVelocity.magnitude > _forceToDie)
		{
			Invoke("Die", _timeToDie);
			_animator.Play("hurt", 0, 0f);
		}
	}
}
