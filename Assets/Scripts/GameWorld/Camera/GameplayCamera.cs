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
	public Vector2 _horizontalLimits;
	public Vector2 _fieldOfViewBounds;

	void Start()
	{
		_cameraHeight = 2f * Camera.main.orthographicSize;
		_cameraWidth = _cameraHeight * Camera.main.aspect;

		_dragDistance = Vector3.one;
		
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

				if(target.OutOfSlingShot)

					FollowTarget(target.transform.position, _dampTime);

				else

					if(_dragDistance.x < 0f)

						cameraSpeed.x = _horizontalLockArea.y;
					else
						cameraSpeed.x = _horizontalLockArea.x;
			else

				cameraSpeed.x = _horizontalLockArea.x;

			FollowTarget(cameraSpeed, _dampTime);
		}

		_isDraging = false;
	}
	
	void FollowTarget(Vector3 targetPosition, float dampTime)
	{
		Vector3 destination = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
		transform.position = Vector3.SmoothDamp(transform.position, destination, ref _velocity, dampTime);

		transform.position = new Vector3(Mathf.Clamp(transform.position.x,
		                                             _horizontalLockArea.x, 
		                                             _horizontalLockArea.y),
		                                transform.position.y, transform.position.z);
	}

	public void ZoomCamera(float zoomFactor)
	{

	}
	
	public void DragCamera(Vector3 dragPosition)
	{
		_isDraging = true;

		Vector3 dragPos = transform.position - dragPosition;

		if(dragPosition.x != 0f)

			_dragDistance = dragPosition;
	
		FollowTarget(dragPos, _dampTime);
	}
}
