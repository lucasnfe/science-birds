using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ABParticleSystem : MonoBehaviour {

	public int        _particlePoolSize;
	public int		  _minVel, _maxVel;
	public int 		  _minAngle, _maxAngle;
	public float	  _minMass, _maxMass;
	public float 	  _minLifetime, _maxLifetime;
	public float 	  _systemLifetime;
	public float 	  _shootingRate;

	public bool 	  _applyGravity;
	public bool 	  _shootParticles;

	public GameObject[] _particlePrefab;
	public GameObject   _repellerPrefab;
	public GameObject   _attractorPrefab;

	private float 	  _shootingTimer;
	private float 	  _selfDestructionTimer;
	private List<ABParticle> _particles;

	// Use this for initialization
	void Start () {
	
		_particles = new List<ABParticle>();

		for (int i = 0; i < _particlePoolSize; i++) {

			// Instantiate the particle and make it disabled
			GameObject randPrefab = _particlePrefab[Random.Range(0, _particlePrefab.Length)];
			GameObject partibleObj = Instantiate(randPrefab); 
			partibleObj.name = "SlugParticle_" + i;
			partibleObj.transform.parent = transform;

			partibleObj.SetActive(false);

			ABParticle part = partibleObj.GetComponent<ABParticle> ();

			if (part == null)
				part = partibleObj.AddComponent<ABParticle> ();

			part.Create (this);

			_particles.Add(part);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (_shootParticles) {

			if (_systemLifetime > 0) {

				_selfDestructionTimer += Time.deltaTime;
				if (_selfDestructionTimer >= _systemLifetime) {

					_shootParticles = false;

					if (GetParticlesAliveAmount () == 0)
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

	public void ShootParticle() {

		ABParticle inactiveParticle = FindDeadParticle ();

		if (inactiveParticle != null) {

			float mass = Random.Range (_minMass, _maxMass);
			float randVel = Random.Range (_minVel, _maxVel);
			float randAng = Random.Range (_minAngle, _maxAngle) * Mathf.Deg2Rad;
			float lifetime = Random.Range (_minLifetime, _maxLifetime);

			Vector2 velocity = new Vector2 (Mathf.Cos(randAng), Mathf.Sin(randAng)) * randVel;

			inactiveParticle.mass = mass;
			inactiveParticle.Shoot (velocity, lifetime, _applyGravity);
		}
	}

	public void KillParticle(ABParticle particle) {

		particle.isAlive = false;
		particle.gameObject.SetActive (false);
	}

	ABParticle FindDeadParticle() {

		for (int i = 0; i < _particlePoolSize; i++) {

			if (!_particles [i].isAlive)
				return _particles [i];
		}

		return null;
	}

	int GetParticlesAliveAmount() {

		int count = 0;

		for (int i = 0; i < _particlePoolSize; i++) {

			if (_particles [i].isAlive)
				count++;
		}

		return count;
	}
}
