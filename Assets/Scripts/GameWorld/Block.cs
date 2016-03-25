using UnityEngine;
using System.Collections;

/** \class Block
 *  \brief  Contains variables and methods for the structural blocks of the game
 *
 *  Contains information about block sprites, damage received, life, audio and particles.
 *  Also a method for exploding the block and action on collision
 */
public class Block : MonoBehaviour {
	
    /**Controls change of sprites*/
	private int _imgChangedTimes = 0;
    /**Contains the damaged received*/
	private float _receivedDamage = 0f;

    /**Contains the total life of the block*/
	public float _life = 10;
    /**Contains array of sprites for the block*/
	public Sprite []_images;
    /**Contains audio clip to be played when the block takes damage*/
	public AudioClip []_damageClip;
    /**Contains the effect particles to be instanciated and played*/
	public ParticleSystem DestructionEffect;
	
    /**
     *  Instanciates DestructionEffect particle, plays it and destroys it after playing
     */
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
	/**
     *  Implements action on collision
     *  adds damage based on velocity of colliding object
     *  if block is killed, change sprite, if not on simulation plays damage sound,
     *  also change to next sprite and sets received damage to zero
     *  is on last sprite plays block death sound and calls Explode()
     *  @param[in]  collision Collision2D listener
     */
	void OnCollisionEnter2D(Collision2D collision)
	{
		_receivedDamage += collision.relativeVelocity.magnitude;
		if(_receivedDamage >= _life/_images.Length)
		{
			GetComponent<SpriteRenderer>().sprite = _images[_imgChangedTimes];
			
			if(!GameWorld.Instance._isSimulation)
				GetComponent<AudioSource>().PlayOneShot(_damageClip[0]);

			_imgChangedTimes++;
			_receivedDamage = 0;
		}

		if(_imgChangedTimes == _images.Length) {
			ABAudioController.Instance.PlayIndependentSFX(_damageClip[1]);
			Explode();
		}
	}
}
