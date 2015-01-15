using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : MonoBehaviour {

	private Bird _lastThrownBird;

	private List<Pig>  _pigs;
	private List<Bird> _birds;

	// Main game components
	public Transform _slingshot;
	public Transform _slingshotBase;

	public GameObject _pig;
	public GameObject _bird;
	public GameObject []Templates;
	
	public BirdAgent _birdAgent;
	public GameplayCamera _camera;

	// Game world properties
	public float _timeToResetLevel = 1f;
	public Vector3 _slingSelectPos;

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
	void Awake () 
	{	
		_pigs = new List<Pig>();
		_birds = new List<Bird>();

		// Calculating slingshot select position
		Vector3 selectPos = _slingshot.transform.position;
		_slingSelectPos.x += selectPos.x;
		_slingSelectPos.y += selectPos.y;
		_slingSelectPos.z += _slingshot.FindChild("slingshot_front").transform.position.z;
	}

	// Update is called once per frame
	void Update ()
	{
		// Activate game AI if it is set
		if(_birdAgent != null && !_birdAgent.IsThrowingBird)
		{
			if(_birds.Count == 0)
				return;

			// Wait the level stay stable before tthrowing next bird
			if(CalcLevelStability() > 0.1f)
				return;

			if(_pigs.Count > 0 && _birds[0] && !_birds[0].JumpToSlingshot && _lastThrownBird != _birds[0]) {

				Pig randomPig = _pigs[Random.Range(0, _pigs.Count - 1)];
				_birdAgent.ThrowBird(_birds[0], randomPig, _slingSelectPos);
				_lastThrownBird = _birds[0];
			}
		}

		// Check if birds was trown, if it died and swap them when needed
		ManageBirds();
	}

	void ManageBirds()
	{
		if(_birds.Count == 0)
			return;
		
		// Move next bird to the slingshot
		if(_birds[0].JumpToSlingshot)
			_birds[0].SetBirdOnSlingshot();
		
		// Kill current bird if it flies to outside the level
		if(_birds[0].OutOfSlingShot)
			
			if(_birds[0].transform.position.x > _camera.RightBound() + _camera.CalculateCameraRect().width/2f ||
			   _birds[0].transform.position.x < _camera.LeftBound()  - _camera.CalculateCameraRect().width/2f)
		{
			RemoveLastTrajectoryParticle(_birds[0].name);
			_birds[0].Die();
		}
	}

	public Bird GetCurrentBird()
	{
		if(_birds.Count > 0)
			return _birds[0];
		
		return null;
	}

	void ResetLevel()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

	public void AddTrajectoryParticle(GameObject particleTemplate, Vector3 position, string parentName)
	{
		GameObject particle = (GameObject) Instantiate(particleTemplate, position, Quaternion.identity);
		particle.transform.parent = transform.FindChild("Effects").transform;
		particle.name = parentName;

		// If it is an animated particle, destroy it after animation
		Animator anim = particle.GetComponent<Animator>();

		if(anim != null)
			Destroy(particle, 0.5f);
	}
	
	public void AddBird(Object original, Vector3 position, Quaternion rotation, string birdname, bool isFirst = false)
	{
		GameObject newGameObject = (GameObject)Instantiate(original, position, rotation);
		newGameObject.transform.parent = GameWorld.Instance.transform.Find("Birds");
		newGameObject.name = birdname;

		Bird bird = newGameObject.GetComponent<Bird>();

		if(isFirst)
			bird.rigidbody2D.gravityScale = 0f;

		if(bird != null)
			_birds.Add(bird);
	}

	public void AddPig(Object original, Vector3 position, Quaternion rotation)
	{
		GameObject newGameObject = AddBlock(original, position, rotation);

		Pig pig = newGameObject.GetComponent<Pig>();
		if(pig != null)
			_pigs.Add(pig);
	}

	public GameObject AddBlock(Object original, Vector3 position, Quaternion rotation)
	{
		GameObject newGameObject = (GameObject)Instantiate(original, position, rotation);
		newGameObject.transform.parent = GameWorld.Instance.transform.Find("Blocks");

		return newGameObject;
	}

	public void KillPig(Pig pig)
	{
		_pigs.Remove(pig);
		
		if(_pigs.Count == 0)
		{
			//			Invoke("ResetLevel", _timeToResetLevel);
			return;
		}
	}
	
	public void KillBird(Bird bird)
	{
		_birds.Remove(bird);
		
		if(_birds.Count == 0)
		{
			//			Invoke("ResetLevel", _timeToResetLevel);
			return;
		}
		
		_birds[0].rigidbody2D.gravityScale = 0f;
		_birds[0].JumpToSlingshot = true;
	}

	public void RemoveLastTrajectoryParticle(string currentBirdName)
	{
		int lastBirdIndex = int.Parse(currentBirdName.Substring(currentBirdName.Length - 1)) - 1;
		
		if(lastBirdIndex >= 0)
		{
			string lastBirdName = currentBirdName.Remove(currentBirdName.Length - 1, 1);
			lastBirdName = lastBirdName + lastBirdIndex;
			
			Transform effects = transform.FindChild("Effects");
			
			foreach (Transform child in effects)
			{
				if(child.gameObject.name == lastBirdName)
					Destroy(child.gameObject);
			}
		}
	}

	public int GetPigsAvailableAmount()
	{
		return _pigs.Count;
	}
	
	public int GetBirdsAvailableAmount()
	{
		return _birds.Count;
	}
	

	public void ClearWorld()
	{
		Transform blocks = transform.FindChild("Blocks");
		foreach(Transform b in blocks)
		{
			Destroy(b.gameObject);
		}

		_pigs.Clear();

		Transform birds = transform.FindChild("Birds");
		foreach(Transform b in birds)
		{
			Destroy(b.gameObject);
		}

		_birds.Clear();

		Transform effects = transform.FindChild("Effects");
		foreach(Transform b in effects)
		{
			Destroy(b.gameObject);
		}
	}

	private float CalcLevelStability()
	{
		float totalVelocity = 0f;

		Transform blocks = transform.FindChild("Blocks");
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
