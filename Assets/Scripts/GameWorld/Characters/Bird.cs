using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/** \class Bird
 *  \brief  Management of Bird Characters of game
 *
 *  Contains creation and deletion methods for the Bird Character
 *  as well as movement methods, collision, triggers, particles and actions
 */
public class Bird : Character {
    /**index of next particle to be added on trajectory*/
    private int _nextParticleTrajectory;

    /**Radius of drag*/
    public float _dragRadius = 1.0f;
    /**Speed of drag*/
    public float _dragSpeed = 1.0f;
    /**Bird's initial gravity*/
    public float _launchGravity = 1.0f;
    /**Frequency which to drop particles during flight*/
    public float _trajectoryParticleFrequency = 0.5f;
    /**Jump Force for idle animation*/
    public float _jumpForce;
    /**Max delay time to next jump*/
    public float _maxTimeToJump;

    /**X and Y force of launch*/
    public Vector2   _launchForce;
    
    /**Array of Game Objects with trajectory particles.*/
    public GameObject[] _trajectoryParticlesTemplates;

    /**True if bird is selected to be shot, false otherwise*/
	public bool IsSelected{ get; set;}
    /**True if bird is already flying, false otherwise*/
	public bool IsFlying{ get; set;}
    /**Boolean to check conditions if Bird on slingshot*/
    public bool JumpToSlingshot{ get; set; }
    /**True if flying out of sling shot, false otherwise*/
    public bool OutOfSlingShot{ get; set; }
	
    /**
     *  When script instance is being loaded, calls parent Awake, calculates next jump delay,
     *  Invokes next idle jump, Disables collision against blocks to avoid early collisions
     */
	public override void Awake ()
    {
		base.Awake();

        GameWorld.Instance._slingshotBaseTransform.gameObject.SetActive(false);

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
		
		// Disable collision agains blocks to avoid early collisions
		int birdsLayer = LayerMask.NameToLayer("Birds");
		int blocksLayer = LayerMask.NameToLayer("Blocks");
		
		Physics2D.IgnoreLayerCollision(birdsLayer, blocksLayer, true);
    }
    /**
     *  At every update if Bird is flying and out of the slingshot, drag it. If is in front of slingshot
     *  enables collision against blocks.
     */
    void Update()
    {
        if(IsFlying && !OutOfSlingShot)
            DragBird(transform.position);
		
		if(IsInFrontOfSlingshot())
		{
			// Enabling collision agains blocks
			int birdsLayer = LayerMask.NameToLayer("Birds");
			int blocksLayer = LayerMask.NameToLayer("Blocks");
			
			Physics2D.IgnoreLayerCollision(birdsLayer, blocksLayer, false);
		}
    }

