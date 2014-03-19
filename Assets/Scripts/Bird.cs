using UnityEngine;
using System.Collections;

public class Bird : MonoBehaviour {
	
	public float mouseSensitivity = 1.0f;
	public float birdFollowSpeed = 1.0f;
	public float movementRadius = 1.0f;
		
	public bool _isMainBird = false;
	public void setMainBird(bool mainBird)
	{
		_isMainBird = mainBird;
	}
		
	private bool didTouchBird = false;
	
	private float blinkTimer;
	private float pressedTimer;
	
	private float nextBlinkTime;
	private float nextPressedBlinkTime;
	
	private Vector3 initialPosition;
	private Vector3 deltaMovement;
	
	private Animator animator;
	private CircleCollider2D collider;
	
	private bool _didHurled = false;
    public bool didHurled()
	{
		return _didHurled;
	}
	

	// Use this for initialization
	void Start () {
	
		nextBlinkTime = Random.Range(0.5f, 4.0f);
		nextPressedBlinkTime = Random.Range(0.5f, 4.0f);
				
	    animator = GetComponent<Animator>();		
		collider = GetComponent<CircleCollider2D>();
	}
		
	// Update is called once per frame
	void Update () {
		
	    if (Input.GetMouseButtonDown(0) && _isMainBird)
	    {
	        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

	        if(collider.OverlapPoint(mousePosition))
			{
				initialPosition = transform.position;
				
				didTouchBird = true;
				animator.SetBool("pressed", true);
			}
			else
			{
				didTouchBird = false;
			}
	    }
		
		if(Input.GetMouseButton(0) && didTouchBird)
		{
			// The bird must blink randomly if he is being pressed
			pressedTimer += Time.deltaTime;
		
			if(pressedTimer >= nextPressedBlinkTime)
			{
				animator.SetBool("pressed_blink", true);		
			
				nextPressedBlinkTime = Random.Range(0.5f, 4.0f);
				pressedTimer = 0.0f;	
			}
			
			Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePosition = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
			
			if(Vector3.Distance(mousePosition, initialPosition) > movementRadius)
			{
				mousePosition = (mousePosition - initialPosition).normalized * movementRadius + initialPosition;
			}
			
			transform.position = Vector3.Lerp (transform.position, mousePosition, Time.deltaTime * birdFollowSpeed);
		}
 		
		if(Input.GetMouseButtonUp(0) && didTouchBird)
		{
	        deltaMovement = transform.position - initialPosition;	
			
			animator.SetBool("hurled", true);	
			
			// The bird starts with no gravity, so we must set it
			rigidbody2D.gravityScale = 0.5f;
			rigidbody2D.AddForce(new Vector2(-deltaMovement.x * mouseSensitivity * 1.5f, 
											 -deltaMovement.y * mouseSensitivity));
			
			_didHurled = true;
		}
		
		// The bird must blink randomly if he is in idle state
		blinkTimer += Time.deltaTime;
		
		if(blinkTimer >= nextBlinkTime)
		{
			animator.SetBool("blink", true);		
			
			nextBlinkTime = Random.Range(0.5f, 4.0f);
			blinkTimer = 0.0f;	
		}
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.transform.tag == "Block" || collision.transform.tag == "Ground")
		{
			_didHurled = false;
		}
	}
}
