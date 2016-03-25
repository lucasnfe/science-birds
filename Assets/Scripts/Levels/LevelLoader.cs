using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

/** \class LevelLoader
 *  \brief  Contains methods to load and save created levels on xml file.
 *
 *  Can load all levels from the GeneratedLevels folder in xml format, also can save the current level in xml format.
 */
public class LevelLoader : MonoBehaviour {
	
    /**
     *  Loads all the .xml files in the GeneratedLevels folder. Creates and ABLevel array to save each loaded level.
     *  Load each level in the level array and returns the array.
     *  @return ABLevel[]   array list containing all the loaded levels from the .xml files.
     */
	public static ABLevel[] LoadAllLevels() {
	
		TextAsset []levelsXmlData = Resources.LoadAll<TextAsset>("GeneratedLevels/XML");

		ABLevel []levels = new ABLevel[levelsXmlData.Length];

		for(int i = 0; i < levelsXmlData.Length; i++)
			levels[i] = LoadXmlLevel(levelsXmlData[i].text);

		return levels;
	}
	
    /**
     *  Loads the level file with xmlString name, saves it as an ABLevel, and adds all its game objects to the ABLevel object.
     *  First reads the birds amounts and then
     *  Reads the label and (x,y) position of each object and creates the object.
     *  @param[in]  xmlString   name of the file containing the level to be loaded
     *  @return ABLevel an Angry Birds Level object containing all the game objects of the loaded level.
     */
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

    /** 
     *  Saves and ABLevel to an xml file. Writes the label Level, and then the label BirdsAmount together with the amount of 
     *  Birds in level, then writes the label GameOjbects and for each object writes
     *  GameObject, its label, its x position and its y position.
     *  For the name of the file, it is added one to the size of current levels saved on the GeneratedLevels folder.
     *  Then the file is writen.
     *  @param[in]  level   ABLevel to be saved on .xml file.
     */
    public static void SaveXmlLevel(ABLevel level, Boolean useClassifier, Boolean useRegression, Boolean createPrePop)
    {

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

            foreach (ABGameObject abObj in level.gameObjects)
            {
                writer.WriteStartElement("GameObject");
                writer.WriteAttributeString("label", abObj.Label.ToString());
                writer.WriteAttributeString("x", abObj.Position.x.ToString());
                writer.WriteAttributeString("y", abObj.Position.y.ToString());
                writer.WriteEndElement();
            }
        }

        int levelsAmountInResources = LoadAllLevels().Length;
        //Contains data about the method used in the EA
        //Empty if none, C if used Classifier, R if regression, P if pre populatio
        string name = "";
        if (useClassifier)
            name += "C";
        if (useRegression)
            name += "R";
        if (createPrePop)
            name += "P";

        StreamWriter streamWriter = new StreamWriter("Assets/Resources/GeneratedLevels/XML/genetic-level-" + name + "-" +(levelsAmountInResources + 1) + ".xml");
        streamWriter.WriteLine(output.ToString());
        streamWriter.Close();
    }

    public static void SaveXmlLevel(ABLevel level)
    {

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

            foreach (ABGameObject abObj in level.gameObjects)
            {
                writer.WriteStartElement("GameObject");
                writer.WriteAttributeString("label", abObj.Label.ToString());
                writer.WriteAttributeString("x", abObj.Position.x.ToString());
                writer.WriteAttributeString("y", abObj.Position.y.ToString());
                writer.WriteEndElement();
            }
        }

        int levelsAmountInResources = LoadAllLevels().Length;

        StreamWriter streamWriter = new StreamWriter("Assets/Resources/GeneratedLevels/XML/genetic-level-" + (levelsAmountInResources + 1) + ".xml");
        streamWriter.WriteLine(output.ToString());
        streamWriter.Close();
    }

    /**
     *  Gets the blocks in the scene, adds each of them to a list of GameObject objects, creates
     *  an ABLevel based on the game objects list, using the ABLevelGenerator.GameObjectsToABLevel() method
     *  And then saves this new level to and .xml file.
     */
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