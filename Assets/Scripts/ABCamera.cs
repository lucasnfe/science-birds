using UnityEngine;
using System.Collections;

public class ABCamera : Scroll {
	
	public Bird target;
	
	public float speed = 1.0f;
	public float minSize = 1.0f;
	public float maxSize = 1.0f;
    public float dampTime = 0.15f;
	
    private Vector3 velocity = Vector3.zero;
	
	// Update is called once per frame
	void Update () 
	{	
		base.Update();
		
	    if(Input.GetAxis("Mouse ScrollWheel") < 0) // back
		{
			Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize - speed * Time.deltaTime);
		}
		if(Input.GetAxis("Mouse ScrollWheel") > 0) // forward
		{
			Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize + speed * Time.deltaTime);
		}
			
		Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minSize, maxSize);
	}
	
	void LateUpdate() 
	{		
		float cameraHelfWidth = camera.orthographicSize * camera.aspect;
		float cameraLeftSide = transform.position.x - cameraHelfWidth;
		
		if(target)
		{ 
			float dist = Mathf.Sqrt((target.transform.position.x - cameraLeftSide) * 
									(target.transform.position.x - cameraLeftSide));
		
		    if (target.didHurled() && dist >= cameraHelfWidth)
			{
				MoveCamera();
			}
		}
	}
	 
	void MoveCamera() {
		
        Vector3 point = camera.WorldToViewportPoint(target.transform.position);
		
        Vector3 delta = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - 
			camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
        
		Vector3 destination = transform.position + delta;
        
		transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
	    transform.position = new Vector3(target.transform.position.x, camera.transform.position.y, camera.transform.position.z);
		
		transform.position = new Vector3(Mathf.Clamp(transform.position.x, base.initialPosition.x - base.maxDistanceLeft, 
																		   base.initialPosition.x + base.maxDistanceRight), transform.position.y, transform.position.z);	
	}
}
