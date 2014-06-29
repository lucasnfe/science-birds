using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bird : MonoBehaviour {

    public float _dragRadius = 1.0f;
    public float _dragSpeed = 1.0f;
    public float _launchGravity = 1.0f;
    public float _trajectoryParticleFrequency = 0.5f;
    public Vector2 _launchForce;

    public Transform _slingshot;
    public Transform _slingshotBase;
    public GameObject[] _trajectoryParticles;

    public bool OutOfSlingShot{ get; set; }

    private int _nextParticleTrajectory;

    private Animator _animator;
    private Vector3  _selectPosition;

    void Start ()
    {
        _animator = GetComponent<Animator>();
    }

    void DropTrajectoryParticle()
    {
        _nextParticleTrajectory = (_nextParticleTrajectory + 1) % _trajectoryParticles.Length;
        Instantiate(_trajectoryParticles[_nextParticleTrajectory], transform.position, Quaternion.identity);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CancelInvoke("DropTrajectoryParticle");
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if(collider.tag == "Slingshot" && IsFlying())
        {
            _slingshotBase.transform.position = new Vector3(_selectPosition.x, _selectPosition.y, 1);
            _slingshotBase.transform.rotation = Quaternion.Euler(_slingshotBase.transform.rotation.x,
                                                                 _slingshotBase.transform.rotation.y, 0f);

            transform.DetachChildren();
            OutOfSlingShot = true;
        }
    }

    public bool IsFlying()
    {
        return _animator.GetBool("flying");
    }

    public bool IsSelected()
    {
        return _animator.GetBool("pressed");
    }

    public void SelectBird()
    {
        _selectPosition = transform.position;
        _animator.SetBool("pressed", true);
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
		_animator.SetBool("flying", true);
        _animator.SetBool("pressed", false);

		// The bird starts with no gravity, so we must set it
		rigidbody2D.gravityScale = _launchGravity;

        Vector2 force = new Vector2(_launchForce.x * -deltaPosFromSlingshot.normalized.x,
                                    _launchForce.y * -deltaPosFromSlingshot.normalized.y);

		rigidbody2D.AddForce(force);

        InvokeRepeating("DropTrajectoryParticle", 0.1f, _trajectoryParticleFrequency);
	}
}
