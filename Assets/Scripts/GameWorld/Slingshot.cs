using UnityEngine;
using System.Collections;

public class Slingshot : MonoBehaviour {

    enum SLINGSHOT_LINE_POS
    {
        SLING,
        BIRD
    };
	
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
		if(_lineRenderer && GameWorld.Instance._slingshotBaseTransform.gameObject.activeSelf)
        {
            _lineRenderer.enabled = true;
			_lineRenderer.SetPosition((int)SLINGSHOT_LINE_POS.BIRD, GameWorld.Instance._slingshotBaseTransform.transform.position);
        }
        else

            _lineRenderer.enabled = false;
    }
}
