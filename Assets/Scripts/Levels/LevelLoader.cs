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
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

public class LevelLoader {
	
	public static string ReadXmlLevel(string path) {
	
		string xmlText = "";

		if (path.Contains ("StreamingAssets")) {

			xmlText = File.ReadAllText (path);
		} 
		else {

			string[] stringSeparators = new string[] {"Levels/"};
			string[] arrayPath = path.Split (stringSeparators, StringSplitOptions.None);
			string finalPath = arrayPath [1].Split ('.') [0];

			TextAsset levelData = Resources.Load<TextAsset>("Levels/" + finalPath);
			xmlText = levelData.text;
		}

		return xmlText;
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

			reader.ReadToFollowing("Slingshot");

			reader.MoveToAttribute("x");
			level.slingshot.x = (float)Convert.ToDouble (reader.Value);

			reader.MoveToAttribute("y");
			level.slingshot.y = (float)Convert.ToDouble (reader.Value);

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

				abObj.material = "";
				if (reader.GetAttribute ("material") != null) {

					reader.MoveToAttribute("material");
					abObj.material = reader.Value;
				}
					
				reader.MoveToAttribute("x");
				abObj.x = (float)Convert.ToDouble(reader.Value);

				reader.MoveToAttribute("y");
				abObj.y = (float)Convert.ToDouble(reader.Value);

				abObj.rotation = 0f;
				if (reader.GetAttribute ("rotation") != null) {
				
					reader.MoveToAttribute ("rotation");
					abObj.rotation = (float)Convert.ToDouble (reader.Value);
				}

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

			writer.WriteStartElement("Slingshot");
			writer.WriteAttributeString("x", level.slingshot.x.ToString());
			writer.WriteAttributeString("y", level.slingshot.y.ToString());
			writer.WriteEndElement();

			writer.WriteStartElement("GameObjects");

			foreach(OBjData abObj in level.blocks)
			{
				writer.WriteStartElement("Block");
				writer.WriteAttributeString("type", abObj.type.ToString());
				writer.WriteAttributeString("material", abObj.material.ToString());
				writer.WriteAttributeString("x", abObj.x.ToString());
				writer.WriteAttributeString("y", abObj.y.ToString());
				writer.WriteAttributeString("rotation", abObj.rotation.ToString());
				writer.WriteEndElement();
			}

			foreach(OBjData abObj in level.pigs)
			{
				writer.WriteStartElement("Pig");
				writer.WriteAttributeString("type", abObj.type.ToString());
				writer.WriteAttributeString("material", abObj.material.ToString());
				writer.WriteAttributeString("x", abObj.x.ToString());
				writer.WriteAttributeString("y", abObj.y.ToString());
				writer.WriteAttributeString("rotation", abObj.rotation.ToString());
				writer.WriteEndElement();
			}

			foreach(OBjData abObj in level.platforms)
			{
				writer.WriteStartElement("Platform");
				writer.WriteAttributeString("type", abObj.type.ToString());
				writer.WriteAttributeString("material", abObj.material.ToString());
				writer.WriteAttributeString("x", abObj.x.ToString());
				writer.WriteAttributeString("y", abObj.y.ToString());
				writer.WriteAttributeString("rotation", abObj.rotation.ToString());
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