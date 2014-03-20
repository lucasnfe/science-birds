using UnityEngine;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

public class GameData
{
    public int killedPigs { get; set; }
    public int birdContactsAgainsBlocks { get; set; }
	public int movedObjects { get; set; }
	public float averageYVelociy { get; set; }
	
}

public class Timer : MonoBehaviour {

	private float _timer;
	private int _timeCounter;
	
	public int endTime = 10;
	public Transform _blocks;
	
	// Update is called once per frame
	void FixedUpdate () {
	
		if(endTime >= 0)
		{
			_timer += Time.deltaTime;
		
			if(_timer >= 1.0f)
			{
				_timeCounter++;			
				_timer = 0.0f;
			}
		
			if(_timeCounter >= endTime)
			{
				calcBlocksYVelocityMean();
				WriteGameData(new GameData { 
					killedPigs = 3, 
					birdContactsAgainsBlocks = 2, 
					movedObjects = 1, 
					averageYVelociy = calcBlocksYVelocityMean()});
					
				Application.Quit();
			}
		}
	}
	
	void WriteGameData(GameData gameData)
	{
		XmlSerializer xmls = new XmlSerializer(typeof(GameData));
										  
		using (var stream = File.OpenWrite(Application.dataPath + "/game_data.xml"))
		{
			xmls.Serialize(stream, new GameData { killedPigs = gameData.killedPigs, 
										  birdContactsAgainsBlocks = gameData.birdContactsAgainsBlocks,
										  movedObjects = gameData.movedObjects,
										  averageYVelociy = gameData.averageYVelociy});
										  
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
}
