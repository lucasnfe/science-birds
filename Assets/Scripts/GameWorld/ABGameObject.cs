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
using System;
using System.Collections;

[RequireComponent (typeof (Collider2D))]
[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (SpriteRenderer))]
[RequireComponent (typeof (ABParticleSystem))]
[RequireComponent (typeof (AudioSource))]
public abstract class ABGameObject : MonoBehaviour
{	
	protected int   _spriteChangedTimes;

	protected Collider2D       _collider;
	protected Rigidbody2D      _rigidBody;
	protected AudioSource      _audioSource;
	protected SpriteRenderer   _spriteRenderer;
	protected ABParticleSystem _destroyEffect;

	public Sprite[]    _sprites;
	public AudioClip[] _clips;

	public float _life = 10f;
	public float _timeToDie = 1f;

	public bool IsDying { get; set; }

	protected virtual void Awake() {

		_collider       = GetComponent<Collider2D> ();
		_rigidBody      = GetComponent<Rigidbody2D> ();
		_destroyEffect  = GetComponent<ABParticleSystem> ();
		_spriteRenderer = GetComponent<SpriteRenderer> ();
		_audioSource    = GetComponent<AudioSource> ();

		IsDying = false;
	}

	protected virtual void Update() {

		DestroyIfOutScreen ();
	}

	public virtual void Die()
	{
		Destroy(gameObject);
	}

	public virtual void OnCollisionEnter2D(Collision2D collision)
	{
		DealDamage (collision.relativeVelocity.magnitude);
	}

	public void DealDamage(float damage) {

		if(damage >= _life/_sprites.Length)
		{
			_spriteChangedTimes = Mathf.Clamp (_spriteChangedTimes, 0, _sprites.Length - 1);
			_spriteRenderer.sprite = _sprites[_spriteChangedTimes];

			if(!ABGameWorld.Instance._isSimulation)
				_audioSource.PlayOneShot(_clips[0]);

			_spriteChangedTimes++;
		}

		if(_spriteChangedTimes >= _sprites.Length || damage > _life) {

			ABAudioController.Instance.PlayIndependentSFX(_clips[1]);

			IsDying = true;
			Invoke("Die", _timeToDie);
		}
	}

	void DestroyIfOutScreen() {

		if (ABGameWorld.Instance.IsObjectOutOfWorld (transform, _collider)) {

			IsDying = true;
			Die ();
		}
	}
}
