using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
	
	public int experimentsAmount;
	public float yVelocitySum;
	
	private float _timer;
	
	// Use this for initialization
	void Start () {
		
		_timer = 1.0f;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		_timer += Time.deltaTime;
	
		if(_timer >= 1.0f)
		{
			experimentsAmount++;	
			yVelocitySum += rigidbody2D.velocity.y;	
			
			_timer = 0.0f;
		}
	}
}
