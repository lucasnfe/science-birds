using UnityEngine;
using System;
using System.Collections;

[RequireComponent (typeof (Collider2D))]
[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (AudioSource))]
public abstract class ABGameObject : MonoBehaviour
{	
	protected int   _spriteChangedTimes;
	protected float _receivedDamage;

	public Sprite[]    _sprites;
	public AudioClip[] _clips;

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
		if(_receivedDamage >= _life/_sprites.Length)
		{
			_spriteRenderer.sprite = _sprites[_spriteChangedTimes];

			if(!ABGameWorld.Instance._isSimulation)
				_audioSource.PlayOneShot(_clips[0]);

			_spriteChangedTimes++;
			_receivedDamage = 0;
		}

		if(_spriteChangedTimes == _sprites.Length) {
			
			ABAudioController.Instance.PlayIndependentSFX(_clips[1]);
			Die();
		}
	}

	void DestroyIfOutScreen() {

		if(ABGameWorld.Instance.IsObjectOutOfWorld(transform, _collider))
			Die ();
	}
}
