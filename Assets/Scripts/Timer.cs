using UnityEngine;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;

public struct PigData
{
	public float averageVelocity;
	public int collisionAmount;
};

public struct BlockData
{
	public float averageVelocity;
	public int collisionAmount;
	public float rotation;
};

public struct GameData
{
	public List<BlockData> blocks;
	public List<PigData> pigs;
};

public class Timer : MonoBehaviour {

	private float _timer;
	private bool _gameOver;
	
	private int _levelCounter;
	
	public float endTime { get; set; }
	public bool enableInput { get; set; }
	
	// Prefabs to Instantiate the objects in the scene
	public GameObject catapult_model;
	public GameObject bird_model;
	public GameObject pig_model;
	public GameObject []block_model;
	
	// Parent objects to organize the scene
	public GameObject catapult_parent;
	public GameObject blocks_parent;
	public GameObject pigs_parent;
	public GameObject birds_parent;
	
	private GameObject _slingshot;
	private GameObject _mainBird;
	
	public List<Level> levels;
	private List<GameData> _gameData;
	
	private Vector3 _initialMainBirdPos;
	
	public ArrayList sceneObjects;
	
	void Start ()
	{		
		_levelCounter = 0;
		
		sceneObjects = new ArrayList();
		_gameData = new List<GameData>();
			
		BuildLevelStaticElements(levels[_levelCounter]);
		BuildLevel(levels[_levelCounter]);
		
		_levelCounter++;
	}
	
	void CollectGameData()
	{
		List<BlockData> blocksInScene = new List<BlockData>();
		List<PigData> pigsInScene = new List<PigData>();
		
        foreach (Transform child in blocks_parent.transform)
		{
			if(child.GetComponent<Block>())
			{	
				Block b = child.GetComponent<Block>();
						
				BlockData blockData = new BlockData();
				blockData.collisionAmount = b.collisionAmount;
				blockData.rotation = b.transform.rotation.z;
				blockData.averageVelocity = b.velocity / b.experimentsAmount;
			
				blocksInScene.Add(blockData);
			}
		}
		
        foreach (Transform child in pigs_parent.transform)
		{
			Block b = child.GetComponent<Block>();
			
			PigData pigData = new PigData();
			pigData.collisionAmount = b.collisionAmount;
			pigData.averageVelocity = b.velocity / b.experimentsAmount;
			
			pigsInScene.Add(pigData);
		}
						
		GameData gd = new GameData();
						
		gd.blocks = blocksInScene;
		gd.pigs = pigsInScene;
	
		_gameData.Add(gd);
	}
	
	void TryKillingPigs()
	{	
		Vector2 slingShotScreenPos = Camera.main.WorldToScreenPoint(_slingshot.transform.position);
			
		//Create rect with catapult				
		Rect slingShotRect = new Rect(	slingShotScreenPos.x - 100, slingShotScreenPos.y + 30, 60, 200);

        foreach (Transform child in pigs_parent.transform)
		{			
			Vector2 pigPos = Camera.main.WorldToScreenPoint(child.gameObject.transform.position);	
			_mainBird.GetComponent<Bird>().TryKillPig(slingShotRect, pigPos);
		}
	}
	
	void SetMainBird(GameObject bird)
	{
		bird.GetComponent<Bird>().isMainBird(true);
		
		ABCamera abCamera = Camera.main.GetComponent<ABCamera>();
		if(abCamera)
		{
			abCamera.target = bird.GetComponent<Bird>();
		}
		
		_mainBird = bird;
		_mainBird.transform.position = _initialMainBirdPos;
		
		_mainBird.GetComponent<Animator>().SetBool("hurt", false);
	}
	
