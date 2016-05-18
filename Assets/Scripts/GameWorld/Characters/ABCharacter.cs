using UnityEngine;
using System.Collections;

public class ABCharacter : ABGameObject {

	public Animator _animator;
	public float _maxTimeToBlink;

	// Use this for initialization
	protected override void Awake () {

		base.Awake ();

		_animator = GetComponent<Animator>();
	
		float nextBlinkDelay = Random.Range(0.0f, _maxTimeToBlink);
		Invoke("Blink", nextBlinkDelay + 1.0f);
	}

	public bool IsIdle() {
		
		return _animator.GetCurrentAnimatorStateInfo(0).IsName("idle");
	}

	void Blink() {
		
		if(IsIdle())
			_animator.Play("blink", 0, 0f);
		
		float nextBlinkDelay = Random.Range(0.0f, _maxTimeToBlink);
		Invoke("Blink", nextBlinkDelay + 1.0f);
	}
}
