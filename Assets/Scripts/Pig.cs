using UnityEngine;
using System.Collections;

public class Pig : MonoBehaviour {

	private float timer;
	private float nextBlinkTime;
	
	private Animator animator;

	// Use this for initialization
	void Start () {
	
		nextBlinkTime = Random.Range(0.5f, 4.0f);
		
	    // Get the animator
	    animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
		timer += Time.deltaTime;
		
		if(timer >= nextBlinkTime)
		{	
			animator.SetBool("blinking", true);		
			
			nextBlinkTime = Random.Range(0.5f, 4.0f);
			timer = 0.0f;
		}
	}
}
