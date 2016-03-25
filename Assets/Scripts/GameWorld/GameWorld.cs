using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/** \class GameWorld
 *  \brief  Contains all the information about the game world
 *
 *  Contains info about pigs, birds, slingshot, blocks, score, HUD, camera and other things that builds the world.
 *  Also contains info about winning and losing a game, level stability and destroying and creating the game objects.
 */
public class GameWorld : ABSingleton<GameWorld> {
    /**Counts how many times the player tried to pass the level*/
	static int _levelTimesTried;
    /**Counts how many birds have been thrown*/
	private int _birdsThrown;
    /**True if level has been cleared, false otherwise*/
	private bool _levelCleared;
    /**List of pigs in the level*/
	private List<Pig>  _pigs;
    /**List of birds in the level*/
	private List<Bird> _birds;
    /**Bird object of last thrown bird*/
	private Bird _lastThrownBird;
    /**Angry Birds Level object containing current level*/
	private static ABLevel _currentLevel;
    /**Number of pigs at the start of the level*/
	private int _pigsAtStart;
    /**Level current stability*/
    private float stability;
    /**Current level max velocity of blocks*/
    private float maxVelocity;
    /**Accessor containing getter for number of pigs at the start of the level*/
	public int PigsAtStart { 
		get { return _pigsAtStart; }
	}
	/**Number of birds at the start of the level*/
	private int _birdsAtStart;
    /**Accessor containing the getter for number of birds at the start of the level*/
	public int BirdsAtStart { 
		get { return _birdsAtStart; }
	}
    /**Number of blocks at the start of the level*/
    private int _blocksAtStart;
    /**Accessor containing the getter for number of blocks at the start of the level*/
    public int BlocksAtStart { 
		get { return _blocksAtStart; }
	}
    /**Stability of the blocks until first bird is thrown*/
    private float _stabilityUntilFirstBird;
    /**Accessor containing the getter for the stability of the blocks until first bird is thrown*/
    public float StabilityUntilFirstBird { 
		get { return _stabilityUntilFirstBird; }
	}

	// Main game components
    /**transform for slingshot object*/
	public Transform _slingshotTransform;
    /**transform for slingshot's base object*/
    public Transform _slingshotBaseTransform;
    /**transform for slingshot's front object*/
    public Transform _slingshotFrontTransform;
    /**transform for ground object*/
    public Transform _groundTransform;
    /**transform for blocks object*/
    public Transform _blocksTransform;
    /**transform for birds object*/
    public Transform _birdsTransform;
    /**Game object for the pig*/
	public GameObject _pig;
    /**Game object for the bird*/
    public GameObject _bird;
    /**Game object for the score (point)*/
    public GameObject _point;
    /**Game object for the level failed banner*/
    public GameObject _levelFailedBanner;
    /**Game object for the level cleared banner*/
    public GameObject _levelClearedBanner;

    /**Object for the level source*/
	public LevelSource    _levelSource;
    /**Object for the HUD*/
    public HUD 			  _hud;
    /**Object for the camera*/
	public GameplayCamera _camera;
    /**Object for the Bird Agent AI*/
	public BirdAgent      _birdAgent;
    /**Object for the Rectangle Transform containing the point (score) HUD*/
	public RectTransform  _pointHUD;

	// Game world properties
    /**Check if running simulation or not*/
	public bool    _isSimulation;
    /**Total of tries before forcing to giving up*/
	public int     _timesToGiveUp;
    /**Time before showing the level cleared or failed banner*/
    public float   _timeToResetLevel = 1f;
    /**Slingshot select position*/
	public Vector3 _slingSelectPos;

    /**Game Object's templates containing blocks and pigs*/
	public GameObject []Templates;
    /**Audio clips for the game music*/
	public AudioClip  []_clips;
	
