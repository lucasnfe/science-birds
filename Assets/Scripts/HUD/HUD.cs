using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/** \class HUD
 *  \brief  Handles the HUD display, zooming of the scene and player drag on slingshot
 *
 *  Handles the HUD display, showing and updating the score,
 *  handles the zooming in and out of the scene as well as its speed, 
 *  and the player drag on slingshot to throw the birds
 */
public class HUD : MonoBehaviour {
	/**Zooming speed*/
	public float _zoomSpeed;
    /**Slingshot drag speed*/
	public float _dragSpeed;
    /**Display Rectangle for score*/
	public RectTransform  _scoreDisplay;
    /**Control boolean to know if is zooming in*/
	private bool _isZoomingIn;
    /**Control boolean to know if is zooming out*/
    private bool _isZoomingOut;

    /**Score amount of the player*/
    private uint _totalScore;
    /**Coordinates of the origin of the drag on the slingshot (where the player first clicked before dragging)*/
    private Vector3 _dragOrigin;
    /**Which bird is currently selected to be shot*/
	private Bird _selecetdBird;
    /**
     *  When the script is initialized, displays the score
     */
	void Start() {

		SetScoreDisplay(_totalScore);
	}

	/**
     *  Once per frame, checks if zooming in or out and do the zoom if doing any.
     *  Also checks if left mouse button is pressed, if it is, check if a bird is selected, if it is not,
     *  and a bird is being clicked, put this bird as selected. Also, if a bird is selected and the mouse 
     *  left-button is clicked, check where the mouse is, if not on bird, carries the bird to the new position.
     *  Also checks if mouse left-button got up, and, if a bird where selected and not already flying, throws the bird.
     *  Lastly, check if mouse is being scrolled and if it is, zoom accordingly to the scroll direction.
     */
	void Update () {
		
		if(_isZoomingIn)
		{
			CameraZoom(-0.5f);
			return;
		}
		
		if(_isZoomingOut)
		{
			CameraZoom(0.5f);
			return;
		}

        if(Input.GetMouseButtonDown(0))
        {
			_dragOrigin = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if(hit)
            {
                if(hit.transform.tag == "Bird")
                {
					_selecetdBird = hit.transform.gameObject.GetComponent<Bird>();
					if(_selecetdBird && !_selecetdBird.IsSelected && _selecetdBird == GameWorld.Instance.GetCurrentBird())
                    {
						_selecetdBird.SelectBird();
                    }
                }
            }
        }
        else if(Input.GetMouseButton(0))
        {
            if(_selecetdBird)
            {
				if(!_selecetdBird.IsFlying && _selecetdBird == GameWorld.Instance.GetCurrentBird())
				{
                	Vector3 dragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					dragPosition = new Vector3(dragPosition.x, dragPosition.y, _selecetdBird.transform.position.z);

					_selecetdBird.DragBird(dragPosition);
				}
            }
			else
			{
				Vector3 dragPosition = Input.mousePosition - _dragOrigin;
				GameWorld.Instance._camera.DragCamera(dragPosition * _dragSpeed * Time.fixedDeltaTime);
			}
        }
        else if(Input.GetMouseButtonUp(0))
        {
			if(_selecetdBird && !_selecetdBird.IsFlying && _selecetdBird == GameWorld.Instance.GetCurrentBird())
            {
                _selecetdBird.LaunchBird();
                _selecetdBird = null;
            }
        }
		
		if(Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			float scrollDirection = Input.GetAxis("Mouse ScrollWheel");
			CameraZoom(scrollDirection);
		}
	}
	/**
     *  Setter for the variable _isZoomingIn.
     *  @param[in]  zoomIn  true if is zooming in, false otherwhise.
     */
	public void SetZoomIn(bool zoomIn)
	{
		_isZoomingIn = zoomIn;
	}
    /**
     *  Setter for the variable _isZoomingOut.
     *  @param[in]  zoomOut  true if is zooming in, false otherwhise.
     */
    public void SetZoomOut(bool zoomOut)
	{
		_isZoomingOut = zoomOut;
	}
    /**
     *  Calculates and does the zoom of the camera.
     *  @param[in]  scrollDirection  A positive floating number if it is zooming in, a negative one if zooming out
     */
    public void CameraZoom(float scrollDirection)
	{
		GameWorld.Instance._camera.ZoomCamera(Mathf.Clamp(scrollDirection, -1f, 1f) * _zoomSpeed * Time.deltaTime);
	}
    /**
     *  Sets the current scrote on the display if the score display exists.
     *  @param[in]  score   Unsigned integer containing the actual score to be displayed
     */
	public void SetScoreDisplay(uint score)
	{
		if(_scoreDisplay)
		{
			_totalScore = score;
			_scoreDisplay.GetComponent<Text>().text = _totalScore.ToString();
		}
	}
    /**
     *  Adds new points to the actual score and updates the display
     *  @param[in]  score   Unsigned integer corresponding to the new points
     */
	public void AddScore(uint score)
	{
		_totalScore += score;
		_scoreDisplay.GetComponent<Text>().text = _totalScore.ToString();
	}
    /**
     *  LateUpdate() is called every after Update() has been called, checks if left mouse button is down
     *  and if it is, saves a new drag origin using the actual mouse position
     */
	void LateUpdate()
	{
		if(Input.GetMouseButton(0))
			_dragOrigin = Input.mousePosition;
	}
}
