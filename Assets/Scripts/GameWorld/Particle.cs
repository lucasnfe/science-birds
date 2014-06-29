using UnityEngine;
using System.Collections;

public class Particle : MonoBehaviour {

	private float _particleTimer;
	private Animator _animator;
	
	public bool destroyWhenAnimationFinished;
	public float timeToDestroy = 1.0f;

	// Use this for initialization
	void Start () {
		
		_animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
			
		_particleTimer += Time.deltaTime;
		
		if(_particleTimer >= timeToDestroy)
		{
			Destroy(gameObject);
			_particleTimer = 0.0f;
		}
	}
}
