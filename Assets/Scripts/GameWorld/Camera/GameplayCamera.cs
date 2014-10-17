using UnityEngine;
using System.Collections;

public class GameplayCamera : MonoBehaviour {
	
	private float   _minWidth;
	private bool    _isDraging;
	private Vector3 _velocity;
	private Vector3 _dragDistance;
	private Rect    _initialCameraRect;
	
	public GameWorld _gameWorld;

	public float _dampTime;
	public float _levelWidth;

	void Start()
	{
		_initialCameraRect = CalculateCameraRect();
		_dragDistance = Vector3.one;

		_minWidth = _initialCameraRect.width;
	}
	
	void FixedUpdate()
	{
		if(!_isDraging)
		{
			// Updating camera zoom
			camera.orthographicSize = CalculateOrthographicSize(_initialCameraRect);

			float cameraDampTime = _dampTime;
			Vector3 cameraNextPos = Vector3.zero;
			Bird target = _gameWorld.GetCurrentBird();
	
			if(target)
			{
				float birdVelocity = target.rigidbody2D.velocity.x;

				if(target.OutOfSlingShot && birdVelocity <= 0f)
					return;

				if(target.OutOfSlingShot)
				{
					cameraNextPos.x = RightBound();
					cameraDampTime = _dampTime * (20f/birdVelocity);
				}
			}
			else
			{
				cameraNextPos.x = LeftBound();
			}

			if(cameraNextPos == Vector3.zero)
			{
				if(_dragDistance.x < 0f)
					
					cameraNextPos.x = RightBound();
				else
					cameraNextPos.x = LeftBound();
			}

			FollowTarget(cameraNextPos, cameraDampTime);
		}

		_isDraging = false;
	}

	void FollowTarget(Vector3 targetPosition, float dampTime)
	{
		Vector3 destination = new Vector3(targetPosition.x, transform.position.y, transform.position.z);

		Vector3 cameraPos = transform.position;

		cameraPos = Vector3.SmoothDamp(cameraPos, destination, ref _velocity, dampTime * Time.deltaTime);
		cameraPos.x = Mathf.Clamp(cameraPos.x, LeftBound(), RightBound());

		transform.position = cameraPos;
	}

	public float LeftBound()
	{
		return _initialCameraRect.width/2f - _levelWidth/2f;
	}

	public float RightBound()
	{
		return _levelWidth/2f - _initialCameraRect.width/2f;
	}
	
	public void DragCamera(Vector3 dragPosition)
	{
		Bird target = _gameWorld.GetCurrentBird();
		
		if(target && target.IsFlying())
			return;
		
		_isDraging = true;
		
		Vector3 dragPos = transform.position - dragPosition;
		
		if(dragPosition.x != 0f)
			_dragDistance = dragPosition;

		FollowTarget(dragPos, _dampTime);
	}

	public void ZoomCamera(float zoomFactor)
	{
		//zoomFactor = Mathf.Clamp(zoomFactor,  -0.5f, 0.5f);
		_initialCameraRect.width += zoomFactor;
		_initialCameraRect.width = Mathf.Clamp(_initialCameraRect.width,  _minWidth, _levelWidth);
	}

	public Rect CalculateCameraRect()
	{
		float height = 2f * camera.orthographicSize;
		float width = height * camera.aspect;	
		
		return new Rect(transform.position.x - width/2f, transform.position.y - height/2f, width, height);
	}

	float CalculateOrthographicSize(Rect boundingBox)
	{
		float orthographicSize = camera.orthographicSize;
		Vector3 topRight = new Vector3(boundingBox.x + boundingBox.width, boundingBox.y, 0f);
		Vector3 topRightAsViewport = camera.WorldToViewportPoint(topRight);
		
		if (topRightAsViewport.x >= topRightAsViewport.y)

			orthographicSize = Mathf.Abs(boundingBox.width) / camera.aspect / 2f;
		else
			orthographicSize = Mathf.Abs(boundingBox.height) / 2f;
		
		return orthographicSize;
	}
}
