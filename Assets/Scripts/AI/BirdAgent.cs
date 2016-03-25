using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/**
 *  \struct Shot
 *  \brief  Struct to hold the data of a shot and its constructor
 *
 *  Struct containing the data of a shot and a constructor to set all the data
 */
public struct Shot {
    /**X and Y coordinates of sling*/
	public float x,y;
    /**X and Y coordinates of where to release the sling to shoot the bird*/
	public float dx,dy;
    /**Time of drag*/
    public float dragTime;
    /**
     *  Constructor with all the parameters
     */
	public Shot(float x, float y, float dx, float dy, float dragTime) 
	{
		this.x = x;
		this.y = y;
		this.dx = dx;
		this.dy = dy;
		this.dragTime = dragTime;
	}
}
/** \class BirdAgent
 *  \brief  Agent that makes the actions to shoot the bird.
 *
 *  Calculates the shot data using TrajectoryPlanner, adds noise if last shot missed, 
 *  and make all the actions necessary to shoot the bird to a pig
 */
public class BirdAgent : MonoBehaviour {
    /**Timer to wait for next shot*/
	private float _throwTimer;
    /**Pig to be targeted*/
	private Pig _lastTargetPig;
    /**Current Bird to be shot*/
	private Bird _currentBird;
    /**Shot struct with information about the next shot to be taken*/
	private Shot _nextShot;
    /**Boolean to check if already throwing a bird*/
	public bool IsThrowingBird{ get; set; }

    /**
     *  At every Update, if going to throw a bird, drags it to drag position.
     *  Next step is to add elapsed time since last update to _throwTimer and if greater than
     *  drag time for next shot, sets IsThrowingBir to false and launches the _currentBird.
     */
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
    /**
     *  If the target is the same as the last one, highlight it white.
     *  Sets IsThrowingBird to true, sets the _throwTimer to 0 and updates the _currentBird.
     *  If on simulation, Highlights the target to red.
     *  Gets the next shot struct using Solve() and throws the currentBird
     *  @param[in]  currentBird current Bird object to be shot
     *  @param[in]  targetPig   current Pig targeted by shot
     *  @param[in]  slingPos    Vector2 (X, Y) position of slingshot
     */
	public void ThrowBird(Bird currentBird, Pig targetPig, Vector2 slingPos)
	{
		if(_lastTargetPig)
			_lastTargetPig.GetComponent<SpriteRenderer>().material.color = Color.white;

		IsThrowingBird = true;
		
		_throwTimer = 0f;
		_currentBird = currentBird;

		// Highlight the target
		if(!GameWorld.Instance._isSimulation)
			targetPig.GetComponent<SpriteRenderer>().material.color = Color.red;

		_nextShot = Solve(currentBird, targetPig, slingPos);
		currentBird.SelectBird();
	}
    /**
     *  Picks a pig, if is the same as before adds noise to shot, estimates the trajectory,
     *  and returns new Shot struct with shot data
     *  @param[in]  currentBird current bird to be shot
     *  @param[in]  targetPig   current pig to be shot at
     *  @param[in]  slingPos    Vector2 (X,Y) containing position of slingshot
     *  @return Shot    Shot struct containing data about the shot
     */
	private Shot Solve(Bird currentBird, Pig targetPig, Vector2 slingPos) 
	{
		// 1. Random pick up a pig
		Vector2 shotPos = targetPig.transform.position;
		
		// 2. If the target is very close to before, randomly choose a point near it
		if(_lastTargetPig != null && _lastTargetPig == targetPig)
		{
			float angle = Random.value * Mathf.PI * 2f;
			if(Random.value < 0.5)
				angle *= -1f;

			shotPos.x = shotPos.x + Mathf.Cos(angle) * 0.5f;
			shotPos.y = shotPos.y + Mathf.Cos(angle) * 0.5f;
		}
		
		_lastTargetPig = targetPig;
		
		// 3. Estimate the trajectory
		float birdVel = currentBird._launchForce.x * -2f;
		float birdGrav =  currentBird._launchGravity * Physics2D.gravity.y;

		Vector2 releasePoint = TrajectoryPlanner.estimateLaunchPoint(slingPos, shotPos, birdVel, birdGrav);

		// 4. Calculate the tapping time according the bird type 	
		return new Shot(slingPos.x, slingPos.y, releasePoint.x, releasePoint.y, 0.5f);
	}
}
