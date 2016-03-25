using UnityEngine;
using System.Collections;
/** \class Pig
 *  \brief  Management of Pig Characters of game
 *
 *  Contains pig's life and dust effect game object, contains methods to kill the pig,
 *  and action to take on collision
 */
public class Pig : Character {
    /**Pig's life*/
	public float _life;
    /**GameObject containing the dustEffect for particle trajectory*/
	public GameObject dustEffect;
    /**
     *  Kills the pig. If not on simulation, play audio clip for the pig death,
     *  Spawn the points granted on pig's death, and adds the trajectory particle with the dustEffect GameObject.
     *  Calls GameWorld method KillPig() to kill this pig on the level, and them calls the Die() method from Character
     *  Class.
     */
	public override void Die()
	{
		if(!GameWorld.Instance._isSimulation) {

			ABAudioController.Instance.PlayIndependentSFX(_clips[0]);
			GameWorld.Instance.SpawnPoint(50, transform.position);
			GameWorld.Instance.AddTrajectoryParticle(dustEffect, transform.position, name);
		}

		GameWorld.Instance.KillPig(this);

		base.Die();
	}
    /**
     *  When something collides with the Pig, check if the magnitude of the relative velocity of colliding object
     *  greater than pig's life, if it is, invokes Die() method after _timeToDie seconds and plays the hurt animation.
     */
	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.relativeVelocity.magnitude > _life)
		{
			Invoke("Die", _timeToDie);
			_animator.Play("hurt", 0, 0f);
		}
	}
}
