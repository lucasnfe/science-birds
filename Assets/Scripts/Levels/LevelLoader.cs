using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

public class LevelLoader {
	
	public static ABLevel[] LoadAllLevels() {
	
		TextAsset []levelsXmlData = (TextAsset [])Resources.LoadAll("Levels");

		ABLevel []levels = new ABLevel[levelsXmlData.Length];

		for(int i = 0; i < levelsXmlData.Length; i++)
			levels[i] = LoadXmlLevel(levelsXmlData[i].text);

		return levels;
	}
	
	public static ABLevel LoadXmlLevel(string xmlString) {

		ABLevel level = new ABLevel();

		level.pigs = new List<OBjData>();
		level.blocks = new List<OBjData>();
		level.platforms = new List<OBjData>();

		using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
		{
			reader.ReadToFollowing("BirdsAmount");
			level.birdsAmount = Convert.ToInt32(reader.ReadElementContentAsString());

			reader.ReadToFollowing("GameObjects");
			reader.Read ();

			while (reader.Read())
			{
				OBjData abObj = new OBjData();
				string nodeName = reader.LocalName;

				if (nodeName == "GameObjects")
					break;

				reader.MoveToAttribute("type");
				abObj.type = reader.Value;

				reader.MoveToAttribute("material");
				abObj.material = reader.Value;

				reader.MoveToAttribute("x");
				abObj.x = (float)Convert.ToDouble(reader.Value);

				reader.MoveToAttribute("y");
				abObj.y = (float)Convert.ToDouble(reader.Value);

				if (nodeName == "Block") {

					level.blocks.Add (abObj);
					reader.Read ();
				} 
				else if (nodeName == "Pig") {

					level.pigs.Add (abObj);
					reader.Read ();
				}
				else if (nodeName == "Platform") {

					level.platforms.Add (abObj);
					reader.Read ();
				}
			}
		}

		return level;
	}

	public static void SaveXmlLevel(ABLevel level, string path) {

		StringBuilder output = new StringBuilder();
		XmlWriterSettings ws = new XmlWriterSettings();
		ws.Indent = true;

		using (XmlWriter writer = XmlWriter.Create(output, ws))
		{
			writer.WriteStartElement("Level");

			writer.WriteStartElement("BirdsAmount");
			writer.WriteValue(level.birdsAmount);
			writer.WriteEndElement();

			writer.WriteStartElement("GameObjects");

			foreach(OBjData abObj in level.blocks)
			{
				writer.WriteStartElement("Block");
				writer.WriteAttributeString("type", abObj.type.ToString());
				writer.WriteAttributeString("material", abObj.material.ToString());
				writer.WriteAttributeString("x", abObj.x.ToString());
				writer.WriteAttributeString("y", abObj.y.ToString());
				writer.WriteEndElement();
			}

			foreach(OBjData abObj in level.pigs)
			{
				writer.WriteStartElement("Pig");
				writer.WriteAttributeString("type", abObj.type.ToString());
				writer.WriteAttributeString("material", abObj.material.ToString());
				writer.WriteAttributeString("x", abObj.x.ToString());
				writer.WriteAttributeString("y", abObj.y.ToString());
				writer.WriteEndElement();
			}

			foreach(OBjData abObj in level.platforms)
			{
				writer.WriteStartElement("Platform");
				writer.WriteAttributeString("type", abObj.type.ToString());
				writer.WriteAttributeString("material", abObj.material.ToString());
				writer.WriteAttributeString("x", abObj.x.ToString());
				writer.WriteAttributeString("y", abObj.y.ToString());
				writer.WriteEndElement();
			}
		}
			
		StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.WriteLine(output.ToString());
		streamWriter.Close();
	}
		
	public static Dictionary<string, GameObject> LoadABResource(string path) {

		// Load block templates and cast them to game objects
		UnityEngine.Object[] objs = Resources.LoadAll(path);

		Dictionary<string, GameObject> resources = new Dictionary<string, GameObject>();

		for (int i = 0; i < objs.Length; i++) {

			GameObject abGameObject = (GameObject)objs[i];
			resources [abGameObject.name] = abGameObject;
		}

		return resources;
	}

	public void SaveLevelOnScene() {

//		Transform blocksInScene = GameWorld.Instance.BlocksInScene();
//
//		List<GameObject> objsInScene = GameWorld.Instance.BlocksInScene();

//		ABLevel level = ABLevelGenerator.GameObjectsToABLevel(objsInScene.ToArray());
//		SaveXmlLevel(level);
	}
}