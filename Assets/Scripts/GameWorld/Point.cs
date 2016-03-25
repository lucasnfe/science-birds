using UnityEngine;
using System.Collections;
/** \class Point
 *  \brief  Management of Game points (Score)
 *
 *  Contains time to wait untill killing the score, start method and die method.
 */
public class Point : MonoBehaviour {

    /**Time to wait before invoking die method*/
	public float _timeToDie = 0.5f;

	/**
     *  On initialization, invokes the Die() methos after _timeToDie seconds
     */
	void Start () {

		Invoke("Die", _timeToDie);
	}
	/**
     *  Destroy the Point game object
     */
	void Die() {

		Destroy(this.gameObject);
	}
}
