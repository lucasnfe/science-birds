using UnityEngine;
using System.Collections;

public class ABTNT : ABGameObject {

	public float _explosionArea = 1f;
	public float _explosionPower = 1f;

	public override void Die(bool withEffect = true)
	{
		ScoreHud.Instance.SpawnScorePoint(200, transform.position);
		Explode ();

		base.Die(withEffect);
	}

	void Explode() {

		Collider2D[] colliders = Physics2D.OverlapCircleAll (transform.position, _explosionArea);

		foreach (Collider2D coll in colliders) {

			Vector2 direction = (coll.transform.position - transform.position).normalized;
			if(coll.attachedRigidbody)
				coll.attachedRigidbody.AddForce (direction * _explosionPower, ForceMode2D.Impulse);
		}
	}
}
