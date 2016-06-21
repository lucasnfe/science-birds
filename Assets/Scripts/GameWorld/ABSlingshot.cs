using UnityEngine;
using System.Collections;

public class ABSlingshot : MonoBehaviour {
	
    public Vector3 _positionOffset;
    public float _width;

    LineRenderer _lineRenderer;

    void Start() {

        _lineRenderer = gameObject.GetComponent<LineRenderer>();

        _lineRenderer.material = new Material(Shader.Find("Custom/Solid Color"));
        _lineRenderer.SetWidth(_width, _width);

		_lineRenderer.SetPosition((int)SLINGSHOT_LINE_POS.SLING, transform.position + _positionOffset);
        _lineRenderer.enabled = false;
    }

    void Update()
    {
		if(_lineRenderer && ABGameWorld.Instance.IsSlingshotBaseActive())
        {
            _lineRenderer.enabled = true;
			_lineRenderer.SetPosition((int)SLINGSHOT_LINE_POS.BIRD, ABGameWorld.Instance.GetSlingshotBasePosition());
        }
        else

            _lineRenderer.enabled = false;
    }
}
