using UnityEngine;
using System.Collections;

public class Scroll : MonoBehaviour {

	public float mouseSensitivity = 1.0f;
	public float maxDistanceLeft = 1.0f;
	public float maxDistanceRight = 1.0f;
	
	public Rigidbody2D[] backgroundLayers;
	
	private bool didTouchBackground = false;
	
	protected Vector3 lastPosition;
	protected Vector3 initialPosition;

	void Start()
	{
		initialPosition = transform.position;
	}

	public void Update()
	{
	    if (Input.GetMouseButtonDown(0))
	    {
			lastPosition = Input.mousePosition;
			
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

	        if(hit && hit.transform.tag != "Bird")
				didTouchBackground = true;
			else
				didTouchBackground = false;
	    }
 
	    if (Input.GetMouseButton(0))
	    {
	        if(didTouchBackground)
			{
		        Vector3 deltaMovement = Input.mousePosition - lastPosition;
				deltaMovement = new Vector3(Mathf.Clamp(deltaMovement.x, -30, 30), deltaMovement.y, 0);
				
				float currentAppliedForce = deltaMovement.x * mouseSensitivity * Time.deltaTime;		
				rigidbody2D.AddForce(new Vector2(currentAppliedForce, 0));
	        
				foreach(Rigidbody2D layer in backgroundLayers)
				{
					currentAppliedForce *= 0.75f;
					layer.AddForce(new Vector2(currentAppliedForce, 0));
				}
			
				lastPosition = Input.mousePosition;
			}
	    }
		
		float currentmaxDistanceLeft = maxDistanceLeft;
		float currentmaxDistanceRight = maxDistanceRight;
		
		foreach(Rigidbody2D layer in backgroundLayers)
		{
			currentmaxDistanceLeft *= 0.75f;
			currentmaxDistanceRight *= 0.75f;
			
			layer.transform.position = new Vector3(Mathf.Clamp(layer.transform.position.x, initialPosition.x - currentmaxDistanceLeft, 
																		   initialPosition.x + currentmaxDistanceRight), layer.transform.position.y, layer.transform.position.z); 
			
			
		}
	
		
		transform.position = new Vector3(Mathf.Clamp(transform.position.x, initialPosition.x - maxDistanceLeft, 
																		   initialPosition.x + maxDistanceRight), transform.position.y, transform.position.z); 
	}
}
