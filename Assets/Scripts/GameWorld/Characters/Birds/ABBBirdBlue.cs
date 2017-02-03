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

using UnityEngine;
using System.Collections;

public class ABBBirdBlue : ABBird {

	public  int   _childrenAmount = 3;
	public  float _angleBetweenBirds = 5f;
	private ABBBirdBlue []_childrenBirds;

	void CreateChildren() {

		_childrenBirds = new ABBBirdBlue [_childrenAmount];
		 
		for(int i = 0; i < _childrenAmount; i++) {

			GameObject obj = (GameObject)Instantiate(ABWorldAssets.BIRDS["BirdBlue"], transform.parent);
			_childrenBirds[i] = obj.GetComponent<ABBBirdBlue> ();
			_childrenBirds[i].CancelInvoke ("IdleJump");
			_childrenBirds[i].gameObject.SetActive (false);
		}
	}
		
	void SpecialAttack() {

		float currentBirdAngle = _angleBetweenBirds * 0.5f * _childrenBirds.Length;
		Vector2 direction = Quaternion.AngleAxis(currentBirdAngle, Vector3.forward) * _rigidBody.velocity.normalized;

		foreach (ABBBirdBlue child in _childrenBirds) {

			child.IsFlying = IsFlying;
			child.OutOfSlingShot = OutOfSlingShot;
			child.JumpToSlingshot = JumpToSlingshot;
			child.transform.position = transform.position;

			child.gameObject.SetActive (true);
			child._rigidBody.gravityScale = _rigidBody.gravityScale;
			child._rigidBody.velocity = direction * _rigidBody.velocity.magnitude;

			direction = Quaternion.AngleAxis(-_angleBetweenBirds, Vector3.forward) * direction.normalized;
		}

		Die ();
	}
}
