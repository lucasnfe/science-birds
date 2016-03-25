using UnityEngine;
using System.Collections;
/** \class ParallaxLayer
 *  \brief  Contains the parallax algorithm
 *
 *  Translates the layer in a different speed then the non-parallax one, giving to it the parallax effect
 */
public class ParallaxLayer : MonoBehaviour
{
    /**Parallax speed*/
	public float _speed = 1f;
    /**Origin of the drag to be used on movement calculation*/
	private Vector3 _dragOrigin;

    /**At initialization, saves the actual camera transform position into drag origin*/
	void Start() {
		
		_dragOrigin = GameWorld.Instance._camera.transform.position;
	}
		
    /**
     *  After updating everything, calculates the drag distance by the difference between the actual camera position and
     *  the one at the initialization. Then calculates the layer movement based on the parallax speed, the drag distance
     *  and using a fixed delta to smooth the movement. Then, translates the layer and updates the drag origin to the actual
     *  camera position.
     */
	void LateUpdate()
	{
		Vector3 dragDistance = GameWorld.Instance._camera.transform.position - _dragOrigin;

		// Movement
		Vector3 movement = new Vector3(_speed * -dragDistance.x, 0, 0) * Time.fixedDeltaTime;
		transform.Translate(movement);

		_dragOrigin = GameWorld.Instance._camera.transform.position;
	}
}
