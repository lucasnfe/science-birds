using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ABParticleSystem : MonoBehaviour {

	public int		  _minVel, _maxVel;
	public int 		  _minAngle, _maxAngle;
	public float	  _minMass, _maxMass;
	public float 	  _minLifetime, _maxLifetime;
	public float 	  _systemLifetime;
	public float 	  _shootingRate;

	public bool 	  _addNoise;
	public bool 	  _applyGravity;
	public bool 	  _shootParticles;

	public GameObject[] _particleSprites;

	private float 	  _shootingTimer;
	private float 	  _selfDestructionTimer;

	private ABObjectPool<ABParticle> _particles;

	private void Start () {
	
		_particles = new ABObjectPool<ABParticle>(25, _particleSprites, InitParticle);
	}
	
	private void Update () {

		if (_shootParticles) {

			if (_systemLifetime > 0) {

				_selfDestructionTimer += Time.deltaTime;
				if (_selfDestructionTimer >= _systemLifetime) {

					_shootParticles = false;

					if (_particles.GetUsedObjects ().Count == 0)
						Destroy (this);
				}
			}

			_shootingTimer += Time.deltaTime;

			if (_shootingTimer >= _shootingRate) {

				ShootParticle ();
				_shootingTimer = 0f;
			}
		}
	}

	private void InitParticle(ABParticle particle) {

		particle.Create (this);
	}

	public ABParticle ShootParticle() {

		ABParticle inactiveParticle = _particles.GetFreeObject();

		if (inactiveParticle != null) {

			float mass = Random.Range (_minMass, _maxMass);
			float randVel = Random.Range (_minVel, _maxVel);
			float randAng = Random.Range (_minAngle, _maxAngle) * Mathf.Deg2Rad;
			float lifetime = Random.Range (_minLifetime, _maxLifetime);

			Vector2 velocity = new Vector2 (Mathf.Cos(randAng), Mathf.Sin(randAng)) * randVel;

			inactiveParticle.mass = mass;
			inactiveParticle.Shoot (velocity, lifetime, _applyGravity, _addNoise);
		}

		return inactiveParticle;
	}

	public void SetParciclesParent(Transform parent) {

		_particles.SetObjectsParent (parent);
	}

	public List<ABParticle> GetUsedParticles() {

		return _particles.GetUsedObjects ();
	}

	public void KillAllParticles() {

		_particles.FreeAllObjects ();
	}

	public void KillParticle(ABParticle particle) {

		_particles.SetFreeObject (particle.gameObject);
	}
}
