using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    Bird selecetdBird;
    public BirdsManager _birdsManager;

	// Update is called once per frame
	void Update () {

        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if(hit)
            {
                if(hit.transform.tag == "Bird")
                {
                    selecetdBird = hit.transform.gameObject.GetComponent<Bird>();
                    if(selecetdBird && !selecetdBird.IsSelected() && selecetdBird == _birdsManager.GetCurrentBird())
                    {
                        selecetdBird.SelectBird();
                    }
                }
            }
        }
        else if(Input.GetMouseButton(0))
        {
            if(selecetdBird && !selecetdBird.IsFlying() && selecetdBird == _birdsManager.GetCurrentBird())
            {
                Vector3 dragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                dragPosition = new Vector3(dragPosition.x, dragPosition.y, selecetdBird.transform.position.z);

                selecetdBird.DragBird(dragPosition);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if(selecetdBird && !selecetdBird.IsFlying() && selecetdBird == _birdsManager.GetCurrentBird())
            {
                selecetdBird.LaunchBird();
                selecetdBird = null;
            }
        }

	}
}