	/**
     *  At initialization, creates new List of pigs and birds, sets level cleared to false, if is not simulation play
     *  the audio. Calculate's slingshot select position based on slingshot's position. If current level is null and 
     *  level source is not, current level is now the next one and number of tries in the level goes to 0.
     *  if current level is not null, decode the game objects and bird's amount, creating them on level, and
     *  adjusts camera to fit the width of the level.
     */
	void Start () 
	{	
		_pigs = new List<Pig>();
		_birds = new List<Bird>();
		_levelCleared = false;
        _stabilityUntilFirstBird = 0f;
		if(!_isSimulation) {

			GetComponent<AudioSource>().PlayOneShot(_clips[0]);
			GetComponent<AudioSource>().PlayOneShot(_clips[1]);
		}

		// Calculating slingshot select position
		Vector3 selectPos = _slingshotTransform.position;
		_slingSelectPos.x += selectPos.x;
		_slingSelectPos.y += selectPos.y;
		_slingSelectPos.z += _slingshotFrontTransform.position.z;

		if(_currentLevel == null && _levelSource != null) 
		{
			_currentLevel = _levelSource.NextLevel();
			_levelTimesTried = 0;
		}

		if(_currentLevel != null) 
		{
			DecodeLevel(_currentLevel.gameObjects, _currentLevel.birdsAmount);
			AdaptCameraWidthToLevel();
		}
	}
    /**
     *  Decodes the list of game objects in objects of the world. Adds blocks and pigs in their respective positions
     *  and with their given rotations, places the first bird in the slingshot, and the others behind it.
     *  @param[in]  gameObjects List of ABGameObjects containing all objects of the level
     *  @param[in]  birdsAmount amount of birds in the level
     */
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
			Vector3 birdsPos = _slingshotTransform.transform.position;
			birdsPos.y = _groundTransform.GetComponent<Collider2D>().bounds.center.y + _groundTransform.GetComponent<Collider2D>().bounds.size.y/2f;
			
