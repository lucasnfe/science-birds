using UnityEngine;
using System.Collections;

public class ABPig : ABCharacter {

	public GameObject dustEffect;

	public override void Die()
	{
		if(!ABGameWorld.Instance._isSimulation) {

			ABAudioController.Instance.PlayIndependentSFX(_clips[0]);
			ABGameWorld.Instance.SpawnScorePoint(50, transform.position);
			ABGameWorld.Instance.AddTrajectoryParticle(dustEffect, transform.position, name);
		}

		ABGameWorld.Instance.KillPig(this);

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