	void ManageBirds()
	{	
		if(_mainBird == null)
		{			
	        foreach (Transform child in birds_parent.transform)
			{
				SetMainBird(child.gameObject);
				break;
			}
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		
		if(_gameOver) return;
		
		ManageBirds();
			
		if(endTime > 0)
		{
			_timer += Time.deltaTime;
			if(_timer >= endTime)
			{	
				CollectGameData();
				
				//TryKillingPigs();
				
				CleanCurrentLevel();
				
				if(_levelCounter < levels.Count)
					BuildLevel(levels[_levelCounter]);
				
				_levelCounter++;
				
				_timer = 0.0f;
			}
		
			if(_levelCounter == levels.Count + 1)
			{		
				GameOver();		
				_gameOver = true;
			}
		}
	}
	
	void GameOver()
	{
		// Save game gata in a xml
		WriteGameData();
		//WriteBlockSizeData();
		Application.Quit();	
	}
	
	void WriteBlockSizeData()
	{
		using (XmlWriter writer = XmlWriter.Create(Application.dataPath + "/block_size_data.xml"))
		{
			writer.WriteStartDocument();
		    
			writer.WriteStartElement("BlockSizeData");
			writer.WriteStartElement("Pig");
				
			Sprite pigSprite = bird_model.GetComponent<SpriteRenderer>().sprite;
			
			int pigN = 0;
			int width = (int)pigSprite.rect.width;
			int height = (int)pigSprite.rect.height;
		
			writer.WriteAttributeString("n", pigN.ToString());
			writer.WriteAttributeString("width", width.ToString());
			writer.WriteAttributeString("height", height.ToString());
			
			writer.WriteEndElement();
			
			int blockIndex = 0;
			
	        foreach (GameObject gameObject in block_model)
			{				
				writer.WriteStartElement("Block");
				
				SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
				if(renderer)
				{
					Sprite blockSprite = renderer.sprite;
							
					width = (int)blockSprite.rect.width;
					height = (int)blockSprite.rect.height;
			
					writer.WriteAttributeString("n", blockIndex.ToString());
					writer.WriteAttributeString("width", width.ToString());
					writer.WriteAttributeString("height", height.ToString());
			
					writer.WriteEndElement();
					blockIndex++;
				}
			}
						
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}
	}
	
	void WriteGameData()
	{		
		using (XmlWriter writer = XmlWriter.Create(Application.dataPath + "/game_data.xml"))
		{
			writer.WriteStartDocument();
		    writer.WriteStartElement("Game");
			
		    foreach (GameData data in _gameData)
		    {
				writer.WriteStartElement("GameData");
				
				writer.WriteStartElement("Blocks");
				
				foreach(BlockData b in data.blocks)
				{
					writer.WriteStartElement("Block");
					writer.WriteAttributeString("averageVelocity", b.averageVelocity.ToString());
					writer.WriteAttributeString("collisions", b.collisionAmount.ToString());
					writer.WriteAttributeString("rotation", b.rotation.ToString());
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				
				writer.WriteStartElement("Pigs");

				foreach(PigData p in data.pigs)
				{					
					writer.WriteStartElement("Pig");
					writer.WriteAttributeString("averageVelocity", p.averageVelocity.ToString());
					writer.WriteAttributeString("collisions", p.collisionAmount.ToString());

					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.WriteEndElement();
		    }
			
		    writer.WriteEndElement();
		    writer.WriteEndDocument();
		}
	}

	void BuildLevelStaticElements(Level level)
	{
		// Placing the catapult
		Vector3 catapultPos = Camera.main.ScreenToWorldPoint(new Vector3(level.catapult.x, level.catapult.y, 10));		
		_slingshot = (GameObject) Instantiate(catapult_model, catapultPos, Quaternion.identity);
		
		if(catapult_parent)
		{
			_slingshot.transform.parent = catapult_parent.transform;
		}
				
		// Placing the birds
       	foreach(LevelData bird in level.birds)
       	{	
			Quaternion defaultRotation = bird_model.transform.rotation;
			
			// Object attributes from file
			Vector3 birdPos = Camera.main.ScreenToWorldPoint(new Vector3(bird.x, bird.y, 10));
			Quaternion birdRot = defaultRotation * Quaternion.Euler(0, 0, bird.rotation);
						
			GameObject clone = (GameObject)Instantiate(bird_model, birdPos, birdRot);
			
			if(birds_parent)
			{
				clone.transform.parent = birds_parent.transform;
			}
			
			// Start the bird enabled or not
			clone.SetActive(bird.enable);
						
			Bird birdClone = clone.GetComponent<Bird>();
			
			if(bird.isMainBird)
			{
				_initialMainBirdPos = birdPos;
				SetMainBird(clone);
			}
			else
			{
				birdClone.isMainBird(false);
			}
			
			// Ignore collisions between birds
			int birdsLayer = LayerMask.NameToLayer("Birds");
			
	        foreach (Transform child in birds_parent.transform)
			{
				Physics2D.IgnoreLayerCollision(birdsLayer, birdsLayer, true);
			}			
		}
	}
	
	void BuildLevel(Level level)
	{
		// Simulation data
		endTime = level.endTime;
		enableInput = level.enableInput;
		
		Time.timeScale = level.timeScale;
		
		// Placing the blocks
       	foreach(LevelData block in level.blocks)
       	{
			Quaternion defaultRotation = block_model[block.n].transform.rotation;
			
			// Block attributes from file
			Vector3 blockPos = Camera.main.ScreenToWorldPoint(new Vector3(block.x, block.y, 10));
			Quaternion blockRot = defaultRotation * Quaternion.Euler(0, 0, block.rotation);
						
			GameObject clone = (GameObject) Instantiate(block_model[block.n], blockPos, blockRot);
			
			if(blocks_parent)
			{
				clone.transform.parent = blocks_parent.transform;
			}
			
			// Start the bird enabled or not
			clone.SetActive(block.enable);
			
			sceneObjects.Add(clone);
		}
		
		// Placing the pigs
       	foreach(LevelData pig in level.pigs)
       	{
			Quaternion defaultRotation = pig_model.transform.rotation;			

			// Pigs attributes from file
			Vector3 pigPos = Camera.main.ScreenToWorldPoint(new Vector3(pig.x, pig.y, 10));
			Quaternion pigRot = defaultRotation * Quaternion.Euler(0, 0, pig.rotation);

			GameObject clone = (GameObject) Instantiate(pig_model, pigPos, pigRot);
			
			if(pigs_parent)
			{
				clone.transform.parent = pigs_parent.transform;
			}
			
			// Start the bird enabled or not
			clone.SetActive(pig.enable);
			
			sceneObjects.Add(clone);
		}
    }
	
	void CleanCurrentLevel()
	{
       	foreach(GameObject clone in sceneObjects)
		{
			Destroy(clone);
		}
	}
}
