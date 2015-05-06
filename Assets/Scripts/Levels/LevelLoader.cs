using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour {
	
	public static ABLevel[] LoadAllLevels() {
	
		TextAsset []levelsXmlData = Resources.LoadAll<TextAsset>("GeneratedLevels/");

		ABLevel []levels = new ABLevel[levelsXmlData.Length];

		for(int i = 0; i < levelsXmlData.Length; i++)
			levels[i] = LoadXmlLevel(levelsXmlData[i].text);

		return levels;
	}
	
	public static ABLevel LoadXmlLevel(string xmlString) {

		ABLevel level = new ABLevel();
		level.gameObjects = new List<ABGameObject>();

		using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
		{
			reader.ReadToFollowing("BirdsAmount");
			level.birdsAmount = Convert.ToInt32(reader.ReadElementContentAsString());

			reader.ReadToFollowing("GameObjects");

			while (reader.ReadToFollowing("GameObject"))
			{
				ABGameObject abObj = new ABGameObject();

				reader.MoveToAttribute("label");
				abObj.Label = Convert.ToInt32(reader.Value);

				Vector2 position = new Vector2();

				reader.MoveToAttribute("x");
				position.x = (float)Convert.ToDouble(reader.Value);

				reader.MoveToAttribute("y");
				position.y = (float)Convert.ToDouble(reader.Value);

				abObj.Position = position;

				level.gameObjects.Add(abObj);
			}
		}

		return level;
	}

	public static void SaveXmlLevel(ABLevel level) {

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

			foreach(ABGameObject abObj in level.gameObjects)
			{
				writer.WriteStartElement("GameObject");
				writer.WriteAttributeString("label", abObj.Label.ToString());
				writer.WriteAttributeString("x", abObj.Position.x.ToString());
				writer.WriteAttributeString("y", abObj.Position.y.ToString());
				writer.WriteEndElement();
			}
		}

		int levelsAmountInResources = LoadAllLevels().Length;

		StreamWriter streamWriter = new StreamWriter("Assets/Resources/GeneratedLevels/genetic-level-" + (levelsAmountInResources + 1) + ".xml");
		streamWriter.WriteLine(output.ToString());
		streamWriter.Close();
	}

	public void SaveLevelOnScene() {

		Transform blocksInScene = GameWorld.Instance._blocksTransform;

		List<GameObject> objsInScene = new List<GameObject>();

		foreach(Transform b in blocksInScene)
		{
			objsInScene.Add(b.gameObject);
		}

		ABLevel level = ABLevelGenerator.GameObjectsToABLevel(objsInScene.ToArray());
		SaveXmlLevel(level);
	}
}