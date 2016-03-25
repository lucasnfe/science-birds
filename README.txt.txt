This file contains instructions to setup your project.

1 - About WEKA
This project uses the WEKA software to create the classifier and regression models to evaluate the generated levels.
The Weka software can be found here: http://www.cs.waikato.ac.nz/ml/weka/

2 - About WEKA API
To use the Weka API, which is coded in Java, it is necessary to create DLLs, usable in the C# project in visual studio.
To do so the IKVM software is used. The software can be downloaded here: http://www.ikvm.net/
Instructions to compile the WEKA API using IKVM can be found here: https://weka.wikispaces.com/IKVM+with+Weka+tutorial
The basic compiling command is here: ikvmc -target:library weka.jar

3 - The DLLs
The DLLs compiled with WEKA 3.7 API are already in the project folder under /Assets/References
And are referenced in the project. To add or remove any references in your project, check here: https://msdn.microsoft.com/en-us/library/wkze6zky(v=vs.120).aspx

About the project:

It is possible to save every created level to a XML file, which can be read to rebuild the same level in the project.To do so, just check the box Save All XML in the project's scene Level Generator.
The same is applied to a CSV file, which is used as the input to a classifier or regression model in weka. To do so, just check the box Save All CSV in the project's scene Level Generator.
To use the classifier model to evaluate the levels, which is found under Assets/Resources/ClassifierModel, just check the Use Classifier box in the project's scene Level Generator.
To use the regression model to evaluate the levels, which is found under Assets/Resources/ClassifierModel, just check the Use Regression box in the project's scene Level Generator.
To use the classifier model to create the initial population, which is found under Assets/Resources/ClassifierModel, just check the Create Pre Population box in the project's scene Level Generator.
To change the models used, go to the Script DataMiningManager.cs, which is in the folder Assets/Scripts/Levels/LevelGenerator. The paths are defined in lines 16 and 17.

WARNING:
Regression model is unstable. Need to create a bigger matrix to save the level, as some have more columns and/or lines than the 22x17 that created the model.