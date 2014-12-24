using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : MonoBehaviour {

	private int _currentBirdIndex;
	private Bird _lastThrownBird;

	// Main game components
	public Transform  _slingshot;
	public List<Pig>  _pigs;
	public List<Bird> _birds;
	
	public GameplayCamera _camera;
	public BirdAgent _birdAgent;

	public float _timeToResetLevel = 1f;
	public Vector3 SlingSelectPos{ get; set; }

	//Here is a private reference only this class can access
	private static GameWorld _instance;
	
	//This is the public reference that other classes will use
	public static GameWorld Instance
	{
		get
		{
			//If _instance hasn't been set yet, we grab it from the scene!
			//This will only happen the first time this reference is used.
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<GameWorld>();

			return _instance;
		}
	}
	
	// Use this for initialization
	void Start () 
	{	
		Transform birds = transform.Find("Birds");
		foreach(Transform b in birds)
		{
			Bird bird = b.GetComponent<Bird>();
			if(bird != null)
				_birds.Add(bird);
		}	

		_birds[_currentBirdIndex].rigidbody2D.gravityScale = 0f;

		_pigs  = new List<Pig>();

		Transform pigs = transform.Find("Blocks");
		foreach(Transform p in pigs)
		{
			Pig pig = p.GetComponent<Pig>();
			if(pig != null)
				_pigs.Add(pig);
		}

		// Calculating slingshot select position
		Vector3 selectPos = _slingshot.transform.position;
		selectPos.x -= _birds[0].collider2D.bounds.size.x/4f;
		selectPos.y += _slingshot.collider2D.bounds.size.y * 2f;
		selectPos.z = _slingshot.FindChild("slingshot_front").transform.position.z + 1;

		SlingSelectPos = selectPos;
	}

	// Update is called once per frame
	void Update ()
	{
		// Activate game AI if it is set
		if(_birdAgent != null && !_birdAgent.IsThrowingBird)
		{
			if(_currentBirdIndex >= _birds.Count)
				return;

			if(CalcLevelStability() > 0.1f)
				return;

			Bird currentBird = _birds[_currentBirdIndex];

			if(currentBird && !currentBird.JumpToSlingshot && _lastThrownBird != currentBird)
			{
				int randomIndex = Random.Range(0, _pigs.Count - 1);
				if(randomIndex >= 0 && randomIndex <= _pigs.Count - 1)
				{
					Pig randomPig = _pigs[randomIndex];
					_birdAgent.ThrowBird(currentBird, randomPig, SlingSelectPos);
					_lastThrownBird = currentBird;
				}
			}
		}

		// Check if birds was trown, if it died and swap them when needed
		ManageBirds();
	}

	void ManageBirds()
	{
		if(_currentBirdIndex >= _birds.Count)
			return;

		if(!_birds[_currentBirdIndex] && !_birds[_currentBirdIndex].JumpToSlingshot)
		{
			_currentBirdIndex++;
			
			// If there are no more birds, reload the game
			if(_currentBirdIndex == _birds.Count)
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
		if(_currentBirdIndex < _birds.Count)
			return _birds[_currentBirdIndex];
		
		return null;
	}

	public void KillPig(Pig pig)
	{
		_pigs.Remove(pig);

		if(_pigs.Count == 0)
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

	private float CalcLevelStability()
	{
		float totalVelocity = 0f;

		Transform blocks = transform.Find("Blocks");
		foreach(Transform b in blocks)
		{
			Rigidbody2D []bodies = b.GetComponentsInChildren<Rigidbody2D>();

			foreach(Rigidbody2D body in bodies)
			{
				totalVelocity += body.velocity.magnitude;
			}
		}

		return totalVelocity;
	}
}
