using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Shot {

	public float x,y;
	public float dx,dy;
	public float dragTime;

	public Shot(float x, float y, float dx, float dy, float dragTime) 
	{
		this.x = x;
		this.y = y;
		this.dx = dx;
		this.dy = dy;
		this.dragTime = dragTime;
	}
}

public class BirdAgent : MonoBehaviour {

	private float _throwTimer;

	private Pig _lastTargetPig;
	private Bird _currentBird;
	private Shot _nextShot;

	public bool IsThrowingBird{ get; set; }

	void Update()
	{
		if(IsThrowingBird)
		{
			Vector2 dragPos = new Vector2(_nextShot.dx, _nextShot.dy);

			_currentBird.DragBird(dragPos);

			_throwTimer += Time.deltaTime;
			if(_throwTimer >= _nextShot.dragTime)
			{
				IsThrowingBird = false;
				_currentBird.LaunchBird();
			}
		}
	}

	public void ThrowBird(Bird currentBird, Pig targetPig, Vector2 slingPos)
	{
		if(_lastTargetPig)
			_lastTargetPig.GetComponent<SpriteRenderer>().material.color = Color.white;

		IsThrowingBird = true;
		_throwTimer = 0f;
		_currentBird = currentBird;

		// Highlight the target
		targetPig.GetComponent<SpriteRenderer>().material.color = Color.red;

		_nextShot = Solve(currentBird, targetPig, slingPos);
		currentBird.SelectBird();
	}

	private Shot Solve(Bird currentBird, Pig targetPig, Vector2 slingPos) 
	{
		// 1. Random pick up a pig
		Vector2 shotPos = targetPig.transform.position;
		
		// 2. If the target is very close to before, randomly choose a point near it
		if(_lastTargetPig != null)
		{
			float distToLastTarget = Vector2.Distance(_lastTargetPig.transform.position, shotPos);
			
			if(distToLastTarget < 0.1f)
			{
				float angle = Random.value * Mathf.PI * 2f;
				shotPos.x = shotPos.x + Mathf.Cos(angle) * 0.5f;
				shotPos.y = shotPos.y + Mathf.Cos(angle) * 0.5f;
			}
		}
		
		_lastTargetPig = targetPig;
		
		// 3. Estimate the trajectory
		float birdVel = currentBird._launchForce.x * Time.fixedDeltaTime;
		float birdGrav =  currentBird._launchGravity * Physics2D.gravity.y;

		Vector2 releasePoint = TrajectoryPlanner.estimateLaunchPoint(slingPos, shotPos, birdVel, birdGrav);

		// 5. Calculate the tapping time according the bird type 	
		return new Shot(slingPos.x, slingPos.y, releasePoint.x, releasePoint.y, 1f);
	}
}
