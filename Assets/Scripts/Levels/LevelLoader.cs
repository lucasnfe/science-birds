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
	
		TextAsset []levelsXmlData = Resources.LoadAll<TextAsset>("GeneratedLevels/XML-InitPop");

		ABLevel []levels = new ABLevel[levelsXmlData.Length];

		for(int i = 0; i < levelsXmlData.Length; i++)
			levels[i] = LoadXmlLevel(levelsXmlData[i].text);

		return levels;
	}
    
    /** 
     *  Saves and ABLevel to an xml file. Writes the label Level, and then the label BirdsAmount together with the amount of 
     *  Birds in level, then writes the label GameOjbects and for each object writes
     *  GameObject, its label, its x position and its y position.
     *  For the name of the file, it is added one to the size of current levels saved on the GeneratedLevels folder,
     *  Representing what auxiliary method was used, or nothing is added if none was used.
     *  Then the file is writen.
     *  @param[in]  level           ABLevel to be saved on .xml file.
     *  @param[in]  useClassifier   True if classifier was used in the evolution of this population
     *  @param[in]  useRegression   True if regression was used in the evolution of this population
     *  @param[in]  createPrePop    True if classifier was used to create the initial population
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
    /** 
     *  Saves and ABLevel to an xml file. Writes the label Level, and then the label BirdsAmount together with the amount of 
     *  Birds in level, then writes the label GameOjbects and for each object writes
     *  GameObject, its label, its x position and its y position.
     *  For the name of the file, it is added one to the size of current levels saved on the GeneratedLevels folder.
     *  Then the file is writen.
     *  @param[in]  level   ABLevel to be saved on .xml file.
     */
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
     *  Saves and ABLevel to an xml file with the name contained in filename. 
     *  Writes the label Level, and then the label BirdsAmount together with the amount of 
     *  Birds in level, then writes the label GameOjbects and for each object writes
     *  GameObject, its label, its x position and its y position.
     *  @param[in]  level       ABLevel to be saved on .xml file.
     *  @param[in]  filename    
     */
    public static void SaveXmlLevel(ABLevel level, string filename)
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

        //int levelsAmountInResources = LoadAllLevels().Length;

        StreamWriter streamWriter = new StreamWriter("Assets/Resources/GeneratedLevels/XML/"+ filename + ".xml");
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
    /**
     *  Loads all the .xml files in the InitPop folder. Creates a ShiftABLevel array to save each loaded genome.
     *  Load each genome in the level's genome array and returns the array.
     *  @return ShiftABLevel[]   array list containing all the loaded levels' genomes from the .xml files.
     */
    public static ShiftABLevel[] LoadAllGenomes()
    {
        TextAsset[] levelsXmlData = new TextAsset[100];
        for (int i = 0; i < 100; i++)
        {
            levelsXmlData[i] = Resources.Load<TextAsset>("GeneratedLevels/XML-InitPop/InitPop_" + i);
        }
        ShiftABLevel[] levels = new ShiftABLevel[levelsXmlData.Length];

        for (int i = 0; i < levelsXmlData.Length; i++)
            levels[i] = LoadXmlGenome(levelsXmlData[i].text);

        return levels;
    }


    /**
     *  Loads the level file with xmlString name, saves it as an ABLevel, and adds all its game objects to the ABLevel object.
     *  First reads the birds amounts and then
     *  Reads the label and (x,y) position of each object and creates the object.
     *  @param[in]  xmlString   name of the file containing the level to be loaded
     *  @return ABLevel an Angry Birds Level object containing all the game objects of the loaded level.
     */
    public static ABLevel LoadXmlLevel(string xmlString)
    {

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
     *  Loads the level genome file with xmlString name, saves it as an ShiftABLevel, and adds all its 
     *  objects to the corresponding stack.
     *  First reads the birds amounts and then for each stack reads
     *  The label, if is a duplicated object and the sum of the heigths of objects under this object.
     *  @param[in]  xmlString   name of the file containing the level genome to be loaded
     *  @return ShiftABLevel an Angry Birds Level Genome object containing all the objects and stacks of the loaded level genome.
     */
    public static ShiftABLevel LoadXmlGenome(string xmlString)
    {

        ShiftABLevel level = new ShiftABLevel();
        level.gameObjects = new List<ABGameObject>();
        using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
        {
            reader.ReadToFollowing("BirdsAmount");
            level.birdsAmount = Convert.ToInt32(reader.ReadElementContentAsString());

            reader.ReadToFollowing("Stacks");
            LinkedList<ShiftABGameObject> stack = new LinkedList<ShiftABGameObject>();
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    if (reader.Name == "GameObjects")
                    {
                        stack = new LinkedList<ShiftABGameObject>();
                    }
                    // Only detect start elements.

                    if (reader.Name == "GameObject")
                    {
                        ShiftABGameObject abObj = new ShiftABGameObject();
                        reader.MoveToAttribute("label");
                        int label = Convert.ToInt32(reader.Value);
                        if (label != -1)
                        {
                            abObj.Label = Convert.ToInt32(reader.Value);

                            reader.MoveToAttribute("isDouble");
                            abObj.IsDouble = Convert.ToBoolean(reader.Value);

                            reader.MoveToAttribute("underObjectsHeight");
                            abObj.UnderObjectsHeight = (float)Convert.ToDouble(reader.Value);
                            stack.AddLast(abObj);
                        }
                        else
                        {
                            reader.MoveToAttribute("isDouble");
                            reader.MoveToAttribute("underObjectsHeight");
                        }
                        
                    }
                }
                if(reader.NodeType == XmlNodeType.EndElement && reader.Name == "GameObjects")
                {
                    level.AddStack(stack);
                }
            }
        }
        return level;
    }
    /** 
     *  Saves a ShiftABLevel to an xml file, with the name defined in filename. 
     *  Writes the label Level, and then the label BirdsAmount together with the amount of 
     *  Birds in level, then writes the label Stacks to define the beginning of the stacks
     *  and for each stack, writes the label GameObjects, and for each object writes the label GameObject and then,
     *  for each object, writes its label, if it is doubled, and the height of the objects below it
     *  @param[in]  level       ShiftABLevel to be saved on .xml file.
     *  @param[in]  filename    The name of the xml file to be saved.
     */
    public static void SaveXmlGenome(ShiftABLevel level, string filename)
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

            writer.WriteStartElement("Stacks");

            for (int i = 0; i < level.GetStacksAmount(); i++)
            {
                writer.WriteStartElement("GameObjects");
                if (level.GetStack(i).Count > 0)
                {
                    for (LinkedListNode<ShiftABGameObject> obj = level.GetStack(i).First; obj != level.GetStack(i).Last.Next; obj = obj.Next)
                    {
                        writer.WriteStartElement("GameObject");
                        writer.WriteAttributeString("label", obj.Value.Label.ToString());
                        writer.WriteAttributeString("isDouble", obj.Value.IsDouble.ToString());
                        writer.WriteAttributeString("underObjectsHeight", obj.Value.UnderObjectsHeight.ToString());
                        writer.WriteEndElement();
                    }
                }
                else
                {
                    writer.WriteStartElement("GameObject");
                    writer.WriteAttributeString("label", "-1");
                    writer.WriteAttributeString("isDouble", "-1");
                    writer.WriteAttributeString("underObjectsHeight", "-1");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        StreamWriter streamWriter = new StreamWriter("Assets/Resources/GeneratedLevels/XML-InitPop/" + filename + ".xml");
        streamWriter.WriteLine(output.ToString());
        streamWriter.Close();
    }
}