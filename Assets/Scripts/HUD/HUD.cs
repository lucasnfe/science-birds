using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour {
	
	public float _zoomSpeed;
	public float _dragSpeed;

	public RectTransform  _scoreDisplay;

	private bool _isZoomingIn; 
	private bool _isZoomingOut;

	private uint _totalScore;

	private Vector3 _dragOrigin;
	private Bird _selecetdBird;

	void Start() {

		SetScoreDisplay(_totalScore);
	}

	// Update is called once per frame
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
	
	public void SetZoomIn(bool zoomIn)
	{
		_isZoomingIn = zoomIn;
	}
	
	public void SetZoomOut(bool zoomOut)
	{
		_isZoomingOut = zoomOut;
	}
	
	public void CameraZoom(float scrollDirection)
	{
		GameWorld.Instance._camera.ZoomCamera(Mathf.Clamp(scrollDirection, -1f, 1f) * _zoomSpeed * Time.deltaTime);
	}

	public void SetScoreDisplay(uint score)
	{
		if(_scoreDisplay)
		{
			_totalScore = score;
			_scoreDisplay.GetComponent<Text>().text = _totalScore.ToString();
		}
	}

	public void AddScore(uint score)
	{
		_totalScore += score;
		_scoreDisplay.GetComponent<Text>().text = _totalScore.ToString();
	}

	void LateUpdate()
	{
		if(Input.GetMouseButton(0))
			_dragOrigin = Input.mousePosition;
	}
}
