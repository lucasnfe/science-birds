using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
	
	public float _zoomSpeed;
	public float _scrollSpeed;

    public GameWorld _gameWorld;
	public GameplayCamera _camera;
	
	private Bird _selecetdBird;
	private Vector3 _dragOrigin;
	
	// Update is called once per frame
	void Update () {

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
					if(_selecetdBird && !_selecetdBird.IsSelected && _selecetdBird == _gameWorld.GetCurrentBird())
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
				if(!_selecetdBird.IsFlying() && _selecetdBird == _gameWorld.GetCurrentBird())
				{
                	Vector3 dragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					dragPosition = new Vector3(dragPosition.x, dragPosition.y, _selecetdBird.transform.position.z);

					_selecetdBird.DragBird(dragPosition);
				}
            }
			else
			{
				Vector3 dragPosition = Input.mousePosition - _dragOrigin;
				dragPosition.x = Mathf.Clamp(dragPosition.x, -_scrollSpeed, _scrollSpeed);

				dragPosition = new Vector3(dragPosition.x, _camera.transform.position.y, _camera.transform.position.z);

				_camera.DragCamera(dragPosition);
			}
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if(_selecetdBird && !_selecetdBird.IsFlying() && _selecetdBird == _gameWorld.GetCurrentBird())
            {
                _selecetdBird.LaunchBird();
                _selecetdBird = null;
            }
        }

		if(Input.GetAxis("Mouse ScrollWheel") != 0f)
			_camera.ZoomCamera(Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed * Time.deltaTime);

		if(Input.GetKey("a"))
		{
			Application.LoadLevel(Application.loadedLevel);
		}
	}

	void LateUpdate()
	{
		if(Input.GetMouseButton(0))
			
			_dragOrigin = Input.mousePosition;
	}
}
