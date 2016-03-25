using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/** \class TrajectoryPlanner
 *  \brief  Calculates the launching position and angle to hit pigs
 *
 *  Uses information of slingshot and pig positions, bird velocity, world gravity and launching angle
 *  to try to hit the pig (does not account for obstacles).
 */
public class TrajectoryPlanner {
    /** 
     *  Estimate launch points given a desired target point using maximum velocity
     *  If there are two launch point for the target, they are both returned in
     *  the list {lower point, higher point)
     *  Note - angles greater than 75 are not considered
     *  @param  [in]    slingPos    bounding rectangle (X, Y) of the slingshot
     *  @param  [in]    targetPig   X, Y coordinates of the target to hit
     *  @param  [in]    birdVel     floating number with velocity of the thrown bird
     *  @param  [in]    birdGrav   floating number with the gravity of the thrown bird
     *  @return Vector2 A list containing 2 possible release points
     */
    public static Vector2 estimateLaunchPoint(Vector2 slingPos, Vector2 targetPig, float birdVel, float birdGrav) {

		/**calculate relative x position of the target (normalized)*/
		float x = targetPig.x - slingPos.x;
        /**calculate relative y position of the target (normalized)*/
        float y = -(targetPig.y - slingPos.y) - 0.8f;
		
        /**Bird Velocity, constant*/
		float v = birdVel;
        /**Gravity on bird*/
		float g = birdGrav;
        /**first estimate launch angle using the projectile equation (constant velocity)*/
        float sqrt = (v*v*v*v) - (g*((g*x*x) + (2f*y*v*v)));
		float angleInRadians = Mathf.Atan(((v*v) - Mathf.Sqrt(sqrt))/(g*x));

		return findReleasePoint(slingPos, angleInRadians, y);
	}

	/** 
     *  Find the release point given the sling location and launch angle, using maximum velocity
     *
     *  @param[in]  slingPos    bounding rectangle (X, Y) of the slingshot
     *  @param[in]  theta       floating number with launching angle in radians (anticlockwise from positive direction of the x-axis)
     *  @param[in]  altitude    floating number with altitude (not used!)
     *  @return  Vector2    the release point on screen
     */
	public static Vector2 findReleasePoint(Vector2 slingPos, float theta, float altitude)
	{
		//float mag = slingHeight * -Physics2D.gravity.y;
		Vector2 distance = new Vector2(Mathf.Cos(theta), -Mathf.Sin(theta));
		return slingPos - distance;
	}
}
