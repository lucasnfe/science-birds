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
		float y = -(targetPig.y - slingPos.y) - 0.8f;
		
		// first estimate launch angle using the projectile equation (constant velocity)
		float v = birdVel;
		float g = birdGrav;
		float sqrt = (v*v*v*v) - (g*((g*x*x) + (2f*y*v*v)));
		float angleInRadians = Mathf.Atan(((v*v) - Mathf.Sqrt(sqrt))/(g*x));

		return findReleasePoint(slingPos, angleInRadians, y);
	}

	/* find the release point given the sling location and launch angle, using maximum velocity
     *
     * @param   sling - bounding rectangle of the slingshot
     *          theta - launch angle in radians (anticlockwise from positive direction of the x-axis)
     * @return  the release point on screen
     */
	public static Vector2 findReleasePoint(Vector2 slingPos, float theta, float altitude)
	{
		//float mag = slingHeight * -Physics2D.gravity.y;
		Vector2 distance = new Vector2(Mathf.Cos(theta), -Mathf.Sin(theta));
		return slingPos - distance;
	}
}
