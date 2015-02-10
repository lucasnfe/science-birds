using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bird : Character {

    private int _nextParticleTrajectory;

    public float _dragRadius = 1.0f;
    public float _dragSpeed = 1.0f;
    public float _launchGravity = 1.0f;
    public float _trajectoryParticleFrequency = 0.5f;
    public float _jumpForce;
    public float _maxTimeToJump;

    public Vector2   _launchForce;
    
    public GameObject[] _trajectoryParticlesTemplates;

	public bool IsSelected{ get; set;}
	public bool IsFlying{ get; set;}
    public bool JumpToSlingshot{ get; set; }
    public bool OutOfSlingShot{ get; set; }
	
	public override void Awake ()
    {
		base.Awake();

        GameWorld.Instance._slingshotBase.gameObject.SetActive(false);

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
    }

    void Update()
    {
        if(IsFlying && !OutOfSlingShot)
            DragBird(transform.position);
    }

    void IdleJump()
    {
        if(JumpToSlingshot)
            return;

        if(IsIdle() && rigidbody2D.gravityScale > 0f)

            rigidbody2D.AddForce(Vector2.up * _jumpForce);

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
    }

    void DropTrajectoryParticle()
    {
		_nextParticleTrajectory = (_nextParticleTrajectory + 1) % _trajectoryParticlesTemplates.Length;
		GameWorld.Instance.AddTrajectoryParticle(_trajectoryParticlesTemplates[_nextParticleTrajectory], transform.position, name);
    }

	public override void Die()
	{
		GameWorld.Instance.KillBird(this);
		base.Die();
	}

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

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot")
        {
            if(JumpToSlingshot)
				GameWorld.Instance._slingshotBase.gameObject.SetActive(false);

			if(IsSelected && IsFlying)
				PlayAudio(3);
        }
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot")
        {
            if(JumpToSlingshot)
				GameWorld.Instance._slingshotBase.gameObject.SetActive(false);

            if(IsFlying)
            {
				OutOfSlingShot = true;

                Vector3 slingBasePos = GameWorld.Instance._slingSelectPos;
                slingBasePos.z = transform.position.z + 0.5f;
                GameWorld.Instance._slingshotBase.transform.position = slingBasePos;
                GameWorld.Instance. _slingshotBase.transform.rotation = Quaternion.Euler(GameWorld.Instance._slingshotBase.transform.rotation.x,
                                                                                         GameWorld.Instance._slingshotBase.transform.rotation.y, 0f);
            }
        }
    }

	void OnTriggerExit2D(Collider2D collider)
	{
		if(collider.tag == "Slingshot")
		{
			if(IsSelected && !IsFlying)
				PlayAudio(2);
		}
	}

	public bool IsInFrontOfSlingshot()
	{
		return transform.position.x > GameWorld.Instance._slingSelectPos.x + _dragRadius;
	}
	
    public void SelectBird()
    {
		IsSelected = true;
        _animator.Play("selected", 0, 0f);

		GameWorld.Instance._slingshotBase.gameObject.SetActive(true);
    }

    public void SetBirdOnSlingshot()
    {
		transform.position = Vector3.MoveTowards(transform.position, GameWorld.Instance._slingSelectPos, _dragSpeed * Time.deltaTime);

		if(Vector3.Distance(transform.position, GameWorld.Instance._slingSelectPos) < 0.1f)
		{
			JumpToSlingshot = false;
			OutOfSlingShot = false;
			rigidbody2D.velocity = Vector2.zero;
		}
    }

	public void DragBird(Vector3 dragPosition)
	{
		dragPosition.z = transform.position.z;
		float deltaPosFromSlingshot = Vector2.Distance(dragPosition, GameWorld.Instance._slingSelectPos);

        // Lock bird movement inside a circle
        if(deltaPosFromSlingshot > _dragRadius)
			dragPosition = (dragPosition - GameWorld.Instance._slingSelectPos).normalized * _dragRadius + GameWorld.Instance._slingSelectPos;

        transform.position = Vector3.Lerp (transform.position, dragPosition, Time.deltaTime * _dragSpeed);

		// Slingshot base look to slingshot
		Vector3 dist = GameWorld.Instance._slingshotBase.transform.position - GameWorld.Instance._slingSelectPos;
        float angle = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
        GameWorld.Instance._slingshotBase.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Slingshot base rotate around the selected point
		Collider2D col = GetComponent<Collider2D>();
		GameWorld.Instance._slingshotBase.transform.position = (transform.position - GameWorld.Instance._slingSelectPos).normalized 
			* col.bounds.size.x/2.25f + transform.position;
	}

	public void LaunchBird()
	{
		IsSelected = false;
	
		Vector2 deltaPosFromSlingshot = transform.position - GameWorld.Instance._slingSelectPos;
		_animator.Play("flying", 0, 0f);

		IsFlying = true;
	
		// The bird starts with no gravity, so we must set it
		rigidbody2D.gravityScale = _launchGravity;
		rigidbody2D.velocity = new Vector2(_launchForce.x * -deltaPosFromSlingshot.x,
                                           _launchForce.y * -deltaPosFromSlingshot.y) * Time.fixedDeltaTime;

		if(!GameWorld.Instance._isSimulation)
        	InvokeRepeating("DropTrajectoryParticle", 0.1f,
		                _trajectoryParticleFrequency / Mathf.Abs(rigidbody2D.velocity.x));
	}
}
