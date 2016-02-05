using UnityEngine;
using System.Collections;

public class ParallaxLayer : MonoBehaviour
{
	public float _speed = 1f;
	private Vector3 _dragOrigin;

	void Start() {
		
		_dragOrigin = GameWorld.Instance.GameplayCam.transform.position;
	}
		
	void LateUpdate()
	{
		Vector3 dragDistance = GameWorld.Instance.GameplayCam.transform.position - _dragOrigin;

		// Movement
		Vector3 movement = new Vector3(_speed * -dragDistance.x, 0, 0) * Time.fixedDeltaTime;
		transform.Translate(movement);

		_dragOrigin = GameWorld.Instance.GameplayCam.transform.position;
	}
}
