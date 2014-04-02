using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public struct LevelData
{
	public int n;
	public float x;
	public float y;
	public float rotation;
	public bool isMainBird;
	public bool enable;
}

public struct Level
{
	public int endTime;
	public int timeScale;
	public bool enableInput;
	
	public LevelData catapult;
	public List <LevelData> birds;
	public List <LevelData> pigs;
	public List <LevelData> blocks;
}

public class LevelParser : MonoBehaviour {
		
	public Timer gameTimer;

	// Use this for initialization
    void Awake()
    {
		ParseFile("/Levels/level1.xml");
	}
	
	void ParseFile(string filename)
	{		
        //Load xml file
		FileInfo theSourceFile = null;
		TextReader reader = null;

		// Read from plain text file if it exists
		theSourceFile = new FileInfo (Application.dataPath + filename);
		if (theSourceFile != null && theSourceFile.Exists)
		{
			reader = theSourceFile.OpenText();  // returns StreamReader
		}
		
		if ( reader == null )
		{
		   Debug.Log("Level file not found or not readable.");
		   return;
		}
		
		gameTimer.levels = new List<Level>();
		
		string text = reader.ReadToEnd();
		
        //TextAsset textXML = (TextAsset)Resources.Load(filename, typeof(TextAsset));
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(text);

		// Parsing Simulation Data
        XmlNode root = xml.FirstChild;
		
	    foreach (XmlNode child in root.ChildNodes)
	    {		
			Level level = new Level();
			
			level.endTime = int.Parse(child.FirstChild.Attributes.GetNamedItem("time").Value);
			level.timeScale = int.Parse(child.FirstChild.Attributes.GetNamedItem("scale").Value);
			level.enableInput = bool.Parse(child.FirstChild.Attributes.GetNamedItem("enableInput").Value);
		
	        // Parsing Catapul Data
			XmlNode catapultData = child.FirstChild.NextSibling;
		
			level.catapult.x = float.Parse(catapultData.Attributes.GetNamedItem("x").Value);
			level.catapult.y = float.Parse(catapultData.Attributes.GetNamedItem("y").Value);
		
			// Parsing birds Data
			XmlNode birdsData = catapultData.NextSibling;
		
			level.birds = new List<LevelData>();
		
	        foreach(XmlNode node in birdsData)
	        {
				LevelData bird = new LevelData();
			
				bird.n = int.Parse(node.Attributes.GetNamedItem("n").Value);
				bird.x = float.Parse(node.Attributes.GetNamedItem("x").Value);
				bird.y = float.Parse(node.Attributes.GetNamedItem("y").Value);
				bird.rotation = float.Parse(node.Attributes.GetNamedItem("rotation").Value);
				bird.enable = bool.Parse(node.Attributes.GetNamedItem("enable").Value);
				bird.isMainBird = bool.Parse(node.Attributes.GetNamedItem("isMainBird").Value);
			
				level.birds.Add(bird);
	        }
		
			// Parsing pigs Data
			XmlNode pigsData = birdsData.NextSibling;
			
			level.pigs = new List<LevelData>();
		
	       	foreach(XmlNode node in pigsData)
	       	{
				LevelData pig = new LevelData();
			
				pig.n = int.Parse(node.Attributes.GetNamedItem("n").Value);
				pig.x = float.Parse(node.Attributes.GetNamedItem("x").Value);
				pig.y = float.Parse(node.Attributes.GetNamedItem("y").Value);
				pig.rotation = float.Parse(node.Attributes.GetNamedItem("rotation").Value);
				pig.enable = bool.Parse(node.Attributes.GetNamedItem("enable").Value);
						
				level.pigs.Add(pig);
	        }
		
			// Parsing blocks Data
			XmlNode blocksData = pigsData.NextSibling;
			
			level.blocks = new List<LevelData>();
		
	       	foreach(XmlNode node in blocksData)
	       	{
				LevelData block = new LevelData();
			
				block.n = int.Parse(node.Attributes.GetNamedItem("n").Value);
				block.x = float.Parse(node.Attributes.GetNamedItem("x").Value);
				block.y = float.Parse(node.Attributes.GetNamedItem("y").Value);
				block.rotation = float.Parse(node.Attributes.GetNamedItem("rotation").Value);
				block.enable = bool.Parse(node.Attributes.GetNamedItem("enable").Value);
						
				level.blocks.Add(block);
	        }
			
			gameTimer.levels.Add(level);
		}
	}
}
