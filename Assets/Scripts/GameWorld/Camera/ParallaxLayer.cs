using UnityEngine;
using System.Collections;

public class ParallaxLayer : MonoBehaviour
{
	public GameplayCamera _camera;
	public float _speed = 1f;

	private Vector3 _dragOrigin;

	void Start() {
		
		_dragOrigin = _camera.transform.position;
	}
		
	void LateUpdate()
	{
		Vector3 dragDistance = _camera.transform.position - _dragOrigin;

		// Movement
		Vector3 movement = new Vector3(_speed * -dragDistance.x, 0, 0) * Time.fixedDeltaTime;
		transform.Translate(movement);

		_dragOrigin = _camera.transform.position;
	}
}
