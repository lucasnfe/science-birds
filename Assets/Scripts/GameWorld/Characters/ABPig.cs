using UnityEngine;
using System.Collections;

public class ABPig : ABCharacter {

	public GameObject dustEffect;

	public override void Die()
	{
		if(!ABGameWorld.Instance._isSimulation) {

			ABAudioController.Instance.PlayIndependentSFX(_clips[0]);
			_destroyEffect.ShootParticle ();

			ScoreHud.Instance.SpawnScorePoint(50, transform.position);
		}

		ABGameWorld.Instance.KillPig(this);

		base.Die();
	}
}
