using UnityEngine;
using System.Collections;

/** \class GameplayCamera
 *  \brief  Contains the methods and attributes for camera manipulation during gameplay
 *
 *  Manipulates zoom, draging and bounds of the camera, moving it around during gameplay
 */
public class GameplayCamera : MonoBehaviour {
    /**Initial camera rectangle*/
	private Rect  _initialCameraRect;
	/*Check if camera is draging**/
	private bool  _isDraging;
    /**minimun width of the camera*/
	private float _minWidth;
    /**Distance to drag the camera*/
	private float _dragDistance;

    /**
     *  damp time used on the lerp effect (makes the camera moves a little slower than the speed of the object it is 
     *  following)
     */
	public float _dampTime;
    /**Maximun width the camera has*/
	public float _cameraMaxWidth;

    /**
     *  At initialization, sets drag distance to 1, calculates the initital rectangle of the camera
     *  and gets its width for the minimun width.
     */
	void Awake()
	{
		_dragDistance = 1f;
		_initialCameraRect = CalculateCameraRect();
		_minWidth = _initialCameraRect.width;
	}
	/**
     *  At every update, updates camera zoom, check if is draging, if not, sets the current bird as a target to camera,
     *  zeroes the next position of the camera vector, makes its x coordinate the leftbound, and, if the bird is out of
     *  the slingshot and in front of it, makes the x coordinate half of the right bound. If not, checks the drag distance
     *  if negative the x coordinate is the right bound, if positive it is the left bound. Then, follows the target using
     *  the next camera position calculated before. At last, set isDraging to false.
     */
	void Update()
	{
		// Updating camera zoom
		CalculateOrthographicSize();

		if(!_isDraging)
		{
			Bird target = GameWorld.Instance.GetCurrentBird();

			Vector3 cameraNextPos = Vector3.zero;
			cameraNextPos.x = LeftBound();

			if(target && target.OutOfSlingShot && target.IsInFrontOfSlingshot())
			{
				cameraNextPos.x = RightBound()/2f;
			}
			else
			{
				if(_dragDistance < 0f)

					cameraNextPos.x = RightBound();
				else
					cameraNextPos.x = LeftBound();
			}

			FollowTarget(cameraNextPos);
		}

		_isDraging = false;
	}
    /**
     *  Follows the target, giving the camera position the position of the transform of the camera, and then calculating its
     *  X coordinate with Lerp, using the actual position, the target position and a damp time. Then, makes the position
     *  of the transform of the camera the new camera position
     *  @param[in]  targetPosition  Vector3 containing the position of the target of the camera. 
     */
	void FollowTarget(Vector3 targetPosition)
	{
		Vector3 cameraPos = transform.position;
		cameraPos.x = Mathf.Lerp(cameraPos.x, targetPosition.x, _dampTime * Time.deltaTime);
		transform.position = cameraPos;
	}

    /**Getter for the left bound, returning the subtraction of half the width of the camera minus half the max width*/
	public float LeftBound()
	{
		return _initialCameraRect.width/2f - _cameraMaxWidth/2f;
	}

    /**Getter for the right bound, returning the subtraction of half the max width minus half the initial width*/
    public float RightBound()
	{
		return _cameraMaxWidth/2f - _initialCameraRect.width/2f;
	}
	
    /**
     *  Drags the camera based on the drag distance, gets the current bird as a target, and, if not flying sets isDraging to true
     *  and then if dragDistance magnitude is positive, sets _dragDistance to the dragDistance.x  coordinate.
     *  Then, sets the camera position as the gameplay camera transform position, and calculates the camera position
     *  X coordinate using lerp and them clamping it. At last, sets the gameplay camera transform position to the camera position.
     *  @param[in]  dragDistance    Vector3 containing the distance which to drag the camera.
     */
	public void DragCamera(Vector3 dragDistance)
	{
		Bird target = GameWorld.Instance.GetCurrentBird();

		if(target && target.IsFlying)
			return;

		_isDraging = true;

		if(dragDistance.magnitude > 0f)
			_dragDistance = dragDistance.x;

		Vector3 cameraPos = transform.position;
		cameraPos.x = Mathf.Lerp(cameraPos.x, cameraPos.x - dragDistance.x, _dampTime * Time.deltaTime);
		cameraPos.x = Mathf.Clamp(cameraPos.x, LeftBound(), RightBound());
		transform.position = cameraPos;
	}
	
    /**
     *  Sets the camera width, clamping it
     *  @param[in]  width   new width to set the camera rectangle
     */
	public void SetCameraWidth(float width)
	{
		_initialCameraRect.width = width;
		_initialCameraRect.width = Mathf.Clamp(_initialCameraRect.width,  _minWidth, _cameraMaxWidth);
	}
	
    /**
     *  If not a simulation, calculates the camera zoom using the zoom factor and clamping it
     *  @param[in]  zoomFactor  float containing factor to use in zoom
     */
	public void ZoomCamera(float zoomFactor)
	{
		if(!GameWorld.Instance._isSimulation)
		{
			_initialCameraRect.width += zoomFactor;
			_initialCameraRect.width = Mathf.Clamp(_initialCameraRect.width,  _minWidth, _cameraMaxWidth);
		}
	}

    /**
     *  calculates the width and height of the camera rectangle and then creates a rectangle object using them
     *  @return Rect    rectangle of the camera.
     */
	public Rect CalculateCameraRect()
	{
		float height = 2f * GetComponent<Camera>().orthographicSize;
		float width = height * GetComponent<Camera>().aspect;	

		return new Rect(transform.position.x - width/2f, transform.position.y - height/2f, width, height);
	}
    /**
     *  calculates the ortographicSize component of the camera. Gets its actual size, calculates the top right based on
     *  camera rectangle, gets the world to viewport point based on the top right, if the viewport x is greater than y
     *  the ortographic is calculated based on the width and aspect. Else, it is calculated by the heigth.
     *  the the ortographic size is calculated by Lerping the actual size, the found size and the damp time.
     *  @return float   ortographic size of the camera.
     */
	public float CalculateOrthographicSize()
	{
		float orthographicSize = GetComponent<Camera>().orthographicSize;
		
		Vector3 topRight = new Vector3(_initialCameraRect.x + _initialCameraRect.width, _initialCameraRect.y, 0f);
		Vector3 topRightAsViewport = GetComponent<Camera>().WorldToViewportPoint(topRight);
		
		if (topRightAsViewport.x >= topRightAsViewport.y)

			orthographicSize = Mathf.Abs(_initialCameraRect.width) / GetComponent<Camera>().aspect / 2f;
		else
			orthographicSize = Mathf.Abs(_initialCameraRect.height) / 2f;
		
		GetComponent<Camera>().orthographicSize = Mathf.Lerp(GetComponent<Camera>().orthographicSize, orthographicSize, _dampTime * Time.deltaTime);
		
		return orthographicSize;
	}
}
