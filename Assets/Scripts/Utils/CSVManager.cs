using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/** \class  CSVManager
*  \brief  manages the CSV files
*
*   Contains the creation, saving and loading functionallity for the CSV files that represents the level as an input for the classification and regression models
*/
public class CSVManager
{
    /// <summary>
    /// Class to store one CSV row
    /// </summary>
    public class CsvRow : List<string>
    {
        public string LineText { get; set; }
    }

    /// <summary>
    /// Class to write data to a CSV file
    /// </summary>
    public class CsvFileWriter : StreamWriter
    {
        public CsvFileWriter(Stream stream)
            : base(stream)
        {
        }

        public CsvFileWriter(string filename, bool append)
            : base(filename, append)
        {
        }

        /// <summary>
        /// Writes a single row to a CSV file.
        /// </summary>
        /// <param name="row">The row to be written</param>
        public void WriteRow(CsvRow row)
        {
            StringBuilder builder = new StringBuilder();
            bool firstColumn = true;
            foreach (string value in row)
            {
                // Add separator if this isn't the first value
                if (!firstColumn)
                    builder.Append(',');
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                firstColumn = false;
            }
            row.LineText = builder.ToString();
            WriteLine(row.LineText);
        }
    }

    /// <summary>
    /// Class to read data from a CSV file
    /// </summary>
    public class CsvFileReader : StreamReader
    {
        public CsvFileReader(Stream stream)
            : base(stream)
        {
        }

