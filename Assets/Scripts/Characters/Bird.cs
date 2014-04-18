using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bird : MonoBehaviour {
	
	
	// Attributes 
    private static float X_OFFSET = 0.5f;
    private static float Y_OFFSET = 0.65f;
    private static float BOUND = 0.1f;
//    private static float STRETCH = 0.4f;
//    private static int X_MAX = 640;
	
    //10,    15,    20,    25,    30,    35,    40,    45,    50,    55,    60,    65,    70
    private static float[] _launchAngle = {0.13f,  0.215f, 0.296f, 0.381f, 0.476f, 0.567f, 0.657f, 0.741f, 0.832f, 0.924f, 1.014f, 1.106f, 1.197f};
    private static float[] _changeAngle = {0.052f, 0.057f, 0.063f, 0.066f, 0.056f, 0.054f, 0.050f, 0.053f, 0.042f, 0.038f, 0.034f, 0.029f, 0.025f};
    private static float[] _launchVelocity = {2.9f, 2.88f,  2.866f, 2.838f, 2.810f, 2.800f, 2.790f, 2.773f, 2.763f, 2.745f, 2.74f, 2.735f, 2.73f};
	
    // small modification to the scale and angle
    private float _scaleFactor = 1.005f;
	
	public AudioClip[] _clips;
	public GameObject[] _trajectoryParticles;
	public GameObject _catapult;
	
	public float mouseSensitivity = 1.0f;
	public float birdFollowSpeed = 1.0f;
	public float movementRadius = 1.0f;
	public float stretchedRadius = 0.3f;
		
	private bool _isMainBird;
	public bool isMainBird() { return _isMainBird; } 
	public void isMainBird(bool isMainBird) 
	{
		rigidbody2D.velocity = Vector2.zero;
		
		if(isMainBird)
		{
			rigidbody2D.gravityScale = 0.0f;
		}
		else
		{
			rigidbody2D.gravityScale = 0.5f;
		}
		
		_isMainBird = isMainBird;
	}
	
	// Timers
	private float _blinkTimer;
	private float _jumpTimer;
	private float _pressedTimer;
	private float _stretchedTimer;
	private float _trajectoryTimer;
	private float _dieTimer;
	private float _dragTimer;
	
	// Times to call events
	private float _nextPressedBlinkTime = 1.0f;
	private float _nextJumpTime = 1.0f;
	
	public float stretchedTime = 1.0f;
	public float tractoryParticleTime = 1.0f;
	public float dieTime = 1.0f;
	
	private Vector3 _initialPosition;
	private Vector3 _dragPosition;
	
	private Animator _animator;
	private CircleCollider2D _collider;
	
	private bool _didHurled;
    public bool didHurled()
	{
		return _didHurled;
	}
	
	public bool dragBird { get; set; }
	private bool _didTouchBird;
		
	private float _dragSpeed;
	private float _releaseTime;

	// Use this for initialization
	void Start () {
	
		_nextPressedBlinkTime = Random.Range(0.5f, 4.0f);
		_nextJumpTime = Random.Range(1.0f, 4.0f);
		
		_stretchedTimer = stretchedTime;
				
	    _animator = GetComponent<Animator>();		
		_collider = GetComponent<CircleCollider2D>();
	}
	
	void DidTouchBird()
	{
		_initialPosition = transform.position;
		
		_didTouchBird = true;
		
		_animator.SetBool("pressed", true);
		audio.PlayOneShot(_clips[0], 1.0f);
	}
		
	void PressedBlink()
	{
		_pressedTimer += Time.deltaTime;
	
		if(_pressedTimer >= _nextPressedBlinkTime)
		{
			_animator.SetBool("pressed_blink", true);		
		
			_nextPressedBlinkTime = Random.Range(0.5f, 4.0f);
			_pressedTimer = 0.0f;	
		}
	}
	
	void IdleBlink()
	{
		_blinkTimer += Time.deltaTime;
		
		if(_blinkTimer >= _nextPressedBlinkTime)
		{
			_animator.SetBool("blink", true);		
			
			_nextPressedBlinkTime = Random.Range(0.5f, 4.0f);
			_blinkTimer = 0.0f;	
		}
	}
	
	void IdleJump()
	{
		_jumpTimer += Time.deltaTime;
		
		if(_jumpTimer >= _nextJumpTime)
		{
			float randomJumpForce = Random.Range(20.0f, 50.0f);

			rigidbody2D.AddForce(new Vector2(0.0f, randomJumpForce));
			
			_nextJumpTime = Random.Range(1.0f, 4.0f);
			_jumpTimer = 0.0f;	
		}
	}
	
	void PlayStretchAudio()
	{
		_stretchedTimer += Time.deltaTime;
		
		if(_stretchedTimer >= stretchedTime)
		{
			audio.PlayOneShot(_clips[2], 1.0f);
			_stretchedTimer = 0.0f;
		}
	}
		
	// Update is called once per frame
	void Update () {
		
	    if (Input.GetMouseButtonDown(0) && !dragBird && _isMainBird)
	    {
	        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

	        if(_collider.OverlapPoint(mousePosition))
			{
				DidTouchBird();
			}
			else
			{
				_didTouchBird = false;
			}
	    }
		
		if((Input.GetMouseButton(0) || dragBird) && _didTouchBird)
		{
			// The bird must blink randomly if he is being pressed
			PressedBlink();
			
			// If drag position came from other controller, don't get it from Input
			if(!dragBird)
			{
				_dragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			}
			
			_dragPosition = new Vector3(_dragPosition.x, _dragPosition.y, transform.position.z);
			
			float distanceFromBirdToCatapult = Vector3.Distance(_dragPosition, _initialPosition);

			if(distanceFromBirdToCatapult >= stretchedRadius)
			{
				PlayStretchAudio();
			}
			
			// Lock bird movement inside a circle
			if(distanceFromBirdToCatapult > movementRadius)
			{
				_dragPosition = (_dragPosition - _initialPosition).normalized * movementRadius + _initialPosition;
			}
			
			float followSpeed = birdFollowSpeed;
			
			if(dragBird)
			{
				followSpeed = _dragSpeed;
			}
			
			transform.position = Vector3.Lerp (transform.position, _dragPosition, Time.deltaTime * followSpeed);
		}
 		
		if(Input.GetMouseButtonUp(0) && !dragBird && _didTouchBird)
		{
			LaunchBird(transform.position - _initialPosition, 1.5f);
		}
				
		// The bird must blink randomly if he is in idle state
		IdleBlink();
		
		if(!_isMainBird)
		{
			IdleJump();
		}
		
		if(_isMainBird && _animator.GetBool("hurt"))
		{
			_dieTimer += Time.deltaTime;
			
			// Time to kill the bird
			if(_dieTimer >= dieTime)
			{
				transform.parent.audio.Play();
				
				_dieTimer = 0.0f;
				
				Destroy (gameObject);
			}
		}
		
		if(_isMainBird && dragBird)
		{
			_dragTimer += Time.deltaTime;
			
			if(_dragTimer >= _releaseTime)
			{
				LaunchBird(transform.position - _initialPosition, 1.5f);
				
				_dragTimer = 0.0f;
				dragBird = false;
			}
		}
	}
	
	void FixedUpdate()
	{
		// If bird was launched, create particles in the air for his movement
		if(_didHurled)
		{
			_trajectoryTimer += Time.deltaTime;
			
			if(_trajectoryTimer >= tractoryParticleTime)
			{
				int nextParticle = Random.Range(0, _trajectoryParticles.Length);
				Instantiate(_trajectoryParticles[nextParticle], transform.position, Quaternion.identity);
				
				_trajectoryTimer = 0.0f;
			}
		}
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		_animator.SetBool("hurt", true);
		_didHurled = false;
		
		if(collision.transform.tag == "Block")
		{
			audio.PlayOneShot(_clips[3], 1.0f);
		}
	}
	
	void DragBird(Vector3 nextPosition, float speed, float releaseTime)
	{
		if(!_didTouchBird)
		{
			DidTouchBird();
		
			dragBird = true;
			_dragPosition = nextPosition;
			_dragSpeed = speed;
			_releaseTime = releaseTime;
		}
	}
	
	void LaunchBird(Vector2 deltaPosFromSlingshot, float speed)
	{		
		_trajectoryTimer = 0.0f;
		_didHurled = true;
		
		_animator.SetBool("hurled", true);	
		audio.PlayOneShot(_clips[1], 1.0f);
		
		// The bird starts with no gravity, so we must set it
		rigidbody2D.gravityScale = 0.2f;
		rigidbody2D.AddForce(new Vector2(-deltaPosFromSlingshot.x * mouseSensitivity * speed, 
										 -deltaPosFromSlingshot.y * mouseSensitivity));
	}    
	
    // return scene scale determined by the sling size
    private float getSceneScale(Rect sling)
    {
        return sling.height + sling.width;
    }
	
    // find the reference point given the sling
    public Vector2 getReferencePoint(Rect sling)
    {
        Vector2 p = new Vector2((int)(sling.x + X_OFFSET * sling.width), 
							    (int)(sling.y + Y_OFFSET * sling.width));
        return p;
    }
	
    // take the initial angle of the desired trajectory and return the launch angle required
    private float actualToLaunch(float theta)
    {
        for (int i = 1; i < _launchAngle.Length; i++)
        {
            if (theta > _launchAngle[i-1] && theta < _launchAngle[i])
                return theta + _changeAngle[i-1];
        }
		
        return theta + _changeAngle[_launchAngle.Length-1];
    }
	
    // get the velocity for the desired angle
    public float getVelocity(float theta)
    {
        if (theta < _launchAngle[0])    
            return _scaleFactor * _launchVelocity[0];
        
        for (int i = 1; i < _launchAngle.Length; i++)
        {
            if (theta < _launchAngle[i])
                return _scaleFactor * _launchVelocity[i-1];
        }
       
        return _scaleFactor * _launchVelocity[_launchVelocity.Length-1];
    }
	
   /* find the release point given the sling location and launch angle, using maximum velocity
    *
    * @param   sling - bounding rectangle of the slingshot
    *          theta - launch angle in radians (anticlockwise from positive direction of the x-axis)
    * @return  the release point on screen
    */
   public Vector2 findReleasePoint(Rect sling, float theta)
   {
       float mag = sling.height * 10.0f;
       Vector2 refPoint = getReferencePoint(sling);
       Vector2 release = new Vector2((int)(refPoint.x - mag * Mathf.Cos(theta)), 
		                             (int)(refPoint.y + mag * Mathf.Sin(theta)));
       
       return release;
   }
   
	
	/* Estimate launch points given a desired target point using maximum velocity
     * If there are two launch point for the target, they are both returned in
     * the list {lower point, higher point)
     * Note - angles greater than 75 are not considered
     *
     * @param   slingshot - bounding rectangle of the slingshot
     *          targetPoint - coordinates of the target to hit
     * @return  A list containing 2 possible release points
     */
    public List<Vector2> estimateLaunchPoint(Rect slingshot, Vector2 targetPoint) {
        
        // calculate relative position of the target (normalised)
        float scale = getSceneScale(slingshot);
        
		//System.out.println("scale " + scale);
        Vector2 refPoint = getReferencePoint(slingshot);
            
        float x = (targetPoint.x - refPoint.x) / scale;
        float y = -(targetPoint.y - refPoint.y) / scale;
        
        float bestError = 1000;
        float theta1 = 0;
        float theta2 = 0;
        
        // first estimate launch angle using the projectile equation (constant velocity)
        float v = _scaleFactor * _launchVelocity[6];
        float v2 = v * v;
        float v4 = v2 * v2;
        float tangent1 = (v2 - Mathf.Sqrt(v4 - (x * x + 2 * y * v2))) / x;
        float tangent2 = (v2 + Mathf.Sqrt(v4 - (x * x + 2 * y * v2))) / x;

        float t1 = actualToLaunch(Mathf.Atan(tangent1));
        float t2 = actualToLaunch(Mathf.Atan(tangent2));
		        
        // search angles in range [t1 - BOUND, t1 + BOUND]
        for (float theta = t1 - BOUND; theta <= t1 + BOUND; theta += 0.001f)
        {
            float velocity = getVelocity(theta);
            
            // initial velocities
            float u_x = velocity * Mathf.Cos(theta);
            float u_y = velocity * Mathf.Sin(theta);
            
            // the normalised coefficients
            float a = -0.5f / (u_x * u_x);
            float b = u_y / u_x;
            
            // the error in y-coordinate
            float error = Mathf.Abs(a*x*x + b*x - y);
            if (error < bestError)
            {
                theta1 = theta;
                bestError = error;
            }
        }
        
        bestError = 1000;
        
        // search angles in range [t2 - BOUND, t2 + BOUND]
        for (float theta = t2 - BOUND; theta <= t2 + BOUND; theta += 0.001f)
        {
            float velocity = getVelocity(theta);
            
            // initial velocities
            float u_x = velocity * Mathf.Cos(theta);
            float u_y = velocity * Mathf.Sin(theta);
            
            // the normalised coefficients
            float a = -0.5f / (u_x * u_x);
            float b = u_y / u_x;
            
            // the error in y-coordinate
            float error = Mathf.Abs(a*x*x + b*x - y);
            if (error < bestError)
            {
                theta2 = theta;
                bestError = error;
            }
        }
        
        theta1 = actualToLaunch(theta1);
        theta2 = actualToLaunch(theta2);
        
        //System.out.println("Two angles: " + Math.toDegrees(theta1) + ", " + Math.toDegrees(theta2));
            
        // add launch points to the list
        List<Vector2> pts = new List<Vector2>();
        pts.Add(findReleasePoint(slingshot, theta1));
        
        // add the higher point if it is below 75 degrees and not same as first
        if (theta2 < (Mathf.Deg2Rad * 75.0f) && theta2 != theta1)
            pts.Add(findReleasePoint(slingshot, theta2));
        
        return pts;
    }
	
	public void TryKillPig(Rect slingshot, Vector2 pigPosition)
	{
		// Converting coordinates
		slingshot.y += Screen.height; 
		pigPosition.y += Screen.height;
		
		//Debug.Log("slingshot: " + slingshot);
		//Debug.Log("pigPosition: " + pigPosition);
		
		List<Vector2> points = estimateLaunchPoint(slingshot, pigPosition);
			
		Vector3 slingShotPos = new Vector3(slingshot.x, slingshot.y, 0);
		Vector3 deltaMovement = slingShotPos - new Vector3(points[0].x, points[0].y, 0) ;
		
		deltaMovement *= -1;
		//Debug.Log("Delta Move: " + Camera.main.ScreenToWorldPoint(deltaMovement));
		
		DragBird(Camera.main.ScreenToWorldPoint(deltaMovement), 1.5f, 2f);
	}
}
