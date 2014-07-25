using UnityEngine;
using System.Collections;

public class GameplayCamera : MonoBehaviour {

	private bool  _isDraging;
	private float _cameraWidth;
	private float _cameraHeight;
	private Vector3 _velocity;
	private Vector3 _dragDistance;
	private Vector2 _horizontalLockArea;

	public BirdsManager _birdsManager;
	public float _dampTime = 1.0f;
	public float _dragSpeed = 1.0f;
	public float _followBirdSpeed = 1.0f;
	public Vector2 _horizontalLimits;

	void Start()
	{
		_cameraHeight = 2f * Camera.main.orthographicSize;
		_cameraWidth = _cameraHeight * Camera.main.aspect;
		
		_horizontalLockArea.x = transform.position.x + _horizontalLimits.x;
		_horizontalLockArea.y = transform.position.x + _horizontalLimits.y;
	}

	void Update()
	{
		if (!_isDraging)
		{
			Vector3 cameraSpeed = Vector3.zero;
			Bird target = _birdsManager.GetCurrentBird();
	
			if(target)

				if(target.IsFlying())

					cameraSpeed.x = -_followBirdSpeed * target.rigidbody2D.velocity.normalized.x;
				
				else if(!target.OutOfSlingShot)
					
					cameraSpeed.x = _dragSpeed;

			DragCamera(cameraSpeed);
		}

		_isDraging = false;
	}
	
	void FollowTarget(Vector3 targetPosition, float dampTime)
	{
		Vector3 point = camera.WorldToViewportPoint(targetPosition);
		Vector3 delta = targetPosition - camera.ViewportToWorldPoint(new Vector3(0.5f, point.y, point.z));
		
		Vector3 destination = transform.position + delta;
		transform.position = Vector3.SmoothDamp(transform.position, destination, ref _velocity, dampTime);
		transform.position = new Vector3(Mathf.Clamp(transform.position.x, _horizontalLockArea.x, _horizontalLockArea.y),
		                                 transform.position.y, transform.position.z);
	}
	
	public void DragCamera(Vector3 dragPosition)
	{
		_isDraging = true;

		_dragDistance = transform.position - dragPosition;
		_dragDistance.x = Mathf.Clamp(_dragDistance.x, -_dragSpeed, _dragSpeed);

		FollowTarget(_dragDistance, _dampTime);
	}
}
