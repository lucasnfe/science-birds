using UnityEngine;
using System.Collections;
/** \class Slingshot
 *  \brief  Management of the Slingshot object of the game
 *
 *  Contains position of the slingshot, width, line renderer and start and update methods.
 */
public class Slingshot : MonoBehaviour {

    /**enum to check which state the slingshot is: with or without bird*/
    enum SLINGSHOT_LINE_POS
    {
        SLING,
        BIRD
    };
	/**offset to draw the line of the slingshot*/
    public Vector3 _positionOffset;
    /**width of the line of the slingshot*/
    public float _width;
    /**Line renderer to draw the slingshot's line*/
    LineRenderer _lineRenderer;

    /**
     *  On initialization gets a LineRenderer, gives to it the Material which to draw, sets the width,
     *  then sets its SLING-index position based on slingshot's (front) position + the desired offset,
     *  then sets enabled to false.
     */
    void Start() {

        _lineRenderer = gameObject.GetComponent<LineRenderer>();

        _lineRenderer.material = new Material(Shader.Find("Custom/Solid Color"));
        _lineRenderer.SetWidth(_width, _width);

        _lineRenderer.SetPosition((int)SLINGSHOT_LINE_POS.SLING, transform.position + _positionOffset);
        _lineRenderer.enabled = false;
    }

    /**
     *  At every update, check if line renderer exists and if slingshot transform is active
     *  if it is, enable the line renderer and set the BIRD-index position to Slingshot's Base's position (center).
     *  else, disables line renderer.
     */
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
