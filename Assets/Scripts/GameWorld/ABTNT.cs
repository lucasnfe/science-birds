using UnityEngine;
using System.Collections;

public class ABTNT : ABGameObject {

	public float _explosionArea = 1f;
	public float _explosionPower = 1f;

	public override void Die(bool withEffect = true)
	{
		//ScoreHud.Instance.SpawnScorePoint(200, transform.position);
		Explode (transform.position, _explosionArea, _explosionPower);

		base.Die(withEffect);
	}

	public static void Explode(Vector2 position, float explosionArea, float explosionPower) {

		Collider2D[] colliders = Physics2D.OverlapCircleAll (position, explosionArea);

		foreach (Collider2D coll in colliders) {

			Vector2 direction = (Vector2)coll.transform.position - position;
			if (coll.attachedRigidbody) {

				coll.attachedRigidbody.AddForce (direction * explosionPower, ForceMode2D.Impulse);
			}
		}

	}
}
