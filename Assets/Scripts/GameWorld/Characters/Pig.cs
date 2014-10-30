using UnityEngine;
using System.Collections;

public class Pig : Character {

	public float _forceToDie;
	public GameObject dustEffect;

	public override void Die()
	{
		Camera.main.audio.PlayOneShot(_clips[0], 1f);

		_gameWorld.AddTrajectoryParticle(dustEffect, transform.position, name);
		_gameWorld.KillPig(this);

		base.Die();
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
