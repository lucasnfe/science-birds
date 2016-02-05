using UnityEngine;
using System.Collections;

public class ABParticleRepeller : PhysicalBody {

	float strengh = 1000f;

	// Use this for initialization
	void Start () {
		
		applyGravity = false;
	}

	public Vector2 RepelParticle(ABParticle particle) {

		Vector2 direction = transform.position - particle.transform.position;

		float distance = direction.magnitude;
		distance = Mathf.Clamp(distance, 1f, 100f);
	
		float force = -1f * strengh / (distance * distance);
		direction = direction.normalized * force;

		return direction;
	}
}
