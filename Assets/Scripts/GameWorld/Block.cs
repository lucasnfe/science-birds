using UnityEngine;
using System.Collections;

public class Block : ABGameObject {

	public MATERIALS _material;

	public uint  _points;

	public GameObject[] _woodDestructionEffect;
	public GameObject[] _stoneDestructionEffect;
	public GameObject[] _iceDestructionEffect;

	public Sprite []_woodSprites;
	public Sprite []_stoneSprites;
	public Sprite []_iceSprites;

	public AudioClip []_woodDamageClip;
	public AudioClip []_stoneDamageClip;
	public AudioClip []_iceDamageClip;

	private PhysicsMaterial2D _woodMaterial;
	private PhysicsMaterial2D _stoneMaterial;
	private PhysicsMaterial2D _iceMaterial;

	protected override void Awake() {

		base.Awake();

		_woodMaterial = Resources.Load ("Materials/Wood") as PhysicsMaterial2D;
		_stoneMaterial = Resources.Load ("Materials/Stone") as PhysicsMaterial2D;
		_iceMaterial = Resources.Load ("Materials/Ice") as PhysicsMaterial2D;

		SetMaterial (_material);
	}
	
	public override void Die()
	{
		if(!GameWorld.Instance._isSimulation)
		{
			GameWorld.Instance.SpawnScorePoint(_points, transform.position);

			_destroyEffect._shootParticles = true;
			ABParticleManager.Instance.AddParticleSystem (_destroyEffect, transform.position);
		}

		base.Die();
	}

	public void SetMaterial(MATERIALS material) {

		_material = material;

		switch (material) {

		case MATERIALS.wood:
			_typeClips = _woodDamageClip;
			_typeSprites = _woodSprites;
			_destroyEffect._particlePrefab = _woodDestructionEffect;
			_collider.sharedMaterial = _woodMaterial;
			_life *= 1f;
			break;

		case MATERIALS.stone:
			_typeClips = _stoneDamageClip;
			_typeSprites = _stoneSprites;
			_destroyEffect._particlePrefab = _stoneDestructionEffect;
			_collider.sharedMaterial = _stoneMaterial;

			_life *= 2f;
			break;

		case MATERIALS.ice:
			_typeClips = _iceDamageClip;
			_typeSprites = _iceSprites;
			_destroyEffect._particlePrefab = _iceDestructionEffect;
			_collider.sharedMaterial = _iceMaterial;

			_life *= 0.5f;
			break;
		}

		_spriteRenderer.sprite = _typeSprites [0];
	}
}
