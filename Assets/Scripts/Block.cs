using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
	
	public int experimentsAmount { get; set; }
	
	public float velocity { get; set; }
	public int collisionAmount { get; set; }
	
	private float _timer;
	
	// Use this for initialization
	void Start () {
		
		_timer = 0.1f;
		velocity = 0.0f;
		
		experimentsAmount = 0;
		collisionAmount = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		if(rigidbody2D != null)
		{
			_timer += Time.deltaTime;
	
			if(_timer >= 0.1f)
			{
				experimentsAmount++;	
				velocity += rigidbody2D.velocity.magnitude;
			
				_timer = 0.0f;
			}
		}
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.tag != "Bird" && collision.gameObject.tag != "Catapult")
		{
			collisionAmount++;
		}
	}
}
