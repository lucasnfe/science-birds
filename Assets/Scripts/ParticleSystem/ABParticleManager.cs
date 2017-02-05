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
using System.Collections.Generic;

public class ABParticleManager : ABSingleton<ABParticleManager> {

	private ABObjectPool<ABParticleSystem>  _slugParticleSystems;

	// Use this for initialization
	void Start () {

		_slugParticleSystems = new ABObjectPool<ABParticleSystem>(30);
	}

	public ABParticleSystem AddParticleSystem(ABParticleSystem clonePart, Vector3 position) {

		ABParticleSystem particleSys = _slugParticleSystems.GetFreeObject ();
		particleSys.transform.position = position;

		// Randomly assign particle system attributes
		particleSys._minVel   = clonePart._minVel;
		particleSys._maxVel   = clonePart._maxVel;
		particleSys._minAngle = clonePart._minAngle;
		particleSys._maxAngle = clonePart._maxAngle;
		particleSys._minMass  = clonePart._minMass;
		particleSys._maxMass  = clonePart._maxMass;
		particleSys._minLifetime     = clonePart._minLifetime;
		particleSys._maxLifetime     = clonePart._maxLifetime;
		particleSys._systemLifetime  = clonePart._systemLifetime;
		particleSys._shootingRate    = clonePart._shootingRate;
		particleSys._shootParticles  = clonePart._shootParticles;
		particleSys._applyGravity    = clonePart._applyGravity;
		particleSys._particleSprites = clonePart._particleSprites;

		return particleSys;
	}

	public void RemoveParticleSystem(ABParticleSystem clonePart) {

		_slugParticleSystems.SetFreeObject (clonePart.gameObject);
	}
}