        public CsvFileReader(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Reads a row of data from a CSV file
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool ReadRow(CsvRow row)
        {
            row.LineText = ReadLine();
            if (String.IsNullOrEmpty(row.LineText))
                return false;

            int pos = 0;
            int rows = 0;

            while (pos < row.LineText.Length)
            {
                string value;

                // Special handling for quoted field
                if (row.LineText[pos] == '"')
                {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    int start = pos;
                    while (pos < row.LineText.Length)
                    {
                        // Test for quote character
                        if (row.LineText[pos] == '"')
                        {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= row.LineText.Length || row.LineText[pos] != '"')
                            {
                                pos--;
                                break;
                            }
                        }
                        pos++;
                    }
                    value = row.LineText.Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int start = pos;
                    while (pos < row.LineText.Length && row.LineText[pos] != ',')
                        pos++;
                    value = row.LineText.Substring(start, pos - start);
                }

                // Add field to list
                if (rows < row.Count)
                    row[rows] = value;
                else
                    row.Add(value);
                rows++;

                // Eat up to and including next comma
                while (pos < row.LineText.Length && row.LineText[pos] != ',')
                    pos++;
                if (pos < row.LineText.Length)
                    pos++;
            }
            // Delete any unused items
            while (row.Count > rows)
                row.RemoveAt(rows);

            // Return true if any columns read
            return (row.Count > 0);
        }
    }
    /**
    *   Load all the levels contained in the file "filename"
    *   @param[in]  filename    name of the CSV file
    *   @return     count       number of levels loaded
    */
    public static int LoadAllLevelsCSV(String filename)
    {
        //Load eveything in the file
        TextAsset[] levelsCSVData = Resources.LoadAll<TextAsset>(filename);
        
        int count = 0;
        //Load each line into a level
        for (int i = 0; i < levelsCSVData.Length; i++)
        {
            LoadCSVLevel(levelsCSVData[i].text);
            count++;
        }
        return count;
    }
    /**
    *   Creates the CSV structure in which to save the levels
    *   @param[in]  filename    name of the CSV file
    */
    static void createCSV(String filename)
    {
        using (CsvFileWriter writer = new CsvFileWriter(filename, false))
        {
            CsvRow row = new CsvRow();
            //Creates each line and column with the label being its coordinates
            for (int i = 0; i < 22; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    row.Add(String.Format("(X, Y){0}, {1}", i, j));
                }
            }
            //Create the label for birds, if level is playable and fitness
            row.Add(String.Format("Birds"));
            row.Add(String.Format("IsPlayable"));
            row.Add(String.Format("Fitness"));
            writer.WriteRow(row);
        }
    }
    /**
    *   Creates the CSV structure in which to save the levels, without the fitness value
    *   @param[in]  filename    name of the CSV file
    */
    static void createCSVWithoutFitness(String filename)
    {
        using (CsvFileWriter writer = new CsvFileWriter(filename, false))
        {
            CsvRow row = new CsvRow();
            //Creates each line and column with the label being its coordinates
            for (int i = 0; i < 22; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    row.Add(String.Format("(X, Y){0}, {1}", i, j));
                }
            }
            //Create the label for birds and if level is playable
            row.Add(String.Format("Birds"));
            row.Add(String.Format("IsPlayable"));
            writer.WriteRow(row);
        }
    }

    /**
    *   Saves a level on the CSV file containing the fitness
    *   @param[in]  level       the level to be saved
    *   @param[in]  pk          number of pigs at the end of level
    *   @param[in]  fitness     fitness of the level given in simulation
    *   @param[in]  filename    name of the CSV file
    */
    public static void SaveCSVLevel(ShiftABLevel level, float pk, float fitness, String filename)
    {
        //Open the reader
        try
        {
            CsvFileReader reader = new CsvFileReader(filename);
            reader.Close();
        }
        catch
        {
            createCSV(filename);
        }
        // Write data to CSV file
        using (CsvFileWriter writer = new CsvFileWriter(filename, true))
        {
            CsvRow row = new CsvRow();
            //Save each original stack on "even" clumns
            for (int i = 0; i < 11; i++)
            {
                int j = 0;
                if (level.GetStack(i) != null)
                {
                    //Add each object in a line
                    foreach (ShiftABGameObject abObj in level.GetStack(i))
                    {
                        row.Add(String.Format("{0}", abObj.Label));
                        j++;
                    }
                }
                //Fills with -1 where there are no objects
                for (; j < 17; j++)
                {
                    row.Add(String.Format("{0}", -1));
                }

                j = 0;
                if (level.GetStack(i) != null)
                {
                    //If object is doubled, save on the corresponding "odd" column of the matrix, -1 otherwise
                    foreach (ShiftABGameObject abObj in level.GetStack(i))
                    {
                        if (abObj.IsDouble)
                            row.Add(String.Format("{0}", abObj.Label));
                        else
                            row.Add(String.Format("{0}", -1));
                        j++;
                    }
                }
                //Fills with -1 where there are no objects
                for (; j < 17; j++)
                {
                    row.Add(String.Format("{0}", -1));
                }
            }
            //Adds the number of birds, if level is playable(1) or not (0) and fitness
            row.Add(String.Format("{0}", level.birdsAmount));
            if (pk < 1)
                row.Add(String.Format("{0}", 1));
            else
                row.Add(String.Format("{0}", 0));
            row.Add(String.Format("{0}", fitness));
            writer.WriteRow(row);
        }
    }

    /**
    *   Saves a level on the CSV file without fitness
    *   @param[in]  level       the level to be saved
    *   @param[in]  pk          number of pigs at the end of level
    *   @param[in]  filename    name of the CSV file
    */
    public static void SaveCSVLevel(ShiftABLevel level, String filename)
    {
        //Open the reader
        try
        {
            CsvFileReader reader = new CsvFileReader(filename);
            reader.Close();
        }
        catch
        {
            createCSVWithoutFitness(filename);
        }
        // Write data to CSV file
        using (CsvFileWriter writer = new CsvFileWriter(filename, true))
        {
            CsvRow row = new CsvRow();
            //Save each original stack on "even" clumns
            for (int i = 0; i < 11; i++)
            {
                int j = 0;
                if (level.GetStack(i) != null)
                {
                    //Add each object in a line
                    foreach (ShiftABGameObject abObj in level.GetStack(i))
                    {
                        row.Add(String.Format("{0}", abObj.Label));
                        j++;
                    }
                }
                //Fills with -1 where there are no objects
                for (; j < 17; j++)
                {
                    row.Add(String.Format("{0}", -1));
                }

                j = 0;
                if (level.GetStack(i) != null)
                {
                    //If object is doubled, save on the corresponding "odd" column of the matrix, -1 otherwise
                    foreach (ShiftABGameObject abObj in level.GetStack(i))
                    {
                        if (abObj.IsDouble)
                            row.Add(String.Format("{0}", abObj.Label));
                        else
                            row.Add(String.Format("{0}", -1));
                        j++;
                    }
                }
                //Fills with -1 where there are no objects
                for (; j < 17; j++)
                {
                    row.Add(String.Format("{0}", -1));
                }
            }
            //Adds the number of birds, if level is playable(1) or not (0) and fitness
            row.Add(String.Format("{0}", level.birdsAmount));
            row.Add(String.Format("{0}", 0));
            writer.WriteRow(row);
        }
    }
    /**
    *   Loads a level in CSV file and writes it on screen
    *   @param[in]  CSVString   the line of the CSV file describing the level
    */
    static void LoadCSVLevel(string CSVString)
    {
        using (CsvFileReader reader = new CsvFileReader(CSVString))
        {
            CsvRow row = new CsvRow();
            while (reader.ReadRow(row))
            {
                foreach (string s in row)
                {
                    Console.Write(s);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    }
}