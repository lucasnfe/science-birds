// SCIENCE BIRDS: A clone version of the Angry Birds game used for 
// research purposes
// 
// Copyright (C) 2016 - Lucas N. Ferreira - lucasnfe@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
//

﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ABGameWorld : ABSingleton<ABGameWorld> {

	static int _levelTimesTried;

	private bool _levelCleared;

	private List<ABPig>      _pigs;
	private List<ABBird>     _birds;
	private List<ABParticle> _birdTrajectory;

	private ABBird     _lastThrownBird;
	private Transform  _blocksTransform;
	private Transform  _birdsTransform;
	private Transform  _plaftformsTransform;
	private Transform  _slingshotBaseTransform;

	private GameObject _slingshot;
	public GameObject Slingshot() { return _slingshot; }

	private GameObject _levelFailedBanner;
	public bool LevelFailed() { return _levelFailedBanner.activeSelf; }

	private GameObject _levelClearedBanner;
	public bool LevelCleared() {  return _levelClearedBanner.activeSelf; }

	private int _pigsAtStart;
	public int PigsAtStart {  get { return _pigsAtStart; } }
	
	private int _birdsAtStart;
	public int BirdsAtStart { get { return _birdsAtStart; } }

	private int _blocksAtStart;
	public int BlocksAtStart { get { return _blocksAtStart; } }

	public ABGameplayCamera GameplayCam { get; set; }
	public float LevelWidth  { get; set; }
	public float LevelHeight { get; set; }
		
	// Game world properties
	public bool    _isSimulation;
	public int     _timesToGiveUp;
	public float   _timeToResetLevel = 1f;
	public int 	   _birdsAmounInARow = 5;

	public AudioClip  []_clips;

	void Awake() {

		_blocksTransform = GameObject.Find ("Blocks").transform;
		_birdsTransform  = GameObject.Find ("Birds").transform;
		_plaftformsTransform = GameObject.Find ("Platforms").transform;

		_levelFailedBanner = GameObject.Find ("LevelFailedBanner").gameObject;
		_levelFailedBanner.gameObject.SetActive (false);

		_levelClearedBanner = GameObject.Find ("LevelClearedBanner").gameObject;
		_levelClearedBanner.gameObject.SetActive(false);

		GameplayCam = GameObject.Find ("Camera").GetComponent<ABGameplayCamera>();
	}

	// Use this for initialization
	void Start () {
		
		_pigs = new List<ABPig>();
		_birds = new List<ABBird>();
		_birdTrajectory = new List<ABParticle>();

		_levelCleared = false;

		if(!_isSimulation) {

			GetComponent<AudioSource>().PlayOneShot(_clips[0]);
			GetComponent<AudioSource>().PlayOneShot(_clips[1]);
		}

		GameObject slingshot = GameObject.Find ("Slingshot");

		// If there are objects in the scene, use them to play
		if (_blocksTransform.childCount     > 0 || _birdsTransform.childCount > 0 || 
			_plaftformsTransform.childCount > 0 || slingshot != null ) {

			foreach(Transform bird in _birdsTransform)
				AddBird (bird.GetComponent<ABBird>());

			foreach (Transform block in _blocksTransform) {

				ABPig pig = block.GetComponent<ABPig>();
				if(pig != null)
					_pigs.Add(pig);
			}

			LevelHeight = ABConstants.LEVEL_ORIGINAL_SIZE.y;
			LevelWidth = ABConstants.LEVEL_ORIGINAL_SIZE.x;

			_slingshot = GameObject.Find ("Slingshot");
		} 
		else {
			
			ABLevel currentLevel = LevelList.Instance.GetCurrentLevel ();

			if (currentLevel != null) {
				
				DecodeLevel (currentLevel);
				AdaptCameraWidthToLevel ();

				_levelTimesTried = 0;
			}
		}

		_slingshotBaseTransform = GameObject.Find ("slingshot_base").transform;
	}

	public void DecodeLevel(ABLevel currentLevel)  {
		
		ClearWorld();

		LevelHeight = ABConstants.LEVEL_ORIGINAL_SIZE.y;
		LevelWidth = (float)currentLevel.width * ABConstants.LEVEL_ORIGINAL_SIZE.x;

		Vector3 cameraPos = GameplayCam.transform.position;
		cameraPos.x = currentLevel.camera.x;
		cameraPos.y = currentLevel.camera.y;
		GameplayCam.transform.position = cameraPos;

		GameplayCam._minWidth = currentLevel.camera.minWidth;
		GameplayCam._maxWidth = currentLevel.camera.maxWidth;

		Vector3 landscapePos = ABWorldAssets.LANDSCAPE.transform.position;
		Vector3 backgroundPos = ABWorldAssets.BACKGROUND.transform.position;

		if (currentLevel.width > 1) {

			landscapePos.x -= LevelWidth / 4f;
			backgroundPos.x -= LevelWidth/ 4f;
		}

		for (int i = 0; i < currentLevel.width; i++) {

			GameObject landscape = (GameObject)Instantiate(ABWorldAssets.LANDSCAPE, landscapePos, Quaternion.identity);
			landscape.transform.parent = transform;

			float screenRate = currentLevel.camera.maxWidth / LevelHeight;
			if (screenRate > 2f) {

				for (int j = 0; j < (int)screenRate; j++) {
					
					Vector3 deltaPos = Vector3.down * (LevelHeight/1.5f + (j * 2f));
					Instantiate(ABWorldAssets.GROUND_EXTENSION, landscapePos + deltaPos, Quaternion.identity);
				}
			}

			landscapePos.x += ABConstants.LEVEL_ORIGINAL_SIZE.x - 0.01f;

			GameObject background = (GameObject)Instantiate(ABWorldAssets.BACKGROUND, backgroundPos, Quaternion.identity);
			background.transform.parent = GameplayCam.transform;
			backgroundPos.x += ABConstants.LEVEL_ORIGINAL_SIZE.x - 0.01f;
		}

		Vector2 slingshotPos = new Vector2 (currentLevel.slingshot.x, currentLevel.slingshot.y);
		_slingshot = (GameObject)Instantiate(ABWorldAssets.SLINGSHOT, slingshotPos, Quaternion.identity);
		_slingshot.name = "Slingshot";
		_slingshot.transform.parent = transform;

		foreach(BirdData gameObj in currentLevel.birds){

			AddBird(ABWorldAssets.BIRDS[gameObj.type], ABWorldAssets.BIRDS[gameObj.type].transform.rotation);
		}

		foreach (OBjData gameObj in currentLevel.pigs) {

			Vector2 pos = new Vector2 (gameObj.x, gameObj.y);
			Quaternion rotation = Quaternion.Euler (0, 0, gameObj.rotation);
			AddPig(ABWorldAssets.PIGS[gameObj.type], pos, rotation);
		}

		foreach(BlockData gameObj in currentLevel.blocks) {

			Vector2 pos = new Vector2 (gameObj.x, gameObj.y);
			Quaternion rotation = Quaternion.Euler (0, 0, gameObj.rotation);

			GameObject block = AddBlock(ABWorldAssets.BLOCKS[gameObj.type], pos,  rotation);

			MATERIALS material = (MATERIALS)System.Enum.Parse(typeof(MATERIALS), gameObj.material);
			block.GetComponent<ABBlock> ().SetMaterial (material);
		}

		foreach(PlatData gameObj in currentLevel.platforms) {
			
			Vector2 pos = new Vector2 (gameObj.x, gameObj.y);
			Quaternion rotation = Quaternion.Euler (0, 0, gameObj.rotation);

			AddPlatform(ABWorldAssets.PLATFORM, pos, rotation, gameObj.scaleX, gameObj.scaleY);
		}

		foreach(OBjData gameObj in currentLevel.tnts) {

			Vector2 pos = new Vector2 (gameObj.x, gameObj.y);
			Quaternion rotation = Quaternion.Euler (0, 0, gameObj.rotation);

			AddBlock(ABWorldAssets.TNT, pos, rotation);
		}
		
		StartWorld();
	}

	// Update is called once per frame
	void Update () {
		
		// Check if birds was trown, if it died and swap them when needed
		ManageBirds();
	}
	
	public bool IsObjectOutOfWorld(Transform abGameObject, Collider2D abCollider) {
		
		Vector2 halfSize = abCollider.bounds.size/2f;
	
		if(abGameObject.position.x - halfSize.x > LevelWidth/2f ||
		   abGameObject.position.x + halfSize.x < -LevelWidth/2f)

			   return true;
		
		return false;
	}

	void ManageBirds() {
		
		if(_birds.Count == 0)
			return;
		
		// Move next bird to the slingshot
		if(_birds[0].JumpToSlingshot)
			_birds[0].SetBirdOnSlingshot();

//		int birdsLayer = LayerMask.NameToLayer("Birds");
//		int blocksLayer = LayerMask.NameToLayer("Blocks");
//		if(_birds[0].IsFlying || _birds[0].IsDying)
//			
//			Physics2D.IgnoreLayerCollision(birdsLayer, blocksLayer, false);
//		else 
//			Physics2D.IgnoreLayerCollision(birdsLayer, blocksLayer, true);
	}

	public ABBird GetCurrentBird() {
		
		if(_birds.Count > 0)
			return _birds[0];
		
		return null;
	}

	public void NextLevel() {
		
		if(LevelList.Instance.NextLevel() == null)

			ABSceneManager.Instance.LoadScene("MainMenu");
		else
			ABSceneManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void ResetLevel() {
		
		if(_levelFailedBanner.activeSelf)
			_levelTimesTried++;

		ABSceneManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void AddTrajectoryParticle(ABParticle trajectoryParticle) {

		_birdTrajectory.Add (trajectoryParticle);
	}

	public void RemoveLastTrajectoryParticle() {

		foreach (ABParticle part in _birdTrajectory)
			part.Kill ();
	}

	public void AddBird(ABBird readyBird) {
		
		if(_birds.Count == 0)
			readyBird.GetComponent<Rigidbody2D>().gravityScale = 0f;

		if(readyBird != null)
			_birds.Add(readyBird);
	}
	
	public GameObject AddBird(GameObject original, Quaternion rotation) {
		
		Vector3 birdsPos = _slingshot.transform.position - ABConstants.SLING_SELECT_POS;

		if(_birds.Count >= 1) {
			
			birdsPos.y = _slingshot.transform.position.y;

			for (int i = 0; i < _birds.Count; i++) {

				if ((i + 1) % _birdsAmounInARow == 0) {

					float coin = (Random.value < 0.5f ? 1f : -1);
					birdsPos.x = _slingshot.transform.position.x + (Random.value * 0.5f * coin);
				}
					
				birdsPos.x -= ABWorldAssets.BIRDS [original.name].GetComponent<SpriteRenderer> ().bounds.size.x * 1.75f;
			}
		}

		GameObject newGameObject = (GameObject)Instantiate(original, birdsPos, rotation);
		Vector3 scale = newGameObject.transform.localScale;
		scale.x = original.transform.localScale.x;
		scale.y = original.transform.localScale.y;
		newGameObject.transform.localScale = scale;

		newGameObject.transform.parent = _birdsTransform;
		newGameObject.name = "bird_" + _birds.Count;

		ABBird bird = newGameObject.GetComponent<ABBird>();
		bird.SendMessage ("InitSpecialPower", SendMessageOptions.DontRequireReceiver);

		if(_birds.Count == 0)
			bird.GetComponent<Rigidbody2D>().gravityScale = 0f;

		if(bird != null)
			_birds.Add(bird);

		return newGameObject;
	}

	public GameObject AddPig(GameObject original, Vector3 position, Quaternion rotation, float scale = 1f) {
		
		GameObject newGameObject = AddBlock(original, position, rotation, scale);

		ABPig pig = newGameObject.GetComponent<ABPig>();
		if(pig != null)
			_pigs.Add(pig);

		return newGameObject;
	}

	public GameObject AddPlatform(GameObject original, Vector3 position, Quaternion rotation, float scaleX = 1f, float scaleY = 1f) {

		GameObject platform = AddBlock (original, position, rotation, scaleX, scaleY);
		platform.transform.parent = _plaftformsTransform;

		return platform;
	}

	public GameObject AddBlock(GameObject original, Vector3 position, Quaternion rotation, float scaleX = 1f, float scaleY = 1f) {
		
		GameObject newGameObject = (GameObject)Instantiate(original, position, rotation);
		newGameObject.transform.parent = _blocksTransform;

		Vector3 newScale = newGameObject.transform.localScale;
		newScale.x = scaleX;
		newScale.y = scaleY;
		newGameObject.transform.localScale = newScale;

		return newGameObject;
	}

	private void ShowLevelFailedBanner()  {
		
		if(_levelCleared)
			return;

		if(!IsLevelStable()) {

			Invoke("ShowLevelFailedBanner", 1f);
		}
		else {
			
			// Player lost the game
			HUD.Instance.gameObject.SetActive(false);

			if (_levelTimesTried < _timesToGiveUp - 1) {

				_levelFailedBanner.SetActive (true);
			}
			else {
				
				_levelClearedBanner.SetActive(true);
				_levelClearedBanner.GetComponentInChildren<Text>().text = "Level Failed!";
			}
		}
	}

	private void ShowLevelClearedBanner() 
	{
		if(!IsLevelStable()) {

			Invoke("ShowLevelClearedBanner", 1f);
		}
		else {
			
			// Player won the game
			HUD.Instance.gameObject.SetActive(false);

			_levelClearedBanner.SetActive(true);
			_levelClearedBanner.GetComponentInChildren<Text>().text = "Level Cleared!";
		}
	}

	public void KillPig(ABPig pig) {
		
		_pigs.Remove(pig);
		
		if(_pigs.Count == 0) {
			
			// Check if player won the game
			if(!_isSimulation) {

				_levelCleared = true;
				Invoke("ShowLevelClearedBanner", _timeToResetLevel);
			}
			
			return;
		}
	}
	
	public void KillBird(ABBird bird) {

		if (!_birds.Contains (bird))
			return;
		
		_birds.Remove(bird);
		
		if(_birds.Count == 0) {
			
			// Check if player lost the game
			if(!_isSimulation)
				Invoke("ShowLevelFailedBanner", _timeToResetLevel);

			return;
		}
		
		_birds[0].GetComponent<Rigidbody2D>().gravityScale = 0f;
		_birds[0].JumpToSlingshot = true;
	}

	public int GetPigsAvailableAmount() {
		
		return _pigs.Count;
	}
	
	public int GetBirdsAvailableAmount() {
		
		return _birds.Count;
	}

	public int GetBlocksAvailableAmount() {
		
		int blocksAmount = 0;

		foreach(Transform b in _blocksTransform) {
			
			if(b.GetComponent<ABPig>() == null)
			
				for(int i = 0; i < b.GetComponentsInChildren<Rigidbody2D>().Length; i++)
					blocksAmount++;
		}

		return blocksAmount;
	}

	public bool IsLevelStable() {
		
		return GetLevelStability() == 0f;
	}

	public float GetLevelStability() {
		
		float totalVelocity = 0f;

		foreach(Transform b in _blocksTransform) {
			
			Rigidbody2D []bodies = b.GetComponentsInChildren<Rigidbody2D>();

			foreach(Rigidbody2D body in bodies) {
				
				if(!IsObjectOutOfWorld(body.transform, body.GetComponent<Collider2D>()))
					totalVelocity += body.velocity.magnitude;
			}
		}

		return totalVelocity;
	}

	public List<GameObject> BlocksInScene() {

		List<GameObject> objsInScene = new List<GameObject>();

		foreach(Transform b in _blocksTransform)
			objsInScene.Add(b.gameObject);

		return objsInScene;
	}

	public Vector3 DragDistance() {

		Vector3 selectPos = (_slingshot.transform.position - ABConstants.SLING_SELECT_POS);
		return _slingshotBaseTransform.transform.position - selectPos;
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

	public void StartWorld() {
		
		_pigsAtStart = GetPigsAvailableAmount();
		_birdsAtStart = GetBirdsAvailableAmount();
		_blocksAtStart = GetBlocksAvailableAmount();
	}

	public void ClearWorld() {
		
		foreach(Transform b in _blocksTransform)
			Destroy(b.gameObject);

		_pigs.Clear();

		foreach(Transform b in _birdsTransform)
			Destroy(b.gameObject);

		_birds.Clear();		
	}

	private void AdaptCameraWidthToLevel() {

		Collider2D []bodies = _blocksTransform.GetComponentsInChildren<Collider2D>();

		if(bodies.Length == 0)
			return;
		
		// Adapt the camera to show all the blocks		
		float levelLeftBound = -LevelWidth/2f;
		float groundSurfacePos = LevelHeight/2f;
				
		float minPosX = Mathf.Infinity;
		float maxPosX = -Mathf.Infinity; 
		float maxPosY = -Mathf.Infinity;

		// Get position of first non-empty stack
		for(int i = 0; i < bodies.Length; i++)
		{
			float minPosXCandidate = bodies[i].transform.position.x - bodies[i].bounds.size.x/2f;
			if(minPosXCandidate < minPosX)
				minPosX = minPosXCandidate;

			float maxPosXCandidate = bodies[i].transform.position.x + bodies[i].bounds.size.x/2f;
			if(maxPosXCandidate > maxPosX)
				maxPosX = maxPosXCandidate;

			float maxPosYCandidate = bodies[i].transform.position.y + bodies[i].bounds.size.y/2f;
			if(maxPosYCandidate > maxPosY)
				maxPosY = maxPosYCandidate;
		}

		float cameraWidth = Mathf.Abs(minPosX - levelLeftBound) + 
			Mathf.Max(Mathf.Abs(maxPosX - minPosX), Mathf.Abs(maxPosY - groundSurfacePos)) + 0.5f;

		GameplayCam.SetCameraWidth(cameraWidth);		
	}
}
