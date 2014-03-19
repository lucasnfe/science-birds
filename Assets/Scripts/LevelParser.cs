using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public struct LevelData
{
	public int n;
	public float x;
	public float y;
	public float rotation;
	public bool isMainBird;
}

public class LevelParser : MonoBehaviour {
	
	public GameObject catapult_model;
	public GameObject bird_model;
	public GameObject pig_model;
	public GameObject []block_model;
	
	public GameObject catapult_parent;
	public GameObject blocks_parent;
	public GameObject characters_parent;

	LevelData catapult;
	List <LevelData> birds;
	List <LevelData> pigs;
	List <LevelData> blocks;

	// Use this for initialization
    void Awake()
    {
		ParseFile("level1");
		BuildLevel();
	}
	
	void ParseFile(string filename)
	{
		birds = new List<LevelData>();
		pigs = new List<LevelData>();
		blocks = new List<LevelData>();
		
        //Load xml file
        TextAsset textXML = (TextAsset)Resources.Load(filename, typeof(TextAsset));
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(textXML.text);

        // Parsing Catapul Data
        XmlNode root = xml.FirstChild;
		
		catapult.x = float.Parse(root.FirstChild.Attributes.GetNamedItem("x").Value);
		catapult.y = float.Parse(root.FirstChild.Attributes.GetNamedItem("y").Value);
		
		// Parsing birds Data
		XmlNode birdsData = root.FirstChild.NextSibling;
		
        foreach(XmlNode node in birdsData)
        {
			LevelData bird = new LevelData();
			
			bird.n = int.Parse(node.Attributes.GetNamedItem("n").Value);
			bird.x = float.Parse(node.Attributes.GetNamedItem("x").Value);
			bird.y = float.Parse(node.Attributes.GetNamedItem("y").Value);
			bird.rotation = float.Parse(node.Attributes.GetNamedItem("rotation").Value);
			bird.isMainBird = bool.Parse(node.Attributes.GetNamedItem("isMainBird").Value);
			
			birds.Add(bird);
        }
		
		// Parsing pigs Data
		XmlNode pigsData = birdsData.NextSibling;
		
       	foreach(XmlNode node in pigsData)
       	{
			LevelData pig = new LevelData();
			
			pig.n = int.Parse(node.Attributes.GetNamedItem("n").Value);
			pig.x = float.Parse(node.Attributes.GetNamedItem("x").Value);
			pig.y = float.Parse(node.Attributes.GetNamedItem("y").Value);
			pig.rotation = float.Parse(node.Attributes.GetNamedItem("rotation").Value);
						
			pigs.Add(pig);
        }
		
		// Parsing blocks Data
		XmlNode blocksData = pigsData.NextSibling;
		
       	foreach(XmlNode node in blocksData)
       	{
			LevelData block = new LevelData();
			
			block.n = int.Parse(node.Attributes.GetNamedItem("n").Value);
			block.x = float.Parse(node.Attributes.GetNamedItem("x").Value);
			block.y = float.Parse(node.Attributes.GetNamedItem("y").Value);
			block.rotation = float.Parse(node.Attributes.GetNamedItem("rotation").Value);
						
			blocks.Add(block);
        }
	}
	
	void BuildLevel()
	{
		// Placing the catapult
		GameObject catapultClone = (GameObject) Instantiate(catapult_model, new 
			Vector3(catapult.x, catapult.y, 10), Quaternion.identity);
		
		if(catapult_parent)
		{
			catapultClone.transform.parent = catapult_parent.transform;
		}
		
		// Placing the birds
       	foreach(LevelData bird in birds)
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
		
		// Placing the blocks
       	foreach(LevelData block in blocks)
       	{
			Quaternion defaultRotation = block_model[block.n].transform.rotation;			
			GameObject clone = (GameObject) Instantiate(block_model[block.n], 
				new Vector3(block.x, block.y, 10), defaultRotation * Quaternion.Euler(0, 0, block.rotation));
			
			if(blocks_parent)
			{
				clone.transform.parent = blocks_parent.transform;
			}
		}
		
		// Placing the pigs
       	foreach(LevelData pig in pigs)
       	{
			Quaternion defaultRotation = pig_model.transform.rotation;			
			GameObject clone = (GameObject) Instantiate(pig_model, 
				new Vector3(pig.x, pig.y, 10), defaultRotation * Quaternion.Euler(0, 0, pig.rotation));
			
			if(characters_parent)
			{
				clone.transform.parent = characters_parent.transform;
			}
			
		}
    }

}
