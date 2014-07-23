using UnityEngine;
using System.Collections;

public class GameplayCamera : MonoBehaviour {

	private bool _isDraging;
	private float _cameraWidth;
	private float _cameraHeight;
	private Vector2 _horizontalLockArea;
	private Vector3 _velocity;

	public BirdsManager _birdsManager;

	public Vector2 _horizontalLimits;
	public float _dampTime = 1.0f;
	
	void Start()
	{
		_cameraHeight = 2f * Camera.main.orthographicSize;
		_cameraWidth = _cameraHeight * Camera.main.aspect;
		
		_horizontalLockArea.x = transform.position.x + _horizontalLimits.x;
		_horizontalLockArea.y = transform.position.x + _horizontalLimits.y;
	}

	void FixedUpdate()
	{
		Bird target = _birdsManager.GetCurrentBird();
		
		if (target && !_isDraging)

			FollowTarget(target.transform.position, _dampTime);

		_isDraging = false;
	}

	void LockCamera()
	{
		// Lock camera movement
		transform.position = new Vector3(Mathf.Clamp(transform.position.x, _horizontalLockArea.x, _horizontalLockArea.y),
		                                 transform.position.y, transform.position.z);
	}

	void FollowTarget(Vector3 targetPosition, float dampTime)
	{
		Vector3 point = camera.WorldToViewportPoint(targetPosition);
		Vector3 delta = new Vector3(targetPosition.x, transform.position.y, targetPosition.z) -
			camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
		
		Vector3 destination = transform.position + delta;
		transform.position = Vector3.SmoothDamp(transform.position, destination, ref _velocity, dampTime);

		LockCamera();
	}
	
	public void DragCamera(Vector3 dragPosition)
	{
		_isDraging = true;

		Vector3 dragDistance = transform.position - dragPosition;
		dragDistance.x = Mathf.Clamp(dragDistance.x, -5f, 5f);

		FollowTarget(dragDistance*0.5f, 0.25f);
	}
}
