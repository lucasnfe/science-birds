using UnityEngine;
using System.Collections;

public class ABParticleAttractor : PhysicalBody {

	float strengh = 200f;

	// Use this for initialization
	void Start () {
	
		mass = 10f;
	
		applyGravity = false;
	}

	public Vector2 AttractParticle(ABParticle particle) {

		Vector2 force = transform.position - particle.transform.position;

		float distance = force.magnitude;
		distance = Mathf.Clamp(distance, 1f, 100f);


		float strenght = (strengh * mass * particle.mass)/ (distance * distance);
		force = force.normalized * strenght;

		return force;
	}
}
