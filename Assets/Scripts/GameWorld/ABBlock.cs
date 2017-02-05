// SCIENCE BIRDS: A clone version of the Angry Birds game used for 
// research purposes
// 
// Copyright (C) 2016 - Lucas N. Ferreira - lucasnfe@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
//

﻿using UnityEngine;
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
	
	public override void Die(bool withEffect = true)
	{
		if(!ABGameWorld.Instance._isSimulation)
			ScoreHud.Instance.SpawnScorePoint(_points, transform.position);

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

	public override void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Bird") {

			ABBird bird = collision.gameObject.GetComponent<ABBird> ();
			float collisionMagnitude = collision.relativeVelocity.magnitude;
			float birdDamage = 1f;

			switch (_material) {

			case MATERIALS.wood:
				birdDamage = bird._woodDamage;
				break;

			case MATERIALS.stone:
				birdDamage = bird._stoneDamage;
				break;

			case MATERIALS.ice:
				birdDamage = bird._iceDamage;
				break;
			}

			DealDamage (collisionMagnitude * birdDamage);
		} 
		else {

			base.OnCollisionEnter2D (collision);
		}
	}
}
