using UnityEngine;
using System.Collections;

public class PhysicalBody : MonoBehaviour {

	public float    mass { get; set; }
	public bool     applyGravity { get; set; }

	public Vector2  velocity { get; set; }
	public Vector2  acceleration {get; set;}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	protected virtual void Update () {
	
		// Apply acceleration
		if (applyGravity)
			ApplyForce (Physics2D.gravity * Time.deltaTime);

		// Update velocity using currently acceleration
		velocity += acceleration;

		// Update position using currently velocity
		transform.position += (Vector3)velocity * Time.deltaTime;

		// Reset acceleration
		acceleration = Vector2.zero;

	}

	public void ApplyForce(Vector2 force) {

		force /= mass;
		acceleration += force;
	}
}
