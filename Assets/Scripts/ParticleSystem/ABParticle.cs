using UnityEngine;
using System.Collections;

public class ABParticle : PhysicalBody {

	public bool  isAlive  { get; set; }

	private float 		 	   _lifetime;
	private bool 		  	   _isDying;

	private SpriteRenderer 	   _renderer;
	private ABParticleSystem _emitter;

	public void Create (ABParticleSystem emitter) {

		mass = 0.15f;

		_renderer = GetComponent<SpriteRenderer> ();
		_emitter = emitter;
	}
	
	// Update is called once per frame
	protected override void Update () {

		// Apply repellers
		foreach (ABParticleRepeller repeller in ABParticleManager.Instance.GetRepellers())
			ApplyForce(repeller.RepelParticle (this) * Time.deltaTime);

		// Apply attractors
		foreach (ABParticleAttractor attractor in ABParticleManager.Instance.GetAttractors())
			ApplyForce(attractor.AttractParticle (this) * Time.deltaTime);

		base.Update ();

		if (_emitter != null) {

			if (!_renderer.isVisible)
				_emitter.KillParticle (this);

			applyGravity = _emitter._applyGravity;
		}
			
		if(_isDying) {

			Color rendColor = _renderer.color;
			rendColor.a = Mathf.Lerp(rendColor.a, 0f, 10f * Time.deltaTime);
			_renderer.color = rendColor;

			if (_renderer.color.a <= 0f)
				_emitter.KillParticle (this);
		}
	}
		
	public void Shoot(Vector2 velocity, float lifetime, bool applyGravity) {

		_isDying = false;

		this.velocity = velocity;
		this.applyGravity = applyGravity;

		Vector3 noise = Random.onUnitSphere * 0.25f;
		noise.z = 0f;
		transform.position = _emitter.transform.position + noise;

		float rot = Random.Range (0f, 360f);
		transform.rotation = Quaternion.Euler (transform.rotation.x, transform.rotation.y, rot);

		gameObject.SetActive (true);

		Color rendColor = _renderer.color;
		rendColor.a = 1f;
		_renderer.color = rendColor;

		isAlive = true;

		Invoke ("Kill", lifetime);
	}

	public void Kill() {

		_isDying = true; 
	}
}
