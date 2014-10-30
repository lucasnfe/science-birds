using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrajectoryPlanner {
	

	/* Estimate launch points given a desired target point using maximum velocity
     * If there are two launch point for the target, they are both returned in
     * the list {lower point, higher point)
     * Note - angles greater than 75 are not considered
     *
     * @param   slingshot - bounding rectangle of the slingshot
     *          targetPoint - coordinates of the target to hit
     * @return  A list containing 2 possible release points
     */
	public static Vector2 estimateLaunchPoint(Vector2 slingPos, Vector2 targetPig, float birdVel, float birdGrav) {

		// calculate relative position of the target (normalised)
		float x = targetPig.x - slingPos.x;
		float y = targetPig.y - slingPos.y;
		
		// first estimate launch angle using the projectile equation (constant velocity)
		float v = birdVel;
		float v2 = v * v;
		float v4 = v2 * v2;
		float g = Physics2D.gravity.y;
		float t1 = Mathf.Atan((v2 - Mathf.Sqrt(v4 - g*(x * x + 2f * y * v2))) / (g*x));
		float t2 = Mathf.Atan((v2 + Mathf.Sqrt(v4 - g*(x * x + 2f * y * v2))) / (g*x));

		Debug.Log(findReleasePoint(slingPos, t1) + " " + findReleasePoint(slingPos, t2));

		return findReleasePoint(slingPos, t1);
	}

	/* find the release point given the sling location and launch angle, using maximum velocity
     *
     * @param   sling - bounding rectangle of the slingshot
     *          theta - launch angle in radians (anticlockwise from positive direction of the x-axis)
     * @return  the release point on screen
     */
	public static Vector2 findReleasePoint(Vector2 slingPos, float theta)
	{
		//float mag = slingHeight * -Physics2D.gravity.y;
		Vector2 release =  new Vector2(slingPos.x - Mathf.Cos(theta), 
		                               slingPos.y - 0.25f - Mathf.Sin(theta));
		
		return release;
	}
}
