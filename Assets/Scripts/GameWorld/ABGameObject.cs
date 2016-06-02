using UnityEngine;
using System;
using System.Collections;

[RequireComponent (typeof (Collider2D))]
[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (SpriteRenderer))]
[RequireComponent (typeof (ABParticleSystem))]
[RequireComponent (typeof (AudioSource))]
public abstract class ABGameObject : MonoBehaviour
{	
	protected int   _spriteChangedTimes;
	protected float _receivedDamage;
	protected float _timeToDie = 1f;

	protected Collider2D       _collider;
	protected Rigidbody2D      _rigidBody;
	protected AudioSource      _audioSource;
	protected SpriteRenderer   _spriteRenderer;
	protected ABParticleSystem _destroyEffect;

	public Sprite[]    _sprites;
	public AudioClip[] _clips;

	public float _life = 10f;

	public bool IsDying { get; set; }

	protected virtual void Awake() {

		_collider       = GetComponent<Collider2D> ();
		_rigidBody      = GetComponent<Rigidbody2D> ();
		_destroyEffect  = GetComponent<ABParticleSystem> ();
		_spriteRenderer = GetComponent<SpriteRenderer> ();
		_audioSource    = GetComponent<AudioSource> ();

		IsDying = false;
	}

	protected virtual void Update() {

		DestroyIfOutScreen ();
	}

	public virtual void Die()
	{
		Destroy(gameObject);
	}
		
	public virtual void OnCollisionEnter2D(Collision2D collision)
	{
		_receivedDamage += collision.relativeVelocity.magnitude;
		if(_receivedDamage >= _life/_sprites.Length)
		{
			_spriteChangedTimes = Mathf.Clamp (_spriteChangedTimes, 0, _sprites.Length - 1);
			_spriteRenderer.sprite = _sprites[_spriteChangedTimes];

			if(!ABGameWorld.Instance._isSimulation)
				_audioSource.PlayOneShot(_clips[0]);

			_spriteChangedTimes++;
			_receivedDamage = 0;
		}

		if(_spriteChangedTimes == _sprites.Length) {
			
			ABAudioController.Instance.PlayIndependentSFX(_clips[1]);

			IsDying = true;
			Invoke("Die", _timeToDie);
		}
	}

	void DestroyIfOutScreen() {

		if (ABGameWorld.Instance.IsObjectOutOfWorld (transform, _collider)) {

			IsDying = true;
			Invoke ("Die", _timeToDie);
		}
	}
}
