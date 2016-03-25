using UnityEngine;
using System.Collections;

/** \class Character
 *  \brief  Contains information about the game characters
 *
 *  Contains character animator, time to die counter, max time to blink, audio clips, 
 *  methods for: initialization, death, audio playing, blinking and getter to check if is idle
 */
public class Character : MonoBehaviour {

    /**The animator for the character*/
	protected Animator _animator;
    /**time to wait before calling Die method*/
	public float _timeToDie;
    /**Max delay until next blink*/
	public float _maxTimeToBlink;
    /**Audio clips that this character have*/
	public AudioClip[] _clips;
	
	/** 
     *  At initialization gets the animator component, randomly chooses a blink delay based on _maxTimeToBlink
     *  And Invokes Blink() after this delay + 1s
     */
	public virtual void Awake () {

		_animator = GetComponent<Animator>();
	
		float nextBlinkDelay = Random.Range(0.0f, _maxTimeToBlink);
		Invoke("Blink", nextBlinkDelay + 1.0f);
	}
    /**
     *  Destroys this game object
     */
	public virtual void Die()
	{
		Destroy(gameObject);
	}
	/**
     *  Plays the requested audio once
     *  @param[in]  audioIndex index of audio clip to be played
     */
	protected void PlayAudio(int audioIndex)
	{
		GetComponent<AudioSource>().PlayOneShot(_clips[audioIndex]);
	}
    /**
     *  Check if character is in idle state of animation
     *  @return bool    true if is at idle state, false otherwise 
     */
	public bool IsIdle()
	{
		return _animator.GetCurrentAnimatorStateInfo(0).IsName("idle");
	}
    /**
     *  If is in idle state, blink. Calculates nextBlinkDelay and Invoke() Blink() again after
     *  the generated delay + 1s
     */
	void Blink()
	{
		if(IsIdle())
			_animator.Play("blink", 0, 0f);
		
		float nextBlinkDelay = Random.Range(0.0f, _maxTimeToBlink);
		Invoke("Blink", nextBlinkDelay + 1.0f);
	}
}
