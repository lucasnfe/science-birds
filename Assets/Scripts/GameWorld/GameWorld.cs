using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : ABSingleton<GameWorld> {

	static int _levelTimesTried;

	private int _birdsThrown;
	private bool _levelCleared;

	private List<Pig>  _pigs;
	private List<Bird> _birds;
	private Bird _lastThrownBird;

	private Transform _groundTransform;
	private Transform _blocksTransform;
	private Transform _birdsTransform;
	private Transform _slingshotTransform;
	private Transform _slingshotBaseTransform;
	private Transform _slingshotFrontTransform;
	private GameObject _levelFailedBanner;
	private GameObject _levelClearedBanner;

	private HUD 		      _hud;
	private BirdAgent         _birdAgent;
	private RectTransform     _pointHUD;

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

	private Vector3 _slingSelectPos;
	public Vector3 SlingSelectPos { 
		get { return _slingSelectPos; }
	}
		
	public GameplayCamera GameplayCam { get; set; }
		
	public GameObject _pig;
	public GameObject _bird;
	public GameObject _point;

	// Game world properties
	public bool    _isSimulation;
	public int     _timesToGiveUp;
	public float   _timeToResetLevel = 1f;

	private GameObject []_templates;
	public AudioClip  []_clips;

	void Awake() {

		_blocksTransform = GameObject.Find ("Blocks").transform;
		_birdsTransform = GameObject.Find ("Birds").transform;
		_groundTransform = GameObject.Find ("Ground").transform;
		_slingshotTransform = GameObject.Find ("Slingshot").transform;
		_slingshotBaseTransform = GameObject.Find ("slingshot_base").transform;
		_slingshotFrontTransform = GameObject.Find ("slingshot_front").transform;

		_levelFailedBanner = GameObject.Find ("LevelFailedBanner").gameObject;
		_levelFailedBanner.gameObject.SetActive (false);

		_levelClearedBanner = GameObject.Find ("LevelClearedBanner").gameObject;
		_levelClearedBanner.gameObject.SetActive(false);

		_hud = GameObject.Find ("HUD").GetComponent<HUD>();
		GameplayCam = GameObject.Find ("Camera").GetComponent<GameplayCamera>();

		if (GameObject.Find ("AI") != null)
			_birdAgent = GameObject.Find ("AI").GetComponent<BirdAgent> ();

		_pointHUD = GameObject.Find ("PointsHUD").GetComponent<RectTransform> ();

		_slingSelectPos = new Vector3 (-0.15f, 0.7f, 1f);
	}

	// Use this for initialization
	void Start () 
	{	
		_pigs = new List<Pig>();
		_birds = new List<Bird>();

		_levelCleared = false;

		// Load block templates and cast them to game objects
		Object[] objs = Resources.LoadAll("Prefabs/GameWorld/Blocks");

		_templates = new GameObject[objs.Length];
		for (int i = 0; i < objs.Length; i++)
			_templates [i] = (GameObject)objs [i];

		if(!_isSimulation) {

			GetComponent<AudioSource>().PlayOneShot(_clips[0]);
			GetComponent<AudioSource>().PlayOneShot(_clips[1]);
		}

		// Calculating slingshot select position
		Vector3 selectPos = _slingshotTransform.position;
		_slingSelectPos += new Vector3 (selectPos.x, selectPos.y, _slingshotFrontTransform.position.z);

		// If there are objects in the scene, use them to play
		if (_blocksTransform.childCount > 0 || _birdsTransform.childCount > 0) {

			foreach(Transform bird in _birdsTransform)
				AddBird (bird.GetComponent<Bird>());

			foreach (Transform block in _blocksTransform) {

				Pig pig = block.GetComponent<Pig>();
				if(pig != null)
					_pigs.Add(pig);
			}

		} else {
			
			ABLevel currentLevel = LevelList.Instance.GetCurrentLevel ();

			if (currentLevel != null){
				
				DecodeLevel (currentLevel.gameObjects, currentLevel.birdsAmount);
				AdaptCameraWidthToLevel ();

				_levelTimesTried = 0;
			}
		}
	}

	public void DecodeLevel(List<ABGameObject> gameObjects, int birdsAmount) 
	{
		ClearWorld();

		foreach(ABGameObject gameObj in gameObjects)
		{
			if(gameObj.Label < GameWorld.Instance._templates.Length)
				
				AddBlock(GameWorld.Instance._templates[gameObj.Label], gameObj.Position, 
				                            GameWorld.Instance._templates[gameObj.Label].transform.rotation);
			else
				AddPig(GameWorld.Instance._pig, gameObj.Position, 
				                          GameWorld.Instance._pig.transform.rotation);
		}

		for(int i = 0; i < birdsAmount; i++)
			AddBird(_bird, _bird.transform.rotation);
		
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

			if(_pigs.Count > 0 && _birds[0] != null && !_birds[0].JumpToSlingshot && _lastThrownBird != _birds[0]) {

				Pig randomPig = _pigs[Random.Range(0, _pigs.Count)];

				_birdAgent.ThrowBird(_birds[0], randomPig, SlingSelectPos);
				_lastThrownBird = _birds[0];
				_birdsThrown++;
			}
		}
	}

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
	
	public bool IsObjectOutOfWorld(Transform abGameObject)
	{
		Vector2 halfSize = abGameObject.GetComponent<Collider2D>().bounds.size/2f;

		Collider2D ground = _groundTransform.GetComponent<Collider2D> ();
		
		if(abGameObject.position.x - halfSize.x > ground.bounds.center.x + ground.bounds.size.x/2f ||
		   abGameObject.position.x + halfSize.x < ground.bounds.center.x - ground.bounds.size.x/2f || 
		   abGameObject.position.y + halfSize.y < ground.bounds.center.y - ground.bounds.size.y/2f)

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
		if(LevelList.Instance.NextLevel() == null)

			ABSceneManager.Instance.LoadScene("MainMenu");
		else
			ABSceneManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void ResetLevel()
	{
		if(_levelFailedBanner.activeSelf)
			_levelTimesTried++;

		ABSceneManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
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

	public void AddBird(Bird readyBird)
	{
		if(_birds.Count == 0)
			readyBird.GetComponent<Rigidbody2D>().gravityScale = 0f;

		if(readyBird != null)
			_birds.Add(readyBird);
	}
	
	public void AddBird(Object original, Quaternion rotation)
	{
		Vector3 birdsPos = SlingSelectPos;

		if(_birds.Count >= 1)
		{
			birdsPos.y = _groundTransform.GetComponent<Collider2D>().bounds.center.y + _groundTransform.GetComponent<Collider2D>().bounds.size.y/2f;

			for(int i = 0; i < _birds.Count; i++)
				birdsPos.x -= _bird.GetComponent<SpriteRenderer>().bounds.size.x * 2f;
		}

		GameObject newGameObject = (GameObject)Instantiate(original, birdsPos, rotation);
		newGameObject.transform.parent = _birdsTransform;
		newGameObject.name = "bird_" + _birds.Count;

		Bird bird = newGameObject.GetComponent<Bird>();

		if(_birds.Count == 0)
			bird.GetComponent<Rigidbody2D>().gravityScale = 0f;

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
		newGameObject.transform.parent = _blocksTransform;

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

		foreach(Transform b in _blocksTransform)
		{
			if(b.GetComponent<Pig>() == null)
			
				for(int i = 0; i < b.GetComponentsInChildren<Rigidbody2D>().Length; i++)
					blocksAmount++;
		}

		return blocksAmount;
	}

	public int GetTemplateIndex(GameObject templateObj)
	{
		for(int i = 0; i < _templates.Length; i++)
		{
			if(templateObj.name == "pig")
				return _templates.Length;

			if(templateObj.name == _templates[i].name)
				return i;
		}

		return -1;
	}

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

	public GameObject GetTemplate(int index) {

		return _templates[index];
	}

	public int GetTemplatesAmount() {

		return _templates.Length;
	}

	public List<GameObject> BlocksInScene() {

		List<GameObject> objsInScene = new List<GameObject>();

		foreach(Transform b in _blocksTransform)
			objsInScene.Add(b.gameObject);

		return objsInScene;
	}

	public Vector3 DragDistance() {

		return _slingshotBaseTransform.transform.position - SlingSelectPos;
	}

	public void SetSlingshotBaseActive(bool isActive) {

		_slingshotBaseTransform.gameObject.SetActive(isActive);
	}

	public void ChangeSlingshotBasePosition(Vector3 position) {

		_slingshotBaseTransform.transform.position = position;
	}

	public void ChangeSlingshotBaseRotation(Quaternion rotation) {

		_slingshotBaseTransform.transform.rotation = rotation;
	}

	public bool IsSlingshotBaseActive() {

		return _slingshotBaseTransform.gameObject.activeSelf;
	}

	public Vector3 GetSlingshotBasePosition() {

		return _slingshotBaseTransform.transform.position;
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

	private void AdaptCameraWidthToLevel() {

		ABLevel currentLevel = LevelList.Instance.GetCurrentLevel ();

		if(currentLevel.gameObjects.Count == 0)
			return;
		
		// Adapt the camera to show all the blocks		
		float levelLeftBound = _groundTransform.transform.position.x - _groundTransform.GetComponent<Collider2D>().bounds.size.x/2f;
		float groundSurfacePos = _groundTransform.transform.position.x + _groundTransform.GetComponent<Collider2D>().bounds.size.y/2f;
				
		float minPosX = currentLevel.gameObjects[0].Position.x - currentLevel.gameObjects[0].GetBounds().size.x/2f;
		float maxPosX = currentLevel.gameObjects[0].Position.x + currentLevel.gameObjects[0].GetBounds().size.x/2f; 
		float maxPosY = currentLevel.gameObjects[0].Position.y + currentLevel.gameObjects[0].GetBounds().size.y/2f;

		// Get position of first non-empty stack
		for(int i = 0; i < currentLevel.gameObjects.Count; i++)
		{
			float minPosXCandidate = currentLevel.gameObjects[i].Position.x - currentLevel.gameObjects[i].GetBounds().size.x/2f;
			if(minPosXCandidate < minPosX)
				minPosX = minPosXCandidate;

			float maxPosXCandidate = currentLevel.gameObjects[i].Position.x + currentLevel.gameObjects[i].GetBounds().size.x/2f;
			if(maxPosXCandidate > maxPosX)
				maxPosX = maxPosXCandidate;

			float maxPosYCandidate = currentLevel.gameObjects[i].Position.y + currentLevel.gameObjects[i].GetBounds().size.y/2f;
			if(maxPosYCandidate > maxPosY)
				maxPosY = maxPosYCandidate;
		}

		float cameraWidth = Mathf.Abs(minPosX - levelLeftBound) + 
			Mathf.Max(Mathf.Abs(maxPosX - minPosX), Mathf.Abs(maxPosY - groundSurfacePos)) + 0.5f;

		GameplayCam.SetCameraWidth(cameraWidth);		
	}
}
