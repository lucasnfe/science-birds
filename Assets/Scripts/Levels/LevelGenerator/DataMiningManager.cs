using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/** \class  DataMiningManager
*  \brief  manages the paths of the classification and regression models and the input data on them
*
*   Contains the path to both the classification and the regression models, creates instances to be classified by the model using the data of a level,
*   creates a data base model based on the hard-coded model of the CSV file that originated the inputs from the classification and regression models,
*   also evaluates a level using the classification or the regression model defined in the respective path
*/
namespace Assets.Scripts.Levels.LevelGenerator
{
    class DataMiningManager
    {
        /**Path to the classification model*/
        public string modelClassifierPath = "Assets/Resources/ClassifierModel/RFALL.model";
        /**Path to the regression model*/
        public string modelRegressionPath = "Assets/Resources/ClassifierModel/MLPRegressor.model";
        /**Creates an input instance to be evaluated by a data mining model
        *   @param[in]  level       the level to be evaluated
        *   @param[out] _instance   the created instance based on the level
        */
        public void CreateInstance(ShiftABLevel level, ref weka.core.Instance _instance)
        {
            //19 rows * 26 columns + bird number + class
            _instance = new weka.core.DenseInstance(376);
            int index = 0;
            //Iterates through all the "even" (starting in 0) columns of the matrix representation of the level
            for (int i = 0; i < 11; i++)
            {
                int j = 0;
                //If stack not empty, fill the data about its blocks
                if (level.GetStack(i) != null)
                {
                    //For each block, add its label as the value in the corresponding "matrix" index
                    foreach (ShiftABGameObject abObj in level.GetStack(i))
                    {
                        _instance.setValue(index++, abObj.Label);
                        j++;
                    }
                }
                //For all the other lines of the stack, fill with -1, the empty value
                for (; j < 17; j++)
                {
                    _instance.setValue(index++, -1);
                }

                j = 0;
                //Check which block is doubled and add them to the "odd" (starting in 1) correspinding column of the matrix
                if (level.GetStack(i) != null)
                {
                    foreach (ShiftABGameObject abObj in level.GetStack(i))
                    {
                        if (abObj.IsDouble)
                            _instance.setValue(index++, abObj.Label);
                        else
                            _instance.setValue(index++, -1);
                        j++;
                    }
                }
                for (; j < 17; j++)
                {
                    _instance.setValue(index++, -1);
                }
            }
            //Sets the value for the number of birds and a default class value
            _instance.setValue(index++, level.birdsAmount);
            _instance.setValue(index, 0);
        }
        /** Creates a data set modeled after the CSV file. 
        *   This is needed to define the structure of instances and define its class variable
        *   @param[out] insts   the created instances data set
        */
        public void CreateDataSet(ref weka.core.Instances insts)
        {
            //Creates an array list with all the attributes needed for the data set
            java.util.ArrayList attributes = new java.util.ArrayList();
            for (int i = 0; i < 22; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    attributes.Add(new weka.core.Attribute(String.Format("(X, Y){0}, {1}", i, j)));
                }
            }
            attributes.Add(new weka.core.Attribute(String.Format("Birds")));
            attributes.Add(new weka.core.Attribute(String.Format("Fitness")));
            insts = new weka.core.Instances("Level", attributes, 1);
        }
        /** 
        *   Evaluates a level using the corresponding classifier
        *   @param[in]  _mylevel        the level to be evaluated
        *   @param[out] _myClassifier   the classifier model used to evaluate the level
        *   @return     int             the class which the level belongs to
        */
        public int EvaluateUsingClassifier(ShiftABLevel _mylevel, weka.classifiers.Classifier _myClassifier)
        {
            //Creates the data set model of the instances and initializes it
            weka.core.Instances insts = null;
            CreateDataSet(ref insts);
            //Creates and initializes the instance representing the level to be evaluated
            weka.core.Instance _instance = null;
            CreateInstance(_mylevel, ref _instance);
            //Sets the class index to the last one
            insts.setClassIndex(insts.numAttributes() - 1);
            //Sets the created instance as belonging to the created data set
            _instance.setDataset(insts);
            //Evaluates the instance with the classifier
            double predictedClass = _myClassifier.classifyInstance(_instance);
            //Binarize the result
            if (Math.Abs(predictedClass) < 0.5)
                return 0;
            else
                return 1;
        }

        /** 
        *   Evaluates a level using the corresponding regression model
        *   @param[in]  _mylevel        the level to be evaluated
        *   @param[out] _myClassifier   the regression model used to evaluate the level
        *   @return     float           the fitness value obtained
        */
        public double EvaluateUsingRegression(ShiftABLevel _mylevel, weka.classifiers.Classifier _myClassifier)
        {
            //Creates the data set model of the instances and initializes it
            weka.core.Instances insts = null;
            CreateDataSet(ref insts);
            //Creates and initializes the instance representing the level to be evaluated
            weka.core.Instance _instance = null;
            CreateInstance(_mylevel, ref _instance);
            //Sets the class index to the last one
            insts.setClassIndex(insts.numAttributes() - 1);
            //Sets the created instance as belonging to the created data set
            _instance.setDataset(insts);
            //Evaluates the instance with the regression model
            double predictedClass = _myClassifier.classifyInstance(_instance);

            return predictedClass;
        }
    }
}
