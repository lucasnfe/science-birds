using UnityEngine;
using System.Collections;

public class Pig : MonoBehaviour {

	private float _timer;
	private float _dieTimer;

	public float dieTime;
	public GameObject dustEffect;

	private float _nextBlinkTime;
	private bool _isSpawning = true;
	private Animator _animator;

	// Use this for initialization
	void Start () {

		_nextBlinkTime = Random.Range(0.5f, 4.0f);

	    // Get the animator
	    _animator = GetComponent<Animator>();

		_animator.SetBool("hurt", false);

	}

	// Update is called once per frame
	void Update () {

		_timer += Time.deltaTime;

		if(_timer >= _nextBlinkTime)
		{
			_animator.SetBool("blinking", true);

			_nextBlinkTime = Random.Range(0.5f, 4.0f);
			_timer = 0.0f;
		}

		if(_animator.GetBool("hurt"))
		{
			_dieTimer += Time.deltaTime;

			// Time to kill the pig
			if(_dieTimer >= dieTime)
			{
				// Create dust effect
				if(dustEffect)
					Instantiate(dustEffect, transform.position, Quaternion.identity);

				Destroy(gameObject);

				_dieTimer = 0.0f;
			}
		}
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.transform.tag == "Bird")
		{
			_animator.SetBool("hurt", true);
			return;
		}

		if(_isSpawning)
		{
			_isSpawning = false;
		}
		else if(collision.relativeVelocity.magnitude > 1.5f)
		{
			_animator.SetBool("hurt", true);
		}

	}
}
