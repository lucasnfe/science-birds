using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : ABSingleton<HUD> {
	
	public float _zoomSpeed;
	public float _dragSpeed;

	public RectTransform  _scoreDisplay;

	public float _simulateDragTime = 1f;
	public int SimulateInputEvent { get; set; }

	public Vector3 SimulateInputPos{ get; set; }
	public Vector3 SimulateInputDelta{ get; set; }

	private bool _isZoomingIn; 
	private bool _isZoomingOut;

	private uint _totalScore;
	private float _simulatedDragTimer;

	private Vector3 _inputPos;
	private Vector3 _dragOrigin;
	private ABBird _selecetdBird;

	void Start() {

		SetScoreDisplay(_totalScore);
	}

	// Update is called once per frame
	void Update () {
		
		if(_isZoomingIn) {
			
			CameraZoom(-0.5f);
			return;
		}
		
		if(_isZoomingOut) {
			
			CameraZoom(0.5f);
			return;
		}


		bool isMouseControlling = true;
		_inputPos = Input.mousePosition;

		if (SimulateInputEvent > 0) {
			_inputPos = SimulateInputPos;
			isMouseControlling = false;
		}

		if(Input.GetMouseButtonDown(0) || SimulateInputEvent == 1) {

			ClickDown (_inputPos);

			if(!isMouseControlling)
				SimulateInputEvent++;
        }
		else if(Input.GetMouseButton(0) || SimulateInputEvent == 2) {

			if(SimulateInputEvent > 0 && !isMouseControlling)
				
				Drag (SimulateInputDelta + _inputPos);
			else
				Drag (_inputPos);

			_simulatedDragTimer += Time.deltaTime;
			if (_simulatedDragTimer >= _simulateDragTime) {

				if(!isMouseControlling)
					SimulateInputEvent++;
				
				_simulatedDragTimer = 0f;
			}
        }
		else if(Input.GetMouseButtonUp(0) || SimulateInputEvent == 3) {
			
			ClickUp ();

			if(!isMouseControlling)
				SimulateInputEvent = 0;
        }
		
		if(Input.GetAxis("Mouse ScrollWheel") != 0f) {
			
			float scrollDirection = Input.GetAxis("Mouse ScrollWheel");
			CameraZoom(scrollDirection);
		}
	}

	void LateUpdate() {
		
		if(Input.GetMouseButton(0))
			_dragOrigin = _inputPos;
	}

	public void ClickDown(Vector3 position) {

		_dragOrigin = position;

		Ray ray = Camera.main.ScreenPointToRay(position);
		RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

		if(hit)
		{
			if(hit.transform.tag == "Bird")
			{
				_selecetdBird = hit.transform.gameObject.GetComponent<ABBird>();
				if(_selecetdBird && !_selecetdBird.IsSelected && _selecetdBird == ABGameWorld.Instance.GetCurrentBird())
				{
					_selecetdBird.SelectBird();
				}
			}
		}
	}

	public void ClickUp() {

		if(_selecetdBird && !_selecetdBird.IsFlying && _selecetdBird == ABGameWorld.Instance.GetCurrentBird())
		{
			_selecetdBird.LaunchBird();
			_selecetdBird = null;
		}
	}

	public void Drag(Vector3 position) {

		if(_selecetdBird) {

			if(!_selecetdBird.IsFlying && _selecetdBird == ABGameWorld.Instance.GetCurrentBird()) {
				
				Vector3 dragPosition = Camera.main.ScreenToWorldPoint(position);
				dragPosition = new Vector3(dragPosition.x, dragPosition.y, _selecetdBird.transform.position.z);

				_selecetdBird.DragBird(dragPosition);
			}
		}
		else {
			
			Vector3 dragPosition = position - _dragOrigin;
			ABGameWorld.Instance.GameplayCam.DragCamera(dragPosition * _dragSpeed * Time.fixedDeltaTime);
		}
	}

	public void SetZoomIn(bool zoomIn) {
		
		_isZoomingIn = zoomIn;
	}
	
	public void SetZoomOut(bool zoomOut) {
		
		_isZoomingOut = zoomOut;
	}
	
	public void CameraZoom(float scrollDirection) {

		ABGameWorld.Instance.GameplayCam.ZoomCamera(Mathf.Clamp(scrollDirection, -1f, 1f) * _zoomSpeed * Time.deltaTime);
	}

	public void SetScoreDisplay(uint score) {
		
		if(_scoreDisplay) {
			
			_totalScore = score;
			_scoreDisplay.GetComponent<Text>().text = _totalScore.ToString();
		}
	}

	public void AddScore(uint score) {
		
		_totalScore += score;
		_scoreDisplay.GetComponent<Text>().text = _totalScore.ToString();
	}

	public uint GetScore() {

		return _totalScore;
	}
}
