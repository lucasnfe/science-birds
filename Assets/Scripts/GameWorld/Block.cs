using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	enum BlockMaterial {wood, stone, ice };

	private int   _imgChangedTimes;
	private float _receivedDamage;

	private AudioSource      _audioSource;
	private SpriteRenderer   _spriteRenderer;
	private ABParticleSystem _destroyEffect;

	private Sprite[]    _typeSprites;
	private AudioClip[] _typeClips;

	public int   _material;
	public uint  _points;
	public float _life = 10;

	public GameObject[] _woodDestructionEffect;
	public GameObject[] _stoneDestructionEffect;
	public GameObject[] _iceDestructionEffect;

	public Sprite []_woodSprites;
	public Sprite []_stoneSprites;
	public Sprite []_iceSprites;

	public AudioClip []_woodDamageClip;
	public AudioClip []_stoneDamageClip;
	public AudioClip []_iceDamageClip;

	void Awake() {

		_destroyEffect = GetComponent<ABParticleSystem> ();
		_spriteRenderer = GetComponent<SpriteRenderer> ();
		_audioSource = GetComponent<AudioSource> ();

		switch (_material) {

		case (int)BlockMaterial.wood:
			_typeClips = _woodDamageClip;
			_typeSprites = _woodSprites;
			_destroyEffect._particlePrefab = _woodDestructionEffect;
			_life *= 1f;
			break;

		case (int)BlockMaterial.stone:
			_typeClips = _stoneDamageClip;
			_typeSprites = _stoneSprites;
			_destroyEffect._particlePrefab = _stoneDestructionEffect;
			_life *= 2f;
			break;

		case (int)BlockMaterial.ice:
			_typeClips = _iceDamageClip;
			_typeSprites = _iceSprites;
			_destroyEffect._particlePrefab = _iceDestructionEffect;
			_life *= 0.5f;
			break;
		}

		_spriteRenderer.sprite = _typeSprites [0];
	}
	
	void Explode()
	{
		if(!GameWorld.Instance._isSimulation)
		{
			GameWorld.Instance.SpawnPoint(_points, transform.position);

			_destroyEffect._shootParticles = true;
			ABParticleManager.Instance.AddParticleSystem (_destroyEffect, transform.position);
		}

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
			Explode();
		}
	}
}
