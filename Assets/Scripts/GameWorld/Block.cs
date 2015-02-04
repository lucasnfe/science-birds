using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
	
	private int _imgChangedTimes = 0;
	private float _receivedDamage = 0f;

	public float _life = 10;
	public Sprite []_images;
	public AudioClip []_damageClip;
	public ParticleSystem DestructionEffect;
	
	void Explode()
	{
		//Instantiate our one-off particle system
		ParticleSystem explosionEffect = Instantiate(DestructionEffect) as ParticleSystem;
		explosionEffect.transform.position = transform.position;
		explosionEffect.transform.parent = GameWorld.Instance.transform.FindChild("Effects");
		
		//play it
		explosionEffect.loop = false;
		explosionEffect.Play();

		Destroy(gameObject);
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		_receivedDamage += collision.relativeVelocity.magnitude;
		if(_receivedDamage >= _life/_images.Length)
		{
			GetComponent<SpriteRenderer>().sprite = _images[_imgChangedTimes];
			Camera.main.audio.PlayOneShot(_damageClip[0], 1.0f);

			_imgChangedTimes++;
			_receivedDamage = 0;
		}

		if(_imgChangedTimes == _images.Length)
		{
			Camera.main.audio.PlayOneShot(_damageClip[1], 1.0f);
			Explode();
		}
	}
}
