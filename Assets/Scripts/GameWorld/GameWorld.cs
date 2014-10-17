using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : MonoBehaviour {

	public float _timeToResetLevel = 1f;
	public Bird []_birds;
	public GameplayCamera _camera;

	private int _currentBirdIndex;

	// Use this for initialization
	void Start () 
	{	
		_birds[_currentBirdIndex].rigidbody2D.gravityScale = 0f;
		
		for(int i = 1; i < _birds.Length; i++)
			_birds[i].rigidbody2D.gravityScale = 1f;
	}

	// Update is called once per frame
	void Update () 
	{
		// Check if birds was trown, if it died and swap them when needed
		ManageBirds();

		// Check if all the pigs had died
		ManagePigs();
	}

	void ManageBirds()
	{
		if(_currentBirdIndex >= _birds.Length)
			return;

		if(!_birds[_currentBirdIndex] && !_birds[_currentBirdIndex].JumpToSlingshot)
		{
			_currentBirdIndex++;
			
			// If there are no more birds, reload the game
			if(_currentBirdIndex == _birds.Length)
			{
				Invoke("ResetLevel", _timeToResetLevel);
				return;
			}
			
			_birds[_currentBirdIndex].rigidbody2D.gravityScale = 0f;
			_birds[_currentBirdIndex].JumpToSlingshot = true;
		}
		
		// Move next bird to the slingshot
		if(_birds[_currentBirdIndex].JumpToSlingshot)
		{
			_birds[_currentBirdIndex].SetBirdOnSlingshot();
		}
		
		// Kill current bird if it flies to outside the level
		if(_birds[_currentBirdIndex].OutOfSlingShot)
			
			if(_birds[_currentBirdIndex].transform.position.x > _camera.RightBound() + _camera.CalculateCameraRect().width/2f ||
			   _birds[_currentBirdIndex].transform.position.x < _camera.LeftBound()  - _camera.CalculateCameraRect().width/2f)
		{
			RemoveLastTrajectoryParticle(_birds[_currentBirdIndex].name);
			_birds[_currentBirdIndex].Die();
		}
	}

	public Bird GetCurrentBird()
	{
		if(_currentBirdIndex < _birds.Length)
			return _birds[_currentBirdIndex];
		
		return null;
	}

	void ManagePigs()
	{
		int pigsAmount = 0;
		Transform blocks = transform.Find("Blocks");
		
		foreach(Transform block in blocks)
		{
			if(block.GetComponent<Pig>() != null)
				pigsAmount++;
		}
		
		if(pigsAmount == 0)
		{
			Invoke("ResetLevel", _timeToResetLevel);
			return;
		}
	}

	void ResetLevel()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

	public void AddTrajectoryParticle(GameObject particleTemplate, Vector3 position, string parentName)
	{
		GameObject particle = (GameObject) Instantiate(particleTemplate, position, Quaternion.identity);
		particle.transform.parent = GameObject.Find("Level/Effects").transform;
		particle.name = parentName;

		// If it is an animated particle, destroy it after animation
		Animator anim = particle.GetComponent<Animator>();

		if(anim != null)
			Destroy(particle, 0.5f);
	}

	public void RemoveLastTrajectoryParticle(string currentBirdName)
	{
		int lastBirdIndex = int.Parse(currentBirdName.Substring(currentBirdName.Length - 1)) - 1;
		
		if(lastBirdIndex > 0)
		{
			string lastBirdName = currentBirdName.Remove(currentBirdName.Length - 1, 1);
			lastBirdName = lastBirdName + lastBirdIndex;

			GameObject effects = GameObject.Find("Level/Effects");
			
			foreach (Transform child in effects.transform)
			{
				if(child.gameObject.name == lastBirdName)
					Destroy(child.gameObject);
			}
		}
	}
}
