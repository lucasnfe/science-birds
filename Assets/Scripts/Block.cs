using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
	
	public int experimentsAmount;
	public float yVelocitySum;
	
	private float _timer;
	
	private int collisionAmount;
	public int GetCollisionAmount()
	{
		return collisionAmount;
	}
	
	// Use this for initialization
	void Start () {
		
		_timer = 1.0f;
		
		yVelocitySum = 0.0f;
		
		experimentsAmount = 0;
		collisionAmount = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		_timer += Time.deltaTime;
	
		if(_timer >= 0.01f)
		{
			experimentsAmount++;	
			yVelocitySum += rigidbody2D.velocity.y;	
			
			_timer = 0.0f;
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
