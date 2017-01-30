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

	public  int           _childrenAmount = 3;
	public  Vector2       _attackForce;
	private ABBBirdBlue []_childrenBirds;

	void CreateChildren() {

		_childrenBirds = new ABBBirdBlue [_childrenAmount];
		 
		for(int i = 0; i < _childrenAmount; i++) {

			GameObject obj = (GameObject)Instantiate(ABWorldAssets.BIRDS["BirdBlue"], transform.parent);
			_childrenBirds[i] = obj.GetComponent<ABBBirdBlue> ();
			_childrenBirds[i].gameObject.SetActive (false);
		}
	}
		
	void SpecialAttack() {

		Vector2 force = _attackForce;
		foreach (ABBBirdBlue child in _childrenBirds) {

			child.IsSelected = IsSelected;
			child.IsFlying = IsFlying;
			child.OutOfSlingShot = OutOfSlingShot;
			child.JumpToSlingshot = JumpToSlingshot;
			child.transform.position = transform.position;

			child.gameObject.SetActive (true);
			child.LaunchBird (force);
			force.y -= 1f;
		}

		Die ();
	}
}
