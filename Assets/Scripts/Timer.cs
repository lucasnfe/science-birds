using UnityEngine;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;

public struct PigData
{
	public int collisionAmount;
};

public struct BlockData
{
	public int collisionAmount;
	public float rotation;
};

public struct GameData
{
	public float averageYVelocity;

	public List<BlockData> blocks;
	public List<PigData> pigs;
};

public class Timer : MonoBehaviour {

	private float _timer;
	private bool _gameOver;
	
	private int _timeCounter, _levelCounter;
	public int endTime = 10;
	
	public GameObject catapult_model;
	public GameObject bird_model;
	public GameObject pig_model;
	public GameObject []block_model;
	
	public GameObject catapult_parent;
	public GameObject blocks_parent;
	public GameObject pigs_parent;
	public GameObject birds_parent;
	
	public List<Level> levels;
	
	private List<GameData> _gameData;
	
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
	
	// Update is called once per frame
	void FixedUpdate () {
			
		if(endTime > 0 && !_gameOver)
		{
			_timer += Time.deltaTime;
		
			if(_timer >= 1.0f)
			{
				_timeCounter++;			
				_timer = 0.0f;
			}
		
			if(_timeCounter == endTime)
			{	
				List<BlockData> blocksInScene = new List<BlockData>();
				List<PigData> pigsInScene = new List<PigData>();
				
		        foreach (Transform child in blocks_parent.transform)
				{
					Block b = child.GetComponent<Block>();
					
					BlockData blockData = new BlockData();
					blockData.collisionAmount = b.GetCollisionAmount();
					blockData.rotation = b.transform.rotation.z;
					
					blocksInScene.Add(blockData);
				}
				
		        foreach (Transform child in pigs_parent.transform)
				{
					Block b = child.GetComponent<Block>();
					
					PigData pigData = new PigData();
					pigData.collisionAmount = b.GetCollisionAmount();
					
					pigsInScene.Add(pigData);
				}
								
				GameData gd = new GameData();
				
				gd.averageYVelocity = calcBlocksYVelocityMean();
				gd.blocks = blocksInScene;
				gd.pigs = pigsInScene;
			
				_gameData.Add(gd);
				
				CleanCurrentLevel();
				
				if(_levelCounter < levels.Count)
					BuildLevel(levels[_levelCounter]);
				
				_levelCounter++;
				
				_timeCounter = 0;
				_timer = 0.0f;
			}
			
			
			if(_levelCounter == levels.Count + 1)
			{				
				// Save game gata in a xml
				WriteGameData();
				Application.Quit();
				
				_gameOver = true;
			}
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
				writer.WriteElementString("averageYVelocity", data.averageYVelocity.ToString());
				
				writer.WriteStartElement("Blocks");
				
				foreach(BlockData b in data.blocks)
				{
					writer.WriteStartElement("Block");
					writer.WriteAttributeString("collisions", b.collisionAmount.ToString());
					writer.WriteAttributeString("rotation", b.rotation.ToString());
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				
				writer.WriteStartElement("Pigs");

				foreach(PigData p in data.pigs)
				{					
					writer.WriteStartElement("Pig");
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
	
	float calcBlocksYVelocityMean()
	{
		if(blocks_parent)
		{	
			int blocksAmount = 0;
			float yVelocityMean = 0.0f;
			
	        foreach (Transform child in blocks_parent.transform)
			{
				Block b = child.GetComponent<Block>();
				yVelocityMean += b.yVelocitySum / b.experimentsAmount;
				blocksAmount++;
			}
			
	        foreach (Transform child in pigs_parent.transform)
			{
				Block b = child.GetComponent<Block>();
				yVelocityMean += b.yVelocitySum / b.experimentsAmount;
				blocksAmount++;
			}
			
			return yVelocityMean/blocksAmount;
		}
		
		return -1;
	}
	
	void BuildLevelStaticElements(Level level)
	{
		// Placing the catapult
		GameObject catapultClone = (GameObject) Instantiate(catapult_model, new 
			Vector3(level.catapult.x, level.catapult.y, 10), Quaternion.identity);
		
		if(catapult_parent)
		{
			catapultClone.transform.parent = catapult_parent.transform;
		}
				
		// Placing the birds
       	foreach(LevelData bird in level.birds)
       	{	
			Quaternion defaultRotation = bird_model.transform.rotation;			
			GameObject clone = (GameObject)Instantiate(bird_model, 
				new Vector3(bird.x, bird.y, 10), defaultRotation * Quaternion.Euler(0, 0, bird.rotation));
			
			if(birds_parent)
			{
				clone.transform.parent = birds_parent.transform;
			}
						
			Bird birdClone = clone.GetComponent<Bird>();
			birdClone.setMainBird(bird.isMainBird);
			
			if(bird.isMainBird)
			{
				ABCamera abCamera = Camera.main.GetComponent<ABCamera>();
				if(abCamera)
				{
					abCamera.target = birdClone;
				}
			}
		}
	}
	
	void BuildLevel(Level level)
	{
		// Simulation data
		endTime = level.endTime;
		Time.timeScale = level.timeScale;
		
		// Placing the blocks
       	foreach(LevelData block in level.blocks)
       	{
			Quaternion defaultRotation = block_model[block.n].transform.rotation;			
			GameObject clone = (GameObject) Instantiate(block_model[block.n], 
				new Vector3(block.x, block.y, 10), defaultRotation * Quaternion.Euler(0, 0, block.rotation));
			
			if(blocks_parent)
			{
				clone.transform.parent = blocks_parent.transform;
			}
			
			sceneObjects.Add(clone);
		}
		
		// Placing the pigs
       	foreach(LevelData pig in level.pigs)
       	{
			Quaternion defaultRotation = pig_model.transform.rotation;			
			GameObject clone = (GameObject) Instantiate(pig_model, 
				new Vector3(pig.x, pig.y, 10), defaultRotation * Quaternion.Euler(0, 0, pig.rotation));
			
			if(pigs_parent)
			{
				clone.transform.parent = pigs_parent.transform;
			}
			
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
