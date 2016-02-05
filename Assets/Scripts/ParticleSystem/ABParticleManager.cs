using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ABParticleManager : ABSingleton<ABParticleManager> {

	private List<ABParticleRepeller>   _repellers;
	private List<ABParticleAttractor>  _attractors;
	private List<ABParticleSystem> 	   _slugParticleSystems;

	public GameObject _repellersPrefab;
	public GameObject _attractorsPrefab;

	public  List<ABParticleRepeller>  GetRepellers() { 
		return _repellers; 
	}

	public  List<ABParticleAttractor>  GetAttractors() { 
		return _attractors; 
	}

	// Use this for initialization
	void Start () {
	
		_repellers = new List<ABParticleRepeller>();
		_attractors = new List<ABParticleAttractor> ();
		_slugParticleSystems = new List<ABParticleSystem>();
	}

	public void AddParticleSystem(ABParticleSystem clonePart, Vector3 position) {

		GameObject go = new GameObject ();
		go.name = "ABParticleSystem" + _slugParticleSystems.Count;
		go.transform.parent = transform;

		ABParticleSystem particleSys = go.AddComponent<ABParticleSystem> ();
		particleSys.transform.position = position;

		// Randomly assign particle system attributes
		particleSys._particlePoolSize = clonePart._particlePoolSize;
		particleSys._minVel = clonePart._minVel;
		particleSys._maxVel = clonePart._maxVel;
		particleSys._minAngle = clonePart._minAngle;
		particleSys._maxAngle = clonePart._maxAngle;
		particleSys._minMass = clonePart._minMass;
		particleSys._maxMass = clonePart._maxMass;
		particleSys._minLifetime = clonePart._minLifetime;
		particleSys._maxLifetime = clonePart._maxLifetime;

		particleSys._systemLifetime = clonePart._systemLifetime;
		particleSys._shootingRate = clonePart._shootingRate;

		particleSys._shootParticles = clonePart._shootParticles;
		particleSys._applyGravity = clonePart._applyGravity;

		particleSys._particlePrefab = clonePart._particlePrefab;;
		particleSys._repellerPrefab = clonePart._repellerPrefab;
		particleSys._attractorPrefab = clonePart._attractorPrefab;

		_slugParticleSystems.Add (particleSys);
	}
		
	public void AddAttractor() {

		Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		worldPos.z = 1f;

		GameObject go = Instantiate(_attractorsPrefab);
		go.name = "ABAttractor_" + _attractors.Count;
		go.transform.parent = transform;
		go.transform.position = worldPos;

		ABParticleAttractor attrac = go.AddComponent<ABParticleAttractor> ();
		_attractors.Add (attrac);
	}

	public void AddRepeller() {

		Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		worldPos.z = 1f;

		GameObject go = Instantiate(_repellersPrefab);
		go.name = "ABRepeller_" + _repellers.Count;
		go.transform.parent = transform;
		go.transform.position = worldPos;

		ABParticleRepeller rep = go.AddComponent<ABParticleRepeller> ();
		_repellers.Add (rep);
	}
}
