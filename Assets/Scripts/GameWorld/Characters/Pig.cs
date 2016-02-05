using UnityEngine;
using System.Collections;

public class Pig : Character {

	public GameObject dustEffect;

	public override void Die()
	{
		if(!GameWorld.Instance._isSimulation) {

			ABAudioController.Instance.PlayIndependentSFX(_typeClips[0]);
			GameWorld.Instance.SpawnScorePoint(50, transform.position);
			GameWorld.Instance.AddTrajectoryParticle(dustEffect, transform.position, name);
		}

		GameWorld.Instance.KillPig(this);

		base.Die();
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.relativeVelocity.magnitude > _life)
		{
			Invoke("Die", _timeToDie);
			_animator.Play("hurt", 0, 0f);
		}
	}
}
