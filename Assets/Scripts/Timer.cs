using UnityEngine;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public int killedPigs { get; set; }
    public int birdContactsAgainsBlocks { get; set; }
	public int movedObjects { get; set; }
	public float averageYVelociy { get; set; }
	
}

public class Timer : MonoBehaviour {

	private float _timer;
	
	private int _timeCounter, _levelCounter;
	public int endTime = 10;

	public Transform _blocks;
	
	public GameObject catapult_model;
	public GameObject bird_model;
	public GameObject pig_model;
	public GameObject []block_model;
	
	public GameObject catapult_parent;
	public GameObject blocks_parent;
	public GameObject characters_parent;
	
	public List<Level> levels;
	
	private List<GameData> _gameData;
	
	public ArrayList sceneObjects;
	
	void Start ()
	{
		sceneObjects = new ArrayList();
		_gameData = new List<GameData>();
			
		BuildLevelStaticElements(levels[_levelCounter]);
		BuildLevel(levels[_levelCounter]);
		
		_levelCounter++;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
			
		if(endTime >= 0 && _levelCounter < levels.Count)
		{
			_timer += Time.deltaTime;
		
			if(_timer >= 1.0f)
			{
				_timeCounter++;			
				_timer = 0.0f;
			}
		
			if(_timeCounter >= endTime)
			{				
				_gameData.Add(new GameData { killedPigs = 3, 
					birdContactsAgainsBlocks = 2, 
					movedObjects = 1, 
					averageYVelociy = calcBlocksYVelocityMean()});
				
				CleanCurrentLevel();
				BuildLevel(levels[_levelCounter]);
				
				_levelCounter++;
				
				_timeCounter = 0;
				_timer = 0.0f;
			}
		}
		
		if(_levelCounter >= levels.Count)
		{
			// Save game gata in a xml
			WriteGameData();
			Application.Quit();
		}
	}
	
	void WriteGameData()
	{
		XmlSerializer xmls = new XmlSerializer(typeof(List<GameData>));
										  
		using (var stream = File.OpenWrite(Application.dataPath + "/game_data.xml"))
		{
			xmls.Serialize(stream, _gameData);
			stream.Close();
		}
	}
	
	float calcBlocksYVelocityMean()
	{
		if(_blocks)
		{	
			int blocksAmount = 0;
			float yVelocityMean = 0.0f;
			
	        foreach (Transform child in _blocks)
			{
				Block block = child.GetComponent<Block>();
				
				yVelocityMean += block.yVelocitySum / block.experimentsAmount;
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
			
			if(characters_parent)
			{
				clone.transform.parent = characters_parent.transform;
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
			
			if(characters_parent)
			{
				clone.transform.parent = characters_parent.transform;
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
