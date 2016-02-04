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

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
		
		// Disable collision agains blocks to avoid early collisions
		int birdsLayer = LayerMask.NameToLayer("Birds");
		int blocksLayer = LayerMask.NameToLayer("Blocks");
		
		Physics2D.IgnoreLayerCollision(birdsLayer, blocksLayer, true);
    }

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
				GameWorld.Instance.SetSlingshotBaseActive(false);

			if(IsSelected && IsFlying)
				GetComponent<AudioSource>().PlayOneShot(_clips[3]);
        }
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot")
        {
            if(JumpToSlingshot)
				GameWorld.Instance.SetSlingshotBaseActive(false);

            if(IsFlying)
            {
				OutOfSlingShot = true;

                Vector3 slingBasePos = GameWorld.Instance.SlingSelectPos;
                slingBasePos.z = transform.position.z + 0.5f;

				GameWorld.Instance.ChangeSlingshotBasePosition(slingBasePos);
				GameWorld.Instance.ChangeSlingshotBaseRotation (Quaternion.identity);
            }
        }
    }

	void OnTriggerExit2D(Collider2D collider)
	{
		if(collider.tag == "Slingshot")
		{
			if(IsSelected && !IsFlying)
				GetComponent<AudioSource>().PlayOneShot(_clips[2]);
		}
	}

	public bool IsInFrontOfSlingshot()
	{
		return transform.position.x + GetComponent<Collider2D>().bounds.size.x > GameWorld.Instance.SlingSelectPos.x + _dragRadius;
	}
	
    public void SelectBird()
    {
		IsSelected = true;
        _animator.Play("selected", 0, 0f);

		GameWorld.Instance.SetSlingshotBaseActive(true);
    }

    public void SetBirdOnSlingshot()
    {
		transform.position = Vector3.MoveTowards(transform.position, GameWorld.Instance.SlingSelectPos, _dragSpeed * Time.deltaTime);

		if(Vector3.Distance(transform.position, GameWorld.Instance.SlingSelectPos) <= 0f)
		{
			JumpToSlingshot = false;
			OutOfSlingShot = false;
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		}
    }

	public void DragBird(Vector3 dragPosition)
	{		
		if (float.IsNaN(dragPosition.x) || float.IsNaN(dragPosition.y))
			return;
			
		dragPosition.z = transform.position.z;
		float deltaPosFromSlingshot = Vector2.Distance(dragPosition, GameWorld.Instance.SlingSelectPos);

        // Lock bird movement inside a circle
        if(deltaPosFromSlingshot > _dragRadius)
			dragPosition = (dragPosition - GameWorld.Instance.SlingSelectPos).normalized * _dragRadius + GameWorld.Instance.SlingSelectPos;
		
		Vector3 velocity = Vector3.zero;
		transform.position = Vector3.SmoothDamp(transform.position, dragPosition, ref velocity, 0.05f);
		
		// Slingshot base look to slingshot
		Vector3 dist = GameWorld.Instance.DragDistance();
        float angle = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
		GameWorld.Instance.ChangeSlingshotBaseRotation(Quaternion.AngleAxis(angle, Vector3.forward));

        // Slingshot base rotate around the selected point
		Collider2D col = GetComponent<Collider2D>();
		GameWorld.Instance.ChangeSlingshotBasePosition ((transform.position - GameWorld.Instance.SlingSelectPos).normalized 
			* col.bounds.size.x / 2.25f + transform.position);
	}

	public void LaunchBird()
	{
		IsSelected = false;
	
		Vector2 deltaPosFromSlingshot = (transform.position - GameWorld.Instance.SlingSelectPos);
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
