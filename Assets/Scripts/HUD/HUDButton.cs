using UnityEngine;

[RequireComponent(typeof(GUITexture))]
public abstract class HUDButton : MonoBehaviour
{
	private GUITexture _gui;
	protected Vector2 _mousePosition;

	//This means this button can only be 'pressed' once a frame.
	protected bool _buttonEnabled;

	public Color _pressedColor;
	private Color _defaultColor;

    public Vector2 screenOffset;

	protected virtual void Start() {

	    _gui = GetComponent<GUITexture>();
		_defaultColor = guiTexture.color;
	}

	protected virtual void Update() {

		if(Input.GetMouseButtonDown(0) && _gui.HitTest(Input.mousePosition))
		{
			ButtonTouchDown();
			return;
		}

		if(_buttonEnabled)
		{
			if(Input.GetMouseButton(0))
			{
				ButtonTouch();
				return;
			}

			if(Input.GetMouseButtonUp(0))
			{
				ButtonTouchUp();
				return;
			}
		}
	}

	public virtual void ButtonTouchDown()
	{
		//This button has been hit.
	    _buttonEnabled = true;
        guiTexture.color = _pressedColor;
	}

	public virtual void ButtonTouch()
	{
		//This button has been hit.
	    _buttonEnabled = true;
        guiTexture.color = _pressedColor;
	}

	public virtual void ButtonTouchUp()
	{
		//This button has been hit.
	    _buttonEnabled = false;
        guiTexture.color = _defaultColor;
	}

    public bool isEnabled()
    {
        return _buttonEnabled;
    }
}
