using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bird : MonoBehaviour {

    private int _nextParticleTrajectory;
    private Animator _animator;
    private Vector3  _selectPosition;

    public float _dragRadius = 1.0f;
    public float _dragSpeed = 1.0f;
    public float _launchGravity = 1.0f;
    public float _trajectoryParticleFrequency = 0.5f;
    public float _jumpForce;
    public float _timeToDie;
    public float _maxTimeToJump;
    public float _maxTimeToBlink;

    public Vector2 _launchForce;
    public Transform _slingshot;
    public Transform _slingshotBase;
    public AudioClip[] _clips;
    public GameObject[] _trajectoryParticles;

    public bool JumpToSlingshot{ get; set; }
    public bool OutOfSlingShot{ get; set; }

    void Start ()
    {
        _animator = GetComponent<Animator>();

        _slingshotBase.active = false;

        float nextBlinkDelay = Random.Range(0.0f, _maxTimeToBlink);
        Invoke("Blink", nextBlinkDelay + 1.0f);

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
    }

    void Update()
    {
        if(IsFlying() && !OutOfSlingShot)

            DragBird(transform.position);
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void IdleJump()
    {
        if(JumpToSlingshot)
            return;

        if(IsIdle() && rigidbody2D.gravityScale > 0f)
        {
            int rotationDirection = Random.Range(-1, 1);
            rigidbody2D.AddForce(Vector2.up * _jumpForce);
        }

        float nextJumpDelay = Random.Range(0.0f, _maxTimeToJump);
        Invoke("IdleJump", nextJumpDelay + 1.0f);
    }

    void Blink()
    {
        if(IsIdle())
            _animator.Play("blink", 0, 0f);

        float nextBlinkDelay = Random.Range(0.0f, _maxTimeToBlink);
        Invoke("Blink", nextBlinkDelay + 1.0f);
    }

    void PlayAudio(int audioIndex)
    {
        if(_clips.Length > audioIndex)
            audio.PlayOneShot(_clips[audioIndex], 1.0f);
    }

    void DropTrajectoryParticle()
    {
        _nextParticleTrajectory = (_nextParticleTrajectory + 1) % _trajectoryParticles.Length;

        GameObject particle = (GameObject) Instantiate(_trajectoryParticles[_nextParticleTrajectory], transform.position, Quaternion.identity);
        particle.transform.parent = GameObject.Find("Foreground/Effects").transform;
        particle.name = name;
    }

    void RemoveLastTrajectoryParticle(string birdName)
    {
        int lastBirdIndex = int.Parse(name.Substring(name.Length - 1)) - 1;

        if(lastBirdIndex > 0)
        {
            string lastBirdName = birdName.Remove(birdName.Length - 1, 1);
            lastBirdName = lastBirdName + lastBirdIndex;

            GameObject effects = GameObject.Find("Foreground/Effects");

            foreach (Transform child in effects.transform)
            {
                if(child.gameObject.name == lastBirdName)

                    Destroy(child.gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(IsFlying())
        {
            CancelInvoke("DropTrajectoryParticle");
            RemoveLastTrajectoryParticle(name);

            Invoke("Die", _timeToDie);
        }
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot" && JumpToSlingshot)
        {
            _slingshotBase.active = false;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot" && IsFlying())
        {
            _slingshotBase.transform.position = _selectPosition;
            _slingshotBase.transform.rotation = Quaternion.Euler(_slingshotBase.transform.rotation.x,
                                                                 _slingshotBase.transform.rotation.y, 0f);

            OutOfSlingShot = true;
        }
    }

    public bool IsIdle()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName("idle");
    }

    public bool IsFlying()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName("flying");
    }

    public bool IsSelected()
    {
        return _selectPosition != Vector3.zero;
    }

    public void SelectBird()
    {
        _slingshotBase.active = true;
        _selectPosition = transform.position;
        _animator.Play("selected", 0, 0f);
    }

    public void SetBirdOnSlingshot(Vector3 endPosition)
    {
        transform.position = Vector3.Lerp(transform.position, endPosition, _dragSpeed * Time.deltaTime);
    }

	public void DragBird(Vector3 dragPosition)
	{
        float deltaPosFromSlingshot = Vector3.Distance(dragPosition, _selectPosition);

        // Lock bird movement inside a circle
        if(deltaPosFromSlingshot > _dragRadius)
            dragPosition = (dragPosition - _selectPosition).normalized * _dragRadius + _selectPosition;

        // Slingshot base look to slingshot
        transform.position = Vector3.Lerp (transform.position, dragPosition, Time.deltaTime * _dragSpeed);

        Vector3 dist = _slingshotBase.transform.position - _selectPosition;
        float angle = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
        _slingshotBase.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Slingshot base rotate around the selected point
        CircleCollider2D col = _slingshot.GetComponent<CircleCollider2D>();
        _slingshotBase.transform.position = transform.position + (transform.position - _selectPosition).normalized * (col.radius+0.05f) * 0.5f;
	}

	public void LaunchBird()
	{
        Vector2 deltaPosFromSlingshot = transform.position - _selectPosition;
		_animator.Play("flying", 0, 0f);

		// The bird starts with no gravity, so we must set it
		rigidbody2D.gravityScale = _launchGravity;

        Vector2 force = new Vector2(_launchForce.x * -deltaPosFromSlingshot.normalized.x,
                                    _launchForce.y * -deltaPosFromSlingshot.normalized.y);

		rigidbody2D.AddForce(force);

        InvokeRepeating("DropTrajectoryParticle", 0.1f, _trajectoryParticleFrequency);
	}
}
