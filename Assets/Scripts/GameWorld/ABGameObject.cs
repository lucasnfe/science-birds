using UnityEngine;
using System;
using System.Collections;

[RequireComponent (typeof (Collider2D))]
[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (AudioSource))]
public abstract class ABGameObject : MonoBehaviour
{	
	protected int   _imgChangedTimes;
	protected float _receivedDamage;

	public Sprite[]      _typeSprites;
	public AudioClip[]   _typeClips;

	protected AudioSource      _audioSource;
	protected SpriteRenderer   _spriteRenderer;
	protected ABParticleSystem _destroyEffect;
	protected Collider2D       _collider;
	protected Rigidbody2D      _rigidBody;

	public float _timeToDie = 1;
	public float _life = 10;

	protected virtual void Awake() {

		_collider       = GetComponent<Collider2D> ();
		_rigidBody      = GetComponent<Rigidbody2D> ();
		_destroyEffect  = GetComponent<ABParticleSystem> ();
		_spriteRenderer = GetComponent<SpriteRenderer> ();
		_audioSource    = GetComponent<AudioSource> ();
	}

	protected virtual void Update() {

		DestroyIfOutScreen ();
	}

	public virtual void Die()
	{
		Destroy(gameObject);
	}
		
	void OnCollisionEnter2D(Collision2D collision)
	{
		_receivedDamage += collision.relativeVelocity.magnitude;
		if(_receivedDamage >= _life/_typeSprites.Length)
		{
			_spriteRenderer.sprite = _typeSprites[_imgChangedTimes];

			if(!GameWorld.Instance._isSimulation)
				_audioSource.PlayOneShot(_typeClips[0]);

			_imgChangedTimes++;
			_receivedDamage = 0;
		}

		if(_imgChangedTimes == _typeSprites.Length) {
			
			ABAudioController.Instance.PlayIndependentSFX(_typeClips[1]);
			Die();
		}
	}

	void DestroyIfOutScreen() {

		if(GameWorld.Instance.IsObjectOutOfWorld(transform, _collider))
			Die ();
	}
}
