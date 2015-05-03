using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : ABSingleton<GameWorld> {

	private int _birdsThrown;
	
	private List<Pig>  _pigs;
	private List<Bird> _birds;
	private Bird _lastThrownBird;

	private static ABLevel _currentLevel;

	private int _pigsAtStart;
	public int PigsAtStart { 
		get { return _pigsAtStart; }
	}
	
	private int _birdsAtStart;
	public int BirdsAtStart { 
		get { return _birdsAtStart; }
	}

	private int _blocksAtStart;
	public int BlocksAtStart { 
		get { return _blocksAtStart; }
	}
	
	private int _stabilityUntilFirstBird;
	public int StabilityUntilFirstBird { 
		get { return _stabilityUntilFirstBird; }
	}

	// Main game components
	public Transform _slingshot;
	public Transform _slingshotBase;
	public Transform _ground;

	public GameObject _pig;
	public GameObject _bird;
	public GameObject _point;
	public GameObject []Templates;
	
	public RectTransform  _pointHUD;

	public HUD 			  _hud;
	public LevelSource    _levelSource;
	public BirdAgent      _birdAgent;
	public GameplayCamera _camera;
	
	public AudioClip[] _clips;

	// Game world properties
	public bool _isSimulation;
	public float _timeToResetLevel = 1f;
	public Vector3 _slingSelectPos;
	
	// Use this for initialization
	void Start () 
	{	
		_pigs = new List<Pig>();
		_birds = new List<Bird>();

		if(!_isSimulation) {

			audio.PlayOneShot(_clips[0]);
			audio.PlayOneShot(_clips[1]);
		}

		// Calculating slingshot select position
		Vector3 selectPos = _slingshot.transform.position;
		_slingSelectPos.x += selectPos.x;
		_slingSelectPos.y += selectPos.y;
		_slingSelectPos.z += _slingshot.FindChild("slingshot_front").transform.position.z;

		if(_currentLevel == null && _levelSource != null)
			_currentLevel = _levelSource.NextLevel();

		if(_currentLevel != null) 
		{
			DecodeLevel(_currentLevel.gameObjects, _currentLevel.birdsAmount);
			AdaptCameraWidthToLevel();
		}
	}

	public void DecodeLevel(List<ABGameObject> gameObjects, int birdsAmount) 
	{
		ClearWorld();

		foreach(ABGameObject gameObj in gameObjects)
		{
			if(gameObj.Label < GameWorld.Instance.Templates.Length)
				
				AddBlock(GameWorld.Instance.Templates[gameObj.Label], gameObj.Position, 
				                            GameWorld.Instance.Templates[gameObj.Label].transform.rotation);
			else
				AddPig(GameWorld.Instance._pig, gameObj.Position, 
				                          GameWorld.Instance._pig.transform.rotation);
		}
		
		//First bird must be in the slingshot
		AddBird(_bird, _slingSelectPos, _bird.transform.rotation, "bird0", true);
		
		if(birdsAmount > 0)
		{
			Vector3 birdsPos = _slingshot.transform.position;
			birdsPos.y = _ground.collider2D.bounds.center.y + _ground.collider2D.bounds.size.y/2f;
			
			for(int i = 0; i < birdsAmount; i++)
			{
				birdsPos.x -= _bird.GetComponent<SpriteRenderer>().bounds.size.x * 2f;
				GameWorld.Instance.AddBird(_bird, birdsPos, _bird.transform.rotation, "bird" + (i+1));
			}
		}
		
		StartWorld();
	}

	// Update is called once per frame
	void Update ()
	{
		// Check if birds was trown, if it died and swap them when needed
		ManageBirds();
		
		// If an object goes out of screen, destroy it
		DestroyIfOutScreen();

		// Activate game AI if it is set
		if(_birdAgent != null && !_birdAgent.IsThrowingBird)
		{				
			if(_birds.Count == 0)
				return;
			
			// Calculate stability until first birds is trown
			if(_birdsThrown == 0)
				_stabilityUntilFirstBird += (BlocksAtStart - GetBlocksAvailableAmount());

			// Wait the level stay stable before tthrowing next bird
			if(!IsLevelStable())
				return;

			if(_pigs.Count > 0 && _birds[0] && !_birds[0].JumpToSlingshot && _lastThrownBird != _birds[0]) {

				Pig randomPig = _pigs[Random.Range(0, _pigs.Count)];
				_birdAgent.ThrowBird(_birds[0], randomPig, _slingSelectPos);
				_lastThrownBird = _birds[0];
				_birdsThrown++;
			}
		}
	}

	void DestroyIfOutScreen()
	{
		Transform blocks = transform.FindChild("Blocks");

		Rigidbody2D []bodies = blocks.GetComponentsInChildren<Rigidbody2D>();

		foreach(Rigidbody2D body in bodies)
		{
			Transform b = body.transform;

			if(IsObjectOutOfWorld(b))
			{
				if(b.GetComponent<Pig>() != null)

					b.GetComponent<Pig>().Die();
				else
					Destroy(b.gameObject);
			}
		}
	
		Transform birds = transform.FindChild("Birds");
		foreach(Transform b in birds)
		{			
			if(IsObjectOutOfWorld(b))
			{
				if(b.GetComponent<Bird>() != null)
				{
					RemoveLastTrajectoryParticle(b.GetComponent<Bird>().name);
					b.GetComponent<Bird>().Die();
				}
			}
		}
	}
	
	public bool IsObjectOutOfWorld(Transform abGameObject)
	{
		Vector2 halfSize = abGameObject.GetComponent<Collider2D>().bounds.size/2f;
		
		if(abGameObject.position.x - halfSize.x > _ground.collider2D.bounds.center.x + _ground.collider2D.bounds.size.x/2f ||
		   abGameObject.position.x + halfSize.x < _ground.collider2D.bounds.center.x - _ground.collider2D.bounds.size.x/2f || 
		   abGameObject.position.y + halfSize.y < _ground.collider2D.bounds.center.y - _ground.collider2D.bounds.size.y/2f)

			   return true;
		
		return false;
	}

	void ManageBirds()
	{
		if(_birds.Count == 0)
			return;
		
		// Move next bird to the slingshot
		if(_birds[0].JumpToSlingshot)
			_birds[0].SetBirdOnSlingshot();
	}

	public Bird GetCurrentBird()
	{
		if(_birds.Count > 0)
			return _birds[0];
		
		return null;
	}

	public void NextLevel()
	{
		_currentLevel = null;

		if(LevelSource.CurrentLevel % _levelSource.LevelLimit() == 0)

			ABSceneManager.Instance.LoadScene(GameData.Instance.CurrentQuestionary);
		else
			ABSceneManager.Instance.LoadScene(Application.loadedLevelName);
	}

	public void ResetLevel()
	{
		ABSceneManager.Instance.LoadScene(Application.loadedLevelName);
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

	public void SpawnPoint(uint point, Vector3 position)
	{
		GameObject pointObj = Instantiate(_point, new Vector3(position.x, position.y, _point.transform.position.z), Quaternion.identity) as GameObject; 
		pointObj.transform.SetParent(_pointHUD.transform);

		Text pointText = pointObj.GetComponent<Text>();
		pointText.text = point.ToString();

		_hud.AddScore(point);
	}

	public void KillPig(Pig pig)
	{
		_pigs.Remove(pig);
		
		if(_pigs.Count == 0)
		{
			// Check if player won the game
			if(!_isSimulation) 
				Invoke("NextLevel", _timeToResetLevel);
			
			return;
		}
	}
	
	public void KillBird(Bird bird)
	{
		_birds.Remove(bird);
		
		if(_birds.Count == 0)
		{
			// Check if player lost the game
			if(!_isSimulation)
				Invoke("ResetLevel", _timeToResetLevel);

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

	public int GetBlocksAvailableAmount()
	{
		int blocksAmount = 0;

		Transform blocks = transform.FindChild("Blocks");
		foreach(Transform b in blocks)
		{
			if(b.GetComponent<Pig>() == null)
			
				for(int i = 0; i < b.GetComponentsInChildren<Rigidbody2D>().Length; i++)
					blocksAmount++;
		}

		return blocksAmount;
	}
	
	public float GetLevelStability()
	{
		float totalVelocity = 0f;

		Transform blocks = transform.FindChild("Blocks");
		foreach(Transform b in blocks)
		{
			Rigidbody2D []bodies = b.GetComponentsInChildren<Rigidbody2D>();

			foreach(Rigidbody2D body in bodies)
			{
				if(!IsObjectOutOfWorld(body.transform))
					totalVelocity += body.velocity.magnitude;
			}
		}

		return totalVelocity;
	}
	
	public bool IsLevelStable()
	{
		return GetLevelStability() == 0f;
	}
	
	public void StartWorld()
	{
		_pigsAtStart = GetPigsAvailableAmount();
		_birdsAtStart = GetBirdsAvailableAmount();
		_blocksAtStart = GetBlocksAvailableAmount();
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
		
		_birdsThrown = 0;
		_stabilityUntilFirstBird = 0;
	}

	private void AdaptCameraWidthToLevel() {
		
		// Adapt the camera to show all the blocks		
		float levelLeftBound = _ground.transform.position.x - _ground.collider2D.bounds.size.x/2f;
		float groundSurfacePos = _ground.transform.position.x + _ground.collider2D.bounds.size.y/2f;
				
		float minPosX = _currentLevel.gameObjects[0].Position.x - _currentLevel.gameObjects[0].GetBounds().size.x/2f;
		float maxPosX = _currentLevel.gameObjects[0].Position.x + _currentLevel.gameObjects[0].GetBounds().size.x/2f; 
		float maxPosY = _currentLevel.gameObjects[0].Position.y + _currentLevel.gameObjects[0].GetBounds().size.y/2f;

		// Get position of first non-empty stack
		for(int i = 0; i < _currentLevel.gameObjects.Count; i++)
		{
			float minPosXCandidate = _currentLevel.gameObjects[i].Position.x - _currentLevel.gameObjects[i].GetBounds().size.x/2f;
			if(minPosXCandidate < minPosX)
				minPosX = minPosXCandidate;

			float maxPosXCandidate = _currentLevel.gameObjects[i].Position.x + _currentLevel.gameObjects[i].GetBounds().size.x/2f;
			if(maxPosXCandidate > maxPosX)
				maxPosX = maxPosXCandidate;

			float maxPosYCandidate = _currentLevel.gameObjects[i].Position.y + _currentLevel.gameObjects[i].GetBounds().size.y/2f;
			if(maxPosYCandidate > maxPosY)
				maxPosY = maxPosYCandidate;
		}

		float cameraWidth = Mathf.Abs(minPosX - levelLeftBound) + 
			Mathf.Max(Mathf.Abs(maxPosX - minPosX), Mathf.Abs(maxPosY - groundSurfacePos)) + 0.5f;

		_camera.SetCameraWidth(cameraWidth);		
	}
}
