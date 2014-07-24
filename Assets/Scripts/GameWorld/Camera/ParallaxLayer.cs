using UnityEngine;
using System.Collections;

public class ParallaxLayer : MonoBehaviour
{
	public GameplayCamera _camera;
	public float _speed = 2f;

	private Vector3 _dragOrigin;

	void Start() {
		
		_dragOrigin = _camera.transform.position;
	}
		
	public void Update()
	{
		Vector3 dragDistance = _camera.transform.position - _dragOrigin;

		// Movement
		Vector3 movement = new Vector3(_speed * -dragDistance.x, 0, 0);	
		movement.x = Mathf.Clamp(movement.x, -5f, 5f);

		movement *= Time.deltaTime;
		transform.Translate(movement);

		_dragOrigin = _camera.transform.position;
	}
}
