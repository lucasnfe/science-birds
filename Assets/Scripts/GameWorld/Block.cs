using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {

	enum BlockMaterial {wood, stone, ice };

	private int   _imgChangedTimes = 0;
	private float _receivedDamage  = 0f;

	private Sprite[] _typeSprites;

	public int   _material = 0;
	public float _life = 10;

	public Sprite []_woodSprites;
	public Sprite []_stoneSprites;
	public Sprite []_iceSprites;

	public AudioClip []_damageClip;

	public ParticleSystem DestructionEffect;

	void Awake() {

		switch (_material) {

		case (int)BlockMaterial.wood:
			_typeSprites = _woodSprites;
			_life *= 1f;
			break;

		case (int)BlockMaterial.stone:
			_typeSprites = _stoneSprites;
			_life *= 2f;
			break;

		case (int)BlockMaterial.ice:
			_typeSprites = _iceSprites;
			_life *= 0.5f;
			break;
		}
	}
	
	void Explode()
	{
		if(!GameWorld.Instance._isSimulation)
		{
			GameWorld.Instance.SpawnPoint(25, transform.position);

			//Instantiate our one-off particle system
			ParticleSystem explosionEffect = Instantiate(DestructionEffect) as ParticleSystem;
			explosionEffect.transform.position = transform.position;
			explosionEffect.transform.parent = GameWorld.Instance.transform.FindChild("Effects");
			
			//play it
			explosionEffect.loop = false;
			explosionEffect.Play();
			
			Destroy(explosionEffect.gameObject, 2f);
		}

		Destroy(gameObject);
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		_receivedDamage += collision.relativeVelocity.magnitude;
		if(_receivedDamage >= _life/_typeSprites.Length)
		{
			GetComponent<SpriteRenderer>().sprite = _typeSprites[_imgChangedTimes];
			
			if(!GameWorld.Instance._isSimulation)
				GetComponent<AudioSource>().PlayOneShot(_damageClip[0]);

			_imgChangedTimes++;
			_receivedDamage = 0;
		}

		if(_imgChangedTimes == _typeSprites.Length) {
			ABAudioController.Instance.PlayIndependentSFX(_damageClip[1]);
			Explode();
		}
	}
}