			for(int i = 0; i < birdsAmount; i++)
			{
				birdsPos.x -= _bird.GetComponent<SpriteRenderer>().bounds.size.x * 2f;
				GameWorld.Instance.AddBird(_bird, birdsPos, _bird.transform.rotation, "bird" + (i+1));
			}
		}
		
		StartWorld();
	}

	/**
     *  Once per frame checks if bird was thrown and died, and if it did, swaps if needed. Also destroys objects
     *  out of screen, activates the game AI if it is set in the game and the blocks are stable.
     *  When AI is activated, chooses randomly a pig and makes it the target to be shot at.
     */
	void Update ()
	{
		// Check if birds was trown, if it died and swap them when needed
		ManageBirds();
		
		// If an object goes out of screen, destroy it
		DestroyIfOutScreen();

        // Activate game AI if it is set
        if (_birdAgent != null && !_birdAgent.IsThrowingBird)
        {
            if (_birds.Count == 0)
                return;

            // Calculate stability until first birds is trown
            stability = GetLevelStability();
            if (_birdsThrown == 0)
            {
                //Normalize it by the max velocity of all blocks summed.
                _stabilityUntilFirstBird += stability/maxVelocity;
            }
            // Wait the level stay stable before throwing next bird
			if(stability != 0f)
                return;

			if(_pigs.Count > 0 && _birds[0] != null && !_birds[0].JumpToSlingshot && _lastThrownBird != _birds[0]) {

				Pig randomPig = _pigs[Random.Range(0, _pigs.Count)];

				_birdAgent.ThrowBird(_birds[0], randomPig, _slingSelectPos);
				_lastThrownBird = _birds[0];
				_birdsThrown++;
			}
		}
	}

    /**
     *  Takes all the blocks and birds of the level and check if their transform are out of world.
     *  If any of them are, destroy them. If a pig or a bird, calls the Die() method.
     */
	void DestroyIfOutScreen()
	{
		Rigidbody2D []bodies = _blocksTransform.GetComponentsInChildren<Rigidbody2D>();

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
	
		foreach(Transform b in _birdsTransform)
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
	
    /**
     *  Checks if the transform of the game object is out of the world bounds.
     *  @param[in]  abGameObject    transform of a game object
     *  @return bool    True if is out of the world, false otherwise
     */
	public bool IsObjectOutOfWorld(Transform abGameObject)
	{
		Vector2 halfSize = abGameObject.GetComponent<Collider2D>().bounds.size/2f;
		
		if(abGameObject.position.x - halfSize.x > _groundTransform.GetComponent<Collider2D>().bounds.center.x + _groundTransform.GetComponent<Collider2D>().bounds.size.x/2f ||
		   abGameObject.position.x + halfSize.x < _groundTransform.GetComponent<Collider2D>().bounds.center.x - _groundTransform.GetComponent<Collider2D>().bounds.size.x/2f || 
		   abGameObject.position.y + halfSize.y < _groundTransform.GetComponent<Collider2D>().bounds.center.y - _groundTransform.GetComponent<Collider2D>().bounds.size.y/2f)

			   return true;
		
		return false;
	}

    /**
     *  If a bird needs to be put on the slingshot, calls the method to put it on.
     */
	void ManageBirds()
	{
		if(_birds.Count == 0)
			return;
		
		// Move next bird to the slingshot
		if(_birds[0].JumpToSlingshot)
			_birds[0].SetBirdOnSlingshot();
	}

    /**
     *  If there is a bird, return the one in position 0 of the array of birds
     *  @return Bird    Bird at position 0 in the array of birds.
     */
	public Bird GetCurrentBird()
	{
		if(_birds.Count > 0)
			return _birds[0];
		
		return null;
	}

    /**
     *  If all levels were already played, loads the questionary scene, 
     *  Else, loads the next level to be played
     */
	public void NextLevel()
	{
		_currentLevel = null;

		if(LevelSource.CurrentLevel % _levelSource.LevelLimit() == 0)

			ABSceneManager.Instance.LoadScene(GameData.Instance.CurrentQuestionary);
		else
			ABSceneManager.Instance.LoadScene(Application.loadedLevelName);
	}

    /**
     *  If player failed the level, adds 1 to the times of tries for the level, and reload the level.
     */
	public void ResetLevel()
	{
		if(_levelFailedBanner.activeSelf)
			_levelTimesTried++;

		ABSceneManager.Instance.LoadScene(Application.loadedLevelName);
	}

    /**
     *  Instantiates the particle, places it in the right spot, adds the parent name to it, animates it and, if
     *  animated, destroys it.
     *  @param[in]  particleTemplate    Game object containing template for particles
     *  @param[in]  position    Vector3 with desired position for particle
     *  @param[in]  parentName  Name of the particle
     */
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
	/**
     *  adds a bird game object to the level, if is the first one zeroes its gravity scale, and if 
     *  a bird really was added, adds it to the vector of birds of the world.
     *  @param[in]  original    original object of the bird
     *  @param[in]  position    the bird's position
     *  @param[in]  rotation    the bird's rotation
     *  @param[in]  birdname    the name of the bird
     *  @param[in]  isFirst     true if bird is first on the array of birds, false otherwise;
     */
	public void AddBird(Object original, Vector3 position, Quaternion rotation, string birdname, bool isFirst = false)
	{
		GameObject newGameObject = (GameObject)Instantiate(original, position, rotation);
		newGameObject.transform.parent = _birdsTransform;
		newGameObject.name = birdname;

		Bird bird = newGameObject.GetComponent<Bird>();

		if(isFirst)
			bird.GetComponent<Rigidbody2D>().gravityScale = 0f;

		if(bird != null)
			_birds.Add(bird);
	}

    /**
     *  Adds a pig game object to the world, and if not null, adds it to the pigs array
     *  @param[in]  original    the pig's original object
     *  @param[in]  position    the pig's position
     *  @param[in]  rotation    the pig's rotation
     */
	public void AddPig(Object original, Vector3 position, Quaternion rotation)
	{
		GameObject newGameObject = AddBlock(original, position, rotation);

		Pig pig = newGameObject.GetComponent<Pig>();
		if(pig != null)
			_pigs.Add(pig);
	}
    /**
     *  Adds a block game object to the world.
     *  @param[in]  original    the block's original object
     *  @param[in]  position    the block's position
     *  @param[in]  rotation    the block's rotation
     *  @return     GameObject  the game object corresponding to the block
     */
    public GameObject AddBlock(Object original, Vector3 position, Quaternion rotation)
	{
		GameObject newGameObject = (GameObject)Instantiate(original, position, rotation);
		newGameObject.transform.parent = _blocksTransform;

		return newGameObject;
	}

    /**
     *  Spawns the score of the player with the corresponding points in the desired position.
     *  Then, adds the points to the total score.
     *  @param[in]  point       The value of the score earned
     *  @param[in]  position    Position to spawn the score
     */
	public void SpawnPoint(uint point, Vector3 position)
	{
		GameObject pointObj = Instantiate(_point, new Vector3(position.x, position.y, _point.transform.position.z), Quaternion.identity) as GameObject; 
		pointObj.transform.SetParent(_pointHUD.transform);

		Text pointText = pointObj.GetComponent<Text>();
		pointText.text = point.ToString();

		_hud.AddScore(point);
	}

    /**
     *  Shows the level failed banner if the game is stable and the player lost the game.
     *  If the game is unstable, waits 1s to Invoke again this same method.
     *  If the player still has not wasted all his tries, activates the level failed banner.
     *  If it has already wasted them, activates the level cleared banner and adds the level failde text;
     */
    private void ShowLevelFailedBanner() 
	{
		if(_levelCleared)
			return;

		if(!IsLevelStable())
		{
			Invoke("ShowLevelFailedBanner", 1f);
		}
		else
		{
			// Player lost the game
			_hud.gameObject.SetActive(false);

			if(_levelTimesTried < _timesToGiveUp - 1)

				_levelFailedBanner.SetActive(true);
			else
			{
				_levelClearedBanner.SetActive(true);
				_levelClearedBanner.GetComponentInChildren<Text>().text = "Level Failed!";
			}
		}
	}

    /**
     *  If level not stable waits 1s and re-Invoke this method, else, activates the hud and the level cleared
     *  banner with the text level cleared.
     */
	private void ShowLevelClearedBanner() 
	{
		if(!IsLevelStable())
		{
			Invoke("ShowLevelClearedBanner", 1f);
		}
		else
		{
			// Player won the game
			_hud.gameObject.SetActive(false);
			_levelClearedBanner.SetActive(true);
			_levelClearedBanner.GetComponentInChildren<Text>().text = "Level Cleared!";
		}
	}

    /**
     *  Kills the pig, removing it from the pigs' array, and if last pig, check if not a simulation.
     *  If it isn't, the player has won, sets the level cleared flag to true and Invoke the ShowLevelClearedBanner() method
     *  @param[in]  pig The pig to be killed.
     */
	public void KillPig(Pig pig)
	{
		_pigs.Remove(pig);
		
		if(_pigs.Count == 0)
		{
			// Check if player won the game
			if(!_isSimulation) {
				_levelCleared = true;
				Invoke("ShowLevelClearedBanner", _timeToResetLevel);
			}
			
			return;
		}
	}

    /**
     *  Kills the bird, removing it from the birds' array, and if last bird, check if not a simulation.
     *  If it isn't, the player has lost, Invoke the ShowLevelFailedBanner() method.
     *  At last sets the gravity scale of the new first bird to 0 and activates the flag to put it in slingshot.
     *  @param[in]  bird The bird to be killed.
     */
    public void KillBird(Bird bird)
	{
		_birds.Remove(bird);
		
		if(_birds.Count == 0)
		{
			// Check if player lost the game
			if(!_isSimulation)
				Invoke("ShowLevelFailedBanner", _timeToResetLevel);

			return;
		}
		
		_birds[0].GetComponent<Rigidbody2D>().gravityScale = 0f;
		_birds[0].JumpToSlingshot = true;
	}

    /**
     *  If the last bird exists, finds the child transform of the world and destroys each of them with the
     *  Same name of the actual bird.
     *  @param[in]  currentBirdName name of the bird that has the particles to be destroyed. 
     */
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

    /**
     *  Getter for the amount of pigs available in the level.
     *  @return int amount of pigs available
     */
	public int GetPigsAvailableAmount()
	{
		return _pigs.Count;
	}
    /**
     *  Getter for the amount of birds available in the level.
     *  @return int amount of birds available
     */
    public int GetBirdsAvailableAmount()
	{
		return _birds.Count;
	}
    /**
     *  Getter for the amount of blocks available in the level.
     *  if not a pig, adds to the amount.
     *  @return int amount of blocks available
     */
    public int GetBlocksAvailableAmount()
	{
		int blocksAmount = 0;
       
        foreach (Transform b in _blocksTransform)
		{
			if(b.GetComponent<Pig>() == null)
			
				for(int i = 0; i < b.GetComponentsInChildren<Rigidbody2D>().Length; i++)
					blocksAmount++;
		}

		return blocksAmount;
	}

    /**
     *  Get the index for the template object, if a pig it is the last index, if not, checks what index holds that name.
     *  @param[in]  templateObj Object to search for the index.
     *  @return int index of the object, -1 if no object found with the same name.
     */
	public int GetTemplateIndex(GameObject templateObj)
	{
		for(int i = 0; i < Templates.Length; i++)
		{
			if(templateObj.name == "pig")
				return Templates.Length;

			if(templateObj.name == Templates[i].name)
				return i;
		}

		return -1;
	}

    /**
     *  Sums the velocity magnitude of each block in the world and returns this sum.
     *  @return float   sum of the velocities of all the blocks inside the world.
     */
	public float GetLevelStability()
	{
		float totalVelocity = 0f;

		foreach(Transform b in _blocksTransform)
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
    /**
     *  Sums the maximun velocity of each block in the world and returns this sum.
     *  @return float   sum of the velocities of all the blocks inside the world.
     */
    public float GetLevelMaxVelocity()
    {
        float maxVelocity = 0f;

        foreach (Transform b in _blocksTransform)
        {
            Rigidbody2D[] bodies = b.GetComponentsInChildren<Rigidbody2D>();

            foreach (Rigidbody2D body in bodies)
            {
                if (!IsObjectOutOfWorld(body.transform))
                {
                    maxVelocity += Physics2D.maxTranslationSpeed;
                }
            }
        }

        return maxVelocity;
    }

    /**
     *  If everything has 0 velocity in the world, it is stable
     *  @return bool    True if stable, false otherwise
     */
    public bool IsLevelStable()
	{
		return GetLevelStability() == 0f;
	}
	
    /**
     *  Starts the world by saving the amount of pigs, blocks and bird available.
     *  And getting the maximum velocity that all the block in the level can have.
     */
	public void StartWorld()
	{
		_pigsAtStart = GetPigsAvailableAmount();
		_birdsAtStart = GetBirdsAvailableAmount();
		_blocksAtStart = GetBlocksAvailableAmount();
        maxVelocity = GetLevelMaxVelocity();
	}

    /**
     *  CLears the world, destroying every block object, clearing the array of pigs, destroying each bird object,
     *  clearing the array of birds, destroying every Effects objects, and setting to zero the amounts of birds
     *  thrown and stability until first bird. 
     */
	public void ClearWorld()
	{
		foreach(Transform b in _blocksTransform)
		{
			Destroy(b.gameObject);
		}

		_pigs.Clear();

		foreach(Transform b in _birdsTransform)
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
    /**
     *  Adapts the camera width to show all the blocks. Checks the game objects to calculate min and max X position of camera
     *  As well as its heigth.
     */
	private void AdaptCameraWidthToLevel() {

		if(_currentLevel.gameObjects.Count == 0)
			return;
		
		// Adapt the camera to show all the blocks		
		float levelLeftBound = _groundTransform.transform.position.x - _groundTransform.GetComponent<Collider2D>().bounds.size.x/2f;
		float groundSurfacePos = _groundTransform.transform.position.x + _groundTransform.GetComponent<Collider2D>().bounds.size.y/2f;
				
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
