using UnityEngine;
using System.Collections;

public class Pig : Character {

	public float _forceToDie;
	public GameObject dustEffect;

	public override void Die()
	{
		if(!GameWorld.Instance._isSimulation) {

			GameWorld.Instance.SpawnPoint(50, transform.position);
			ABAudioController.Instance.PlayMusic(_clips[0]);
			GameWorld.Instance.AddTrajectoryParticle(dustEffect, transform.position, name);
		}

		GameWorld.Instance.KillPig(this);

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
