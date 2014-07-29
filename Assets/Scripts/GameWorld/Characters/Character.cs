using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	protected Animator _animator;

	public float _timeToDie;
	public float _maxTimeToBlink;

	public AudioClip[] _clips;

	// Use this for initialization
	public virtual void Start () {

		_animator = GetComponent<Animator>();
	
		float nextBlinkDelay = Random.Range(0.0f, _maxTimeToBlink);
		Invoke("Blink", nextBlinkDelay + 1.0f);
	}

	public virtual void Die()
	{
		Destroy(gameObject);
	}
	
	protected void PlayAudio(int audioIndex)
	{
		if(_clips.Length > audioIndex)
			audio.PlayOneShot(_clips[audioIndex], 1.0f);
	}

	public bool IsIdle()
	{
		return _animator.GetCurrentAnimatorStateInfo(0).IsName("idle");
	}

	void Blink()
	{
		if(IsIdle())
			_animator.Play("blink", 0, 0f);
		
		float nextBlinkDelay = Random.Range(0.0f, _maxTimeToBlink);
		Invoke("Blink", nextBlinkDelay + 1.0f);
	}
}
