using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ABBird : ABCharacter {

    public float _dragRadius = 1.0f;
    public float _dragSpeed = 1.0f;
    public float _launchGravity = 1.0f;
    public float _trajectoryParticleFrequency = 0.5f;
    public float _jumpForce;
    public float _maxTimeToJump;

    public Vector2   _launchForce;
    
    public GameObject[] _trajectoryParticlesTemplates;

	public bool IsSelected      { get; set; }
	public bool IsFlying        { get; set; }
    public bool JumpToSlingshot { get; set; }
    public bool OutOfSlingShot  { get; set; }

	protected override void Awake ()
    {
		base.Awake();

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
		
		// Disable collision agains blocks to avoid early collisions
		int birdsLayer = LayerMask.NameToLayer("Birds");
		int blocksLayer = LayerMask.NameToLayer("Blocks");
		
		Physics2D.IgnoreLayerCollision(birdsLayer, blocksLayer, true);
    }

	protected override void Update()
    {
		base.Update ();

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

        if(IsIdle() && _rigidBody.gravityScale > 0f) {
			_rigidBody.AddForce(Vector2.up * _jumpForce);
				if(Random.value < 0.5f)
					_audioSource.PlayOneShot(_clips[Random.Range(4, 6)]);
		}

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
    }

	private void CheckVelocityToDie() {

		if (_rigidBody.velocity.magnitude < 0.005f) {

			CancelInvoke ();
			Die ();
		}
	}

	public override void Die()
	{
			ABGameWorld.Instance.KillBird (this);
			base.Die ();
	}

    public override void OnCollisionEnter2D(Collision2D collision)
    {
		if(OutOfSlingShot && !IsDying)
        {
			IsFlying = false;

			_destroyEffect._shootParticles = false;

			ABGameWorld.Instance.RemoveLastTrajectoryParticle ();

			foreach (ABParticle part in _destroyEffect.GetUsedParticles())
				ABGameWorld.Instance.AddTrajectoryParticle (part);

			InvokeRepeating("CheckVelocityToDie", 0f, _timeToDie);
			_animator.Play("die", 0, 0f);

			IsDying = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot")
        {
            if(JumpToSlingshot)
				ABGameWorld.Instance.SetSlingshotBaseActive(false);

			if(IsSelected && IsFlying)
				_audioSource.PlayOneShot(_clips[3]);
        }
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot")
        {
            if(JumpToSlingshot)
				ABGameWorld.Instance.SetSlingshotBaseActive(false);

            if(IsFlying)
            {
				OutOfSlingShot = true;

				Vector3 slingBasePos = ABConstants.SLING_SELECT_POS;
                slingBasePos.z = transform.position.z + 0.5f;

				ABGameWorld.Instance.ChangeSlingshotBasePosition(slingBasePos);
				ABGameWorld.Instance.ChangeSlingshotBaseRotation (Quaternion.identity);
            }
        }
    }

	void OnTriggerExit2D(Collider2D collider)
	{
		if(collider.tag == "Slingshot")
		{
			if(IsSelected && !IsFlying)
				_audioSource.PlayOneShot(_clips[2]);

			if(!IsSelected && IsFlying)
				_audioSource.PlayOneShot(_clips[0]);
		}
	}

	public bool IsInFrontOfSlingshot()
	{
		return transform.position.x + _collider.bounds.size.x > ABConstants.SLING_SELECT_POS.x + _dragRadius;
	}
	
    public void SelectBird()
    {
		IsSelected = true;

		_audioSource.PlayOneShot (_clips[1]);
        _animator.Play("selected", 0, 0f);

		ABGameWorld.Instance.SetSlingshotBaseActive(true);
    }

    public void SetBirdOnSlingshot()
    {
		transform.position = Vector3.MoveTowards(transform.position, ABConstants.SLING_SELECT_POS, _dragSpeed * Time.deltaTime);

		if(Vector3.Distance(transform.position, ABConstants.SLING_SELECT_POS) <= 0f)
		{
			JumpToSlingshot = false;
			OutOfSlingShot = false;
			_rigidBody.velocity = Vector2.zero;
		}
    }

	public void DragBird(Vector3 dragPosition)
	{		
		if (float.IsNaN(dragPosition.x) || float.IsNaN(dragPosition.y))
			return;
			
		dragPosition.z = transform.position.z;
		float deltaPosFromSlingshot = Vector2.Distance(dragPosition, ABConstants.SLING_SELECT_POS);

        // Lock bird movement inside a circle
        if(deltaPosFromSlingshot > _dragRadius)
			dragPosition = (dragPosition - ABConstants.SLING_SELECT_POS).normalized * _dragRadius + ABConstants.SLING_SELECT_POS;
		
		transform.position = Vector3.Lerp(transform.position, dragPosition, _dragSpeed * Time.deltaTime);
		
		// Slingshot base look to slingshot
		Vector3 dist = ABGameWorld.Instance.DragDistance();
        float angle = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
		ABGameWorld.Instance.ChangeSlingshotBaseRotation(Quaternion.AngleAxis(angle, Vector3.forward));

        // Slingshot base rotate around the selected point
		Collider2D col = _collider;
		ABGameWorld.Instance.ChangeSlingshotBasePosition ((transform.position - ABConstants.SLING_SELECT_POS).normalized 
			* col.bounds.size.x / 2.25f + transform.position);
	}

	public void LaunchBird()
	{
		IsSelected = false;
	
		Vector2 deltaPosFromSlingshot = (transform.position - ABConstants.SLING_SELECT_POS);
		_animator.Play("flying", 0, 0f);

		IsFlying = true;
				
		// The bird starts with no gravity, so we must set it
		_rigidBody.gravityScale = _launchGravity;

		Vector2 f = new Vector2 (_launchForce.x * -deltaPosFromSlingshot.x, 
								 _launchForce.y * -deltaPosFromSlingshot.y);
		
		_rigidBody.AddForce(f, ForceMode2D.Impulse);

		if(!ABGameWorld.Instance._isSimulation)
			_destroyEffect._shootParticles = true;

		_audioSource.PlayOneShot(_clips[3]);
	}
}
