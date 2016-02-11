using UnityEngine;
using System.Collections;

public class ABBlock : ABGameObject {

	public MATERIALS _material;

	public uint  _points;

	public Sprite []_woodSprites;
	public Sprite []_stoneSprites;
	public Sprite []_iceSprites;

	protected override void Awake() {

		base.Awake();
		SetMaterial (_material);
	}
	
	public override void Die()
	{
		if(!ABGameWorld.Instance._isSimulation)
		{
			ScoreHud.Instance.SpawnScorePoint(_points, transform.position);

			_destroyEffect._shootParticles = true;
			ABParticleManager.Instance.AddParticleSystem (_destroyEffect, transform.position);
		}

		base.Die();
	}

	public void SetMaterial(MATERIALS material) {

		_material = material;

		switch (material) {

		case MATERIALS.wood:
			_clips = ABWorldAssets.WOOD_DAMAGE_CLIP;
			_sprites = _woodSprites;
			_destroyEffect._particleSprites = ABWorldAssets.WOOD_DESTRUCTION_EFFECT;
			_collider.sharedMaterial = ABWorldAssets.WOOD_MATERIAL;
			_life *= 1f;
			break;

		case MATERIALS.stone:
			_clips = ABWorldAssets.STONE_DAMAGE_CLIP;
			_sprites = _stoneSprites;
			_destroyEffect._particleSprites = ABWorldAssets.STONE_DESTRUCTION_EFFECT;
			_collider.sharedMaterial = ABWorldAssets.STONE_MATERIAL;

			_life *= 2f;
			break;

		case MATERIALS.ice:
			_clips = ABWorldAssets.ICE_DAMAGE_CLIP;
			_sprites = _iceSprites;
			_destroyEffect._particleSprites = ABWorldAssets.ICE_DESTRUCTION_EFFECT;
			_collider.sharedMaterial = ABWorldAssets.ICE_MATERIAL;

			_life *= 0.5f;
			break;
		}

		_spriteRenderer.sprite = _sprites [0];
	}
}
