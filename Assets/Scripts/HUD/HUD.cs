using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    Bird _selecetdBird;
    public BirdsManager _birdsManager;
	public GameplayCamera _camera;
	public Vector3 _dragOrigin;
	
	void Start() {
	
		//InvokeRepeating("CollectDragStartingPoint", 0f, 0.1f);
	}
	
	// Update is called once per frame
	void FixedUpdate () {

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
					if(_selecetdBird && !_selecetdBird.IsSelected() && _selecetdBird == _birdsManager.GetCurrentBird())
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
				if(!_selecetdBird.IsFlying() && _selecetdBird == _birdsManager.GetCurrentBird())
				{
                	Vector3 dragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					dragPosition = new Vector3(dragPosition.x, dragPosition.y, _selecetdBird.transform.position.z);

					_selecetdBird.DragBird(dragPosition);
				}
            }
			else
			{
				Vector3 dragPosition = Input.mousePosition - _dragOrigin;
				dragPosition = new Vector3(dragPosition.x, _camera.transform.position.y, _camera.transform.position.z);

				_camera.DragCamera(dragPosition);
			}

			_dragOrigin = Input.mousePosition;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if(_selecetdBird && !_selecetdBird.IsFlying() && _selecetdBird == _birdsManager.GetCurrentBird())
            {
                _selecetdBird.LaunchBird();
                _selecetdBird = null;
            }
        }
	}

	void CollectDragStartingPoint()
	{
		if(Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))

			_dragOrigin = Input.mousePosition;
	}
}