    /**
     *  If idle and with a gravity scale greater than 0, does an idle jump, plays audio
     *  of jump randomly. Also calculates delay untill next jump
     */
    void IdleJump()
    {
        if(JumpToSlingshot)
            return;

        if(IsIdle() && GetComponent<Rigidbody2D>().gravityScale > 0f) {
			GetComponent<Rigidbody2D>().AddForce(Vector2.up * _jumpForce);
				if(Random.value < 0.5f)
					GetComponent<AudioSource>().PlayOneShot(_clips[Random.Range(4, 6)]);
		}

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
    }
    /**
     *  Drops particle forming the trajectory of the bird
     */
    void DropTrajectoryParticle()
    {
		_nextParticleTrajectory = (_nextParticleTrajectory + 1) % _trajectoryParticlesTemplates.Length;
		GameWorld.Instance.AddTrajectoryParticle(_trajectoryParticlesTemplates[_nextParticleTrajectory], transform.position, name);
    }
    /**
     *  Kills Bird Game Object
     */
	public override void Die()
	{
		GameWorld.Instance.KillBird(this);
		base.Die();
	}
    /**
     *  Called when incoming collider makes contact with Bird collider
     *  If is out of slingshot (flying) stops the flight, stops dropping trajectory particles and starts dying
     *  @param[in]  collision   Collision2D object containing information about the collision in 2D physics
     */
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(OutOfSlingShot)
        {
			IsFlying = false;

            CancelInvoke("DropTrajectoryParticle");
			GameWorld.Instance.RemoveLastTrajectoryParticle(name);

            Invoke("Die", _timeToDie);
            _animator.Play("die", 0, 0f);
        }
    }
    /**
     *  When another object enters Birds trigger collider,
     *  if collider is slingshot and bird is not thrown yet, deactivates it.
     *  If Bird is selected and is flying, plays sound of bird being shot with slingshot.
     *  @param[in]  collider    collider type used in 2D gameplay
     */
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot")
        {
            if(JumpToSlingshot)
				GameWorld.Instance._slingshotBaseTransform.gameObject.SetActive(false);

			if(IsSelected && IsFlying)
				GetComponent<AudioSource>().PlayOneShot(_clips[3]);
        }
    }
    /**
     *  Whenever another object is within a trigger collider attached to this object.
     *  if is slingshot and is in state JumpToSlingshot, deactivates.
     *  if is in flying state turns OutOfSlingShot true and moves slingshot z position
     *  @param[in]  collider    collider type used in 2D gameplay
     */
    void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot")
        {
            if(JumpToSlingshot)
				GameWorld.Instance._slingshotBaseTransform.gameObject.SetActive(false);

            if(IsFlying)
            {
				OutOfSlingShot = true;

                Vector3 slingBasePos = GameWorld.Instance._slingSelectPos;
                slingBasePos.z = transform.position.z + 0.5f;
                GameWorld.Instance._slingshotBaseTransform.transform.position = slingBasePos;
                GameWorld.Instance. _slingshotBaseTransform.transform.rotation = Quaternion.Euler(GameWorld.Instance._slingshotBaseTransform.transform.rotation.x,
                                                                                         GameWorld.Instance._slingshotBaseTransform.transform.rotation.y, 0f);
            }
        }
    }
    /**
     *  When another object leaves a trigger collider attached to Bird
     *  if collide is slingshot, and if Bird is the selected one and
     *  is not flying, play audio file for stretching slingshot.
     *  @param[in]  collider    collider type used in 2D gameplay
     */
    void OnTriggerExit2D(Collider2D collider)
	{
		if(collider.tag == "Slingshot")
		{
			if(IsSelected && !IsFlying)
				GetComponent<AudioSource>().PlayOneShot(_clips[2]);
		}
	}
    /**
     *  Calculates if Bird is in front of the slingshot
     *  @return bool    true if in front of slingshot, false otherwise
     */
	public bool IsInFrontOfSlingshot()
	{
		return transform.position.x + GetComponent<Collider2D>().bounds.size.x > GameWorld.Instance._slingSelectPos.x + _dragRadius;
	}
	/**
     *  Selects the bird, changin IsSelected to true, playing the selected animation and setting to active
     *  the slingshot base transform for collision
     */
    public void SelectBird()
    {
		IsSelected = true;
        _animator.Play("selected", 0, 0f);

		GameWorld.Instance._slingshotBaseTransform.gameObject.SetActive(true);
    }
    /**
     *  Set bird on slingshot, moving it to the right position, and if distance from slingshot is lesser or equal to 0
     *  changes JumpToSlingshot and OutOfSlingShot to false and velocity to 0.
     */
    public void SetBirdOnSlingshot()
    {
		transform.position = Vector3.MoveTowards(transform.position, GameWorld.Instance._slingSelectPos, _dragSpeed * Time.deltaTime);

		if(Vector3.Distance(transform.position, GameWorld.Instance._slingSelectPos) <= 0f)
		{
			JumpToSlingshot = false;
			OutOfSlingShot = false;
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		}
    }
    /**
     *  Drags bird along the slingshot, respecting all the conditions of moving the bird inside the slingshot's band
     *  Locking it in a circle, making the slingshot base face the slingshot, and making the slingshot base
     *  Rotate around the slingshot
     *  @param[in]  dragPosition    Vector3 (X, Y, Z) containing the position which to drag the Bird in the slingshot
     */
	public void DragBird(Vector3 dragPosition)
	{		
		if (float.IsNaN(dragPosition.x) || float.IsNaN(dragPosition.y))
			return;
			
		dragPosition.z = transform.position.z;
		float deltaPosFromSlingshot = Vector2.Distance(dragPosition, GameWorld.Instance._slingSelectPos);

        // Lock bird movement inside a circle
        if(deltaPosFromSlingshot > _dragRadius)
			dragPosition = (dragPosition - GameWorld.Instance._slingSelectPos).normalized * _dragRadius + GameWorld.Instance._slingSelectPos;
		
		Vector3 velocity = Vector3.zero;
		transform.position = Vector3.SmoothDamp(transform.position, dragPosition, ref velocity, 0.05f);
		
		// Slingshot base look to slingshot
		Vector3 dist = GameWorld.Instance._slingshotBaseTransform.transform.position - GameWorld.Instance._slingSelectPos;
        float angle = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
        GameWorld.Instance._slingshotBaseTransform.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Slingshot base rotate around the selected point
		Collider2D col = GetComponent<Collider2D>();
		GameWorld.Instance._slingshotBaseTransform.transform.position = (transform.position - GameWorld.Instance._slingSelectPos).normalized 
			* col.bounds.size.x/2.25f + transform.position;
	}
    /**
     *  Launches the Bird, changing IsSelected to false, calculating distance from slingshot, 
     *  playing the flying animation, changing IsFlying to true, setting the Bird's gravity and,
     *  if not on Simulation, dropping the trajectory particles.
     *  
     */
    public void LaunchBird()
	{
		IsSelected = false;
	
		Vector2 deltaPosFromSlingshot = (transform.position - GameWorld.Instance._slingSelectPos);
		_animator.Play("flying", 0, 0f);

		IsFlying = true;
				
		// The bird starts with no gravity, so we must set it
		GetComponent<Rigidbody2D>().gravityScale = _launchGravity;
		GetComponent<Rigidbody2D>().AddForce(new Vector2(_launchForce.x * -deltaPosFromSlingshot.x,
		                                 _launchForce.y * -deltaPosFromSlingshot.y), ForceMode2D.Impulse);

		if(!GameWorld.Instance._isSimulation)
        	InvokeRepeating("DropTrajectoryParticle", 0.1f,
		                _trajectoryParticleFrequency / Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x));
	}
}
