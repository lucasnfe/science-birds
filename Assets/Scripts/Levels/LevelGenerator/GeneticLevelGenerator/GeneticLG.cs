using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Levels.LevelGenerator;
/** \class  GeneticLG
*  \brief  Creates a level by accordingly to the genetic algorithm
*
*  contains a lot of varaiables for the GA, if ranking a gene, and gene is not already in cache, play it
*  with the AI to evaluate it. If game ended gets the fitness if evaluated all the population
*  check if stop criteria is met, if not, starts next generation, if met, ends evolution.
*  Also has methods to evaluate genome, calculate fitness, end the evolution, check stop criteria,
*  crossover, mutation, genome initialization and saving logs and writing it to files.
*/
public class GeneticLG : RandomLG
{
    // Experiments variables
    /**Number of fitness evaluated during all evolution*/
    private int _fitnessEvaluation;
    /**Number of fitness that skipped evaluation because of classifier*/
    private int _fitnessNotEvaluated = 0;
    /**Number of fitness recovered from cache during all evolution*/
    private int _fitnessRecovered;
    /**Index of current experiment of the GA*/
    private static int _experimentsIdx;

    // Control variables
    /**genome and generation indexes*/
    private int _genomeIdx, _generationIdx;
    /**Counter for how many generation the best fitness has been the same*/
    private int _sameBestFitnessCount;
    /**If a genome is being ranked, true, false otherwise*/
    private bool _isRankingGenome;
    /**If a genome is being ranked only to compare the results with the regression output true, false otherwise*/
    private bool _isRankingWithRegression;
    /**Best fitness from previous generation*/
    private float _lastGenerationBestFitness;
    /**Path to log file*/
    private const string _logFile = "Assets/Experiments/generations.txt";
    /**Path to log file with original method*/
    private const string _logFileOrig = "Assets/Experiments/OriginalAEGenerations.txt";
    /**Path to log file using the classifier*/
    private const string _logFileClass = "Assets/Experiments/ClassifierGenerations.txt";
    /**Path to log file creating the initial population with the classifier*/
    private const string _logFilePrepop = "Assets/Experiments/PrePopGenerations.txt";
    /**Max number of blocks on one level*/
    private const int _blocksMaxAmount = 100;
    /**Total time passed from start to last experiment*/
    private static float _lastExperimentTime;
    /**Content for log and accumulated fitness*/
    private static string _genLog, _logOriginal, _logClassif, _logPrepop;
    /**Content for cache log and recovered from cache */
    private static string _cacheLog, _recoverLog;
    /**the genetic algorithm object*/
    private GeneticAlgorithm<AngryBirdsGen> _geneticAlgorithm;
    /**The Data Mining Manager object*/
    private DataMiningManager _dataMiningManager = new DataMiningManager();

    /**Hash table used to cache fitness calculation*/
    private Dictionary<AngryBirdsGen, float> _fitnessCache;
    /**The last genome used*/
    private AngryBirdsGen _lastgenome;
    //Delete this later, just for debugging
    private AngryBirdsGen _auxgenome;

    /**Fitness function parameters*/
    public float _bn, _d;

    // Genetic Algorithm parameters
    /**Size of population and generations*/
    public int _populationSize, _generationSize;
    /**mutation and crossover rates*/
    public float _mutationRate, _crossoverRate;
    /**True if will have elitism, false otherwise*/
    public bool _elitism;
    /**Amount of experiments to be run*/
    public int _experimentsAmount = 1;

    /**Check if needs to save all the levels on XML and/or CSV format*/
    public bool saveAllXML;
    public bool saveAllCSV;
    /**Check if is using Classifier model or not*/
    public bool useClassifier;
    /**Check if is using Regression model or not*/
    public bool useRegression;
    /**Check if it is creating a pre-population using a Classifier Model*/
    public static bool setCreatePrePopulation;
    /**Boolean to check if the initial population is going to be created by the classifier*/
    public bool createPrePopulation;
    /**Boolean to check if initial population will be loaded from xml files or not*/
    public bool maintainInitPop;
    /** Index to determine which algorithm is being used in a batch test. 0 When using the original algorithm
     *  1 when using the classifier algorithm.
     *  The value is reset to 0 after a experiment is finished, in order to begin the next one.
     */
    private static int samePopExpIdx = 0;
    /**List containing the best fitness of each generation*/
    private List<float> bestFitnesses = new List<float>();
    /**Classifier to be used to classify the levels*/
    private weka.classifiers.Classifier cl;
    /**Fitness obtained by the regression model to compare when playing the same level with the AI*/
    private List<float> fitnessToCompare = new List<float>();
    /** 
     *  Auxiliary variable used when using the regression model, checks if the best individual, that is being evaluated by the IA
     *  Has finished execution
     */
    private bool isEnding = false;

    /**
     *  At the start of this script, calls the method to initialize the GA
     *  It creates a cache for fitness, initializes the GA variables, 
     *  set is ranking genome to false, sets the generation and genome indexes to 0, set time scale to 1, 
     *  Sets camera width to infinity, and remove all objects from the world.
     */
    public void Start()
    {
        InitializeGeneticLG();
    }

    /**
     *  Generates a new level
     *  @return ABLevel a new Angry Birds level
     */
    public override ABLevel GenerateLevel()
    {
        return new ABLevel();
    }

    /**
     *  At every update, if not ranking genome, gets the Nth genome, adds 1 to fitness evaluation, check if 
     *  if fitness is on cache, if not, clears the world and start evaluating the actual genome. If in cache, 
     *  set the genome index to the next, sets is ranking genome to false, and adds 1 to fitness recovered.
     *  If ranking a genome, check if levle is stable and pigs or birds amount is 0, if so, ends the evaluation
     *  of the genome, clears the world, and set is ranking genome to false.
     *  if genome index is equal to the size of population, ranks the population, check stop criteria, 
     *  save generation log, and check if generation index is lesser than the amount of generations, 
     *  if same best fitness has been counted less than 10 times, and if best fitness is greater than 0
     *  (all the stop criterias) If none are met, start next generation. If one of them is met, end the evolution
     *  Also sets genome index to 0 and adds 1 to generation count.
     */
    void Update()
    {
        if (!_isRankingGenome)
        {
            float fitness = Mathf.Infinity;
            _geneticAlgorithm.GetNthGenome(_genomeIdx, out _lastgenome, out fitness);
            _fitnessEvaluation++;

            if (!_fitnessCache.ContainsKey(_lastgenome))
            {

                /**Classifier tested here*/
                //CSVManager.SaveCSVLevel(_lastgenome.level, "Assets/Resources/GeneratedLevels/CSV_Test/levels.csv");
                if (useClassifier)
                {
                    //If classified as a level that can't be completed, gives a bad fitness
                    if (_dataMiningManager.EvaluateUsingClassifier(_lastgenome.level, cl) == 0)
                    {
                        EndEvaluatingGenome(100);
                        _fitnessNotEvaluated++;
                    }
                    //Else, play the level using the AI
                    else
                    {
                        GameWorld.Instance.ClearWorld();
                        StartEvaluatingGenome();
                    }
                }
                //If using regression, use the model to determine the fitness
                else if (useRegression)
                {
                    float _lFitness = (float)_dataMiningManager.EvaluateUsingRegression(_lastgenome.level, cl);
                    Debug.Log("Fitness:" + _lFitness);
                    EndEvaluatingGenome(_lFitness);
                }
                //If not using regression or classifier, just play the game with the AI
                else
                {
                    Debug.Log("Clearing World");
                    GameWorld.Instance.ClearWorld();
                    StartEvaluatingGenome();
                }
            }
            //If genome is in the cache, no need to play the level
            else
            {
                _genomeIdx++;
                _isRankingGenome = false;
                _fitnessRecovered++;
            }
        }
        //If playing through the level has finished, end the evaluation for this genome
        else if (GameWorld.Instance.IsLevelStable() &&
               (GameWorld.Instance.GetPigsAvailableAmount() == 0 ||
                GameWorld.Instance.GetBirdsAvailableAmount() == 0))
        {

            EndEvaluatingGenome();

            GameWorld.Instance.ClearWorld();
            _isRankingGenome = false;
            //Only used when regression is active
            if (isEnding)
                EndEvolution();
        }
        //If evaluated the whole population, gets the best fitness, save the log for this generation and creates
        //A new generation if stop criteria haven't been reached
        if (_genomeIdx == _geneticAlgorithm.PopulationSize)
        {
            _geneticAlgorithm.RankPopulation();

            float bestFitness = CheckStopCriteria();

            /**if (useRegression)
            {
                float fitness = Mathf.Infinity;
                AngryBirdsGen genome = new AngryBirdsGen();
                _geneticAlgorithm.GetBest(out genome, out fitness);
                StartEvaluatingGenome(ref genome);
            }*/
            SaveGenerationLog();
            
            bestFitnesses.Add(bestFitness);
            
            //If stop criteria has not been reached, creates the next generation
            if (_generationIdx < _geneticAlgorithm.Generations && _sameBestFitnessCount < 10 )
            {
                _geneticAlgorithm.CreateNextGeneration();
            }
            //If it has been reached do th efollowing
            else
            {
                //If using regression plays the best genome with the AI to compare the results
                if (useRegression)
                {
                    float fitness = Mathf.Infinity;
                    AngryBirdsGen genome = new AngryBirdsGen();
                    _geneticAlgorithm.GetBest(out genome, out fitness);
                    StartEvaluatingGenome(ref genome);
                    isEnding = true;
                }
                //If not, ends the evolution
                else
                    EndEvolution();
            }
            _genomeIdx = 0;
            _generationIdx++;
        }
    }

    /**
     *  Starts evaluating a genome by first converting the level received as a parameter to a 
     *  Playable level, and then decoding it as a game world level. Also sets is ranking genome to true.
     *  @param[in]  genome  The level to be evaluated 
    */
    private void StartEvaluatingGenome(ref AngryBirdsGen genome)
    {
        ConvertShiftGBtoABGB(ref genome.level);
        GameWorld.Instance.DecodeLevel(genome.level.gameObjects, _lastgenome.level.birdsAmount);
        _isRankingGenome = true;
        _isRankingWithRegression = true;
    }
    /**
     *  Starts evaluating a genome by first converting the level of the last genome to a 
     *  Playable level, and then decoding it as a game world level. Also sets is ranking genome to true.
     */
    private void StartEvaluatingGenome()
    {
        //Debug.Log("Is Updating");
        ConvertShiftGBtoABGB(ref _lastgenome.level);
        Debug.Log("Level Converted");
        GameWorld.Instance.DecodeLevel(_lastgenome.level.gameObjects, _lastgenome.level.birdsAmount);
        Debug.Log("Level Decoded");
        _isRankingGenome = true;
    }
    /**
     *  Ends the evaluation of the genome by getting the number of birds, pigs and blocks at the start
     *  and currently available. Also the stability until first bird. Then calculates the fitness passing
     *  all these variables, and adds 1 to genome index.
     */
    private void EndEvaluatingGenome()
    {
        float bi = GameWorld.Instance.BirdsAtStart;
        float pi = GameWorld.Instance.PigsAtStart;
        float li = GameWorld.Instance.BlocksAtStart;

        float bk = GameWorld.Instance.GetBirdsAvailableAmount();
        float pk = GameWorld.Instance.GetPigsAvailableAmount();
        float lk = GameWorld.Instance.GetBlocksAvailableAmount();
        float sk = GameWorld.Instance.StabilityUntilFirstBird;
        float fitness = Fitness(pk, pi, li, lk, bi, bk, _d, sk);
        _fitnessCache[_lastgenome] = Fitness(pk, pi, li, lk, bi, bk, _d, sk);
        //Save initial population to retest
        /*if(_generationIdx == 0)
        {
            string name = "InitPop_";
            LevelLoader.SaveXmlGenome(_lastgenome.level, name+_genomeIdx);
        }*/
        if (saveAllXML)
            LevelLoader.SaveXmlLevel(_lastgenome.level);
        if (saveAllCSV)
            CSVManager.SaveCSVLevel(_lastgenome.level, pk, fitness, "Assets/Resources/GeneratedLevels/CSV/levels.csv");
        if (_isRankingWithRegression)
        {
            Debug.Log("IsCalculatingFitness");
            fitnessToCompare.Add(fitness);
            _isRankingWithRegression = false;
        }
        else
            _genomeIdx++;
    }
    /**
     *  Ends the evaluation of the genome with a fitness received from a classifier or regression model
     *  And adds 1 to the index of the genome list
     *  @param[in]  fitness The fitness received from the classifier or regression model
     */
    private void EndEvaluatingGenome(float fitness)
    {
        /**Symbolic to make level not "finishable"*/
        float pk = 2;
        _fitnessCache[_lastgenome] = fitness;
        if (saveAllXML)
            LevelLoader.SaveXmlLevel(_lastgenome.level);
        if (saveAllCSV)
            CSVManager.SaveCSVLevel(_lastgenome.level, pk, fitness, "Assets/Resources/GeneratedLevels/CSV/levels.csv");
        _genomeIdx++;
    }
    /**
     *  Calculates the fitness by summing the distance between the number of birds that should have been
     *  used to complete the level from the number that was really used, and the distance between the number of
     *  desirable blocks and the number of blocks in the level, also adding the stability until first bird
     *  And number of pigs left on stage.
     *  @param[in]  pk  Pigs at the end of the level
     *  @param[in]  pi  Pigs at the start of the level
     *  @param[in]  li  Blocks at the start of the level
     *  @param[in]  lk  Blocks at the end of the level
     *  @param[in]  bi  Birds at the start of the level
     *  @param[in]  bk  Birds at the end of the level
     *  @param[in]  d   Parameter for the fitness 
     *  @param[in]  sk  Stability until first bird thrown
     *  @return float   Calculated fitness for genome.
     */
    private float Fitness(float pk, float pi, float li, float lk, float bi, float bk, float d, float sk)
    {
        float distBirds = Mathf.Abs(Mathf.Ceil(_bn * bi) - (bi - bk));
        float distAmountBlocks = Mathf.Abs((Mathf.Ceil(d * _blocksMaxAmount) - li));
        float fitness = distBirds + distAmountBlocks + (sk + pk);
        if (fitness < 0.01f)
            return 0;
        return fitness;
    }

    /**
     *  Evaluates using the fitness contained at the cache
     *  @param[in]  genome      Genome to be evaluated.
     *  @param[in]  genomeIdx   Index of genome to be evaluated.
     *  @return     float       Fitness contained at the cache for the genome.
     */
    public float EvaluateUsingAI(AngryBirdsGen genome, int genomeIdx)
    {
        return _fitnessCache[genome];
    }


    /**
     *  Ends the evolution. Adds 1 to experiments index, saves the results on the log,
     *  If index of experiments is lesser than the total of experiments, Load another level to run
     *  The next experiment. If already equals the total, gets the best genome with the best fitness, 
     *  Writes to log file, default the time scale to 1, plays level starting audio, decode the best genome
     *  of the last generation in a level, save level in xml, disable simulation, disable AI and
     *  allow the player to play the level, and destroy the generator of levels.
     */
    private void EndEvolution()
    {
        // 0 means that the log is being saved when a generation has finished executing but the program is still running
        int onExec = 0;
        // Save results in the corresponding files. 0 means the original EA, 1 means that the classifier has been used
        // and 1 means that the initial population was generated by the classifier
        if (samePopExpIdx == 0)
        {
            SaveLog(ref _logOriginal, onExec);
            WriteLogToFile(_logFileOrig, _logOriginal);
        }
        else if (samePopExpIdx == 1)
        {
            SaveLog(ref _logClassif, onExec);
            WriteLogToFile(_logFileClass, _logClassif);
        }
        else if (samePopExpIdx == 2)
        {
            SaveLog(ref _logPrepop, onExec);
            WriteLogToFile(_logFilePrepop, _logPrepop);
        }
        else
        {
            Debug.LogError("Wrong Same Population Experiment Index");
        }
        samePopExpIdx++;
        //2 if find some way to use pre pop
        if (samePopExpIdx > 1)
        {
            samePopExpIdx = 0;
            _experimentsIdx++;
        }
        //TODO remove this when doing different experiments
        //_experimentsIdx++;
        float fitness = Mathf.Infinity;

        AngryBirdsGen genome = new AngryBirdsGen();
        _geneticAlgorithm.GetBest(out genome, out fitness);

        // Clear the level and decode the best genome of the last generation
        GameWorld.Instance.DecodeLevel(genome.level.gameObjects, genome.level.birdsAmount);

        // Save file in xml for future use
        LevelLoader.SaveXmlLevel(genome.level, useClassifier, useRegression, createPrePopulation);
        //If there are more experiments to be run, run them
        if (_experimentsIdx < _experimentsAmount)
        {
                Application.LoadLevel(Application.loadedLevel);
        }
        else
        {
            // Default time scale
            Time.timeScale = 1f;

            // Play level starting audio
            GameWorld.Instance.GetComponent<AudioSource>().PlayOneShot(GameWorld.Instance._clips[0]);
            GameWorld.Instance.GetComponent<AudioSource>().PlayOneShot(GameWorld.Instance._clips[1]);

            // Clear the level and decode the best genome of the last generation
            //GameWorld.Instance.DecodeLevel(genome.level.gameObjects, genome.level.birdsAmount);	

            // Save file in xml for future use
            //LevelLoader.SaveXmlLevel(genome.level);

            // Disable simulation
            GameWorld.Instance._isSimulation = false;

            // Disable AI and allow player to test the level
            Destroy(GameWorld.Instance._birdAgent.gameObject);

            // Destroy the generator
            Destroy(this.gameObject);
        }
    }

    /**
     *  Checks the stop criteria, getting the best fitness of the generation, compating it to the best
     *  of the last generation, if equal, adds 1 to same best fitness counter, if not, set it to 0.
     *  sets last generation best fitness to the actual best fitness, and return it.
     *  @return float   best fitness of this generation.
     */
    private float CheckStopCriteria()
    {
        float fitness = Mathf.Infinity;
        AngryBirdsGen genome = new AngryBirdsGen();
        _geneticAlgorithm.GetBest(out genome, out fitness);

        if (_lastGenerationBestFitness == fitness)

            _sameBestFitnessCount++;
        else
            _sameBestFitnessCount = 0;

        _lastGenerationBestFitness = (float)fitness;

        return fitness;
    }

    /**
     *  Makes the crossover if the random value is lesser or equal the mutation rate, 
     *  getting the max genome size from both parents, and for each object in each stack 
     *  choosing randomly from what parent to take the object in that stack in that position
     *  if one of them is shorter than the other, the last stacks are copied from the larger parent.
     *  Then does an integer crossover to get the number of birds.
     *  If no crossover occurs, just copies all the objects and number of birds of parent1 to child1
     *  and of parent2 to child2.
     *  After any of the procedures, fix the level size and set the child genomes to the ones created.
     *  @param[out] genome1 parent 1 genome
     *  @param[out] genome2 parent 2 genome
     *  @param[out] child1 child 1 genome
     *  @param[out] child2 child 1 genome  
     */
    public void Crossover(ref Genome<AngryBirdsGen> genome1, ref Genome<AngryBirdsGen> genome2,
                          out Genome<AngryBirdsGen> child1, out Genome<AngryBirdsGen> child2)
    {

        child1 = new Genome<AngryBirdsGen>();
        child2 = new Genome<AngryBirdsGen>();

        AngryBirdsGen genes1 = new AngryBirdsGen();
        AngryBirdsGen genes2 = new AngryBirdsGen();
        if (UnityEngine.Random.value <= _geneticAlgorithm.CrossoverRate)
        {
            int maxGenomeSize = Mathf.Max(genome1.Genes.level.GetStacksAmount(),
                                           genome2.Genes.level.GetStacksAmount());

            for (int i = 0; i < maxGenomeSize; i++)
            {
                if (genome1.Genes.level.GetStacksAmount() == genome2.Genes.level.GetStacksAmount())
                {
                    if (UnityEngine.Random.value < 0.5f)
                        genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
                    else
                        genes1.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));

                    if (UnityEngine.Random.value < 0.5f)
                        genes2.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
                    else
                        genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
                }
                else if (genome1.Genes.level.GetStacksAmount() < genome2.Genes.level.GetStacksAmount())
                {
                    if (i < genome1.Genes.level.GetStacksAmount())
                    {
                        if (UnityEngine.Random.value < 0.5f)
                            genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
                        else
                            genes1.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));


                        if (UnityEngine.Random.value < 0.5f)
                            genes2.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
                        else
                            genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
                    }
                    else
                    {
                        if (UnityEngine.Random.value < 0.5f)
                            genes1.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));

                        if (UnityEngine.Random.value < 0.5f)
                            genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
                    }
                }
                else
                {
                    if (i < genome2.Genes.level.GetStacksAmount())
                    {
                        if (UnityEngine.Random.value < 0.5f)
                            genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
                        else
                            genes1.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));

                        if (UnityEngine.Random.value < 0.5f)
                            genes2.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
                        else
                            genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
                    }
                    else
                    {
                        if (UnityEngine.Random.value < 0.5f)
                            genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));

                        if (UnityEngine.Random.value < 0.5f)
                            genes2.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
                    }
                }
            }

            // Integer crossover for birds
            genes1.level.birdsAmount = (int)(0.5f * genome1.Genes.level.birdsAmount + 0.5f * genome2.Genes.level.birdsAmount);
            genes2.level.birdsAmount = (int)(1.5f * genome1.Genes.level.birdsAmount - 0.5f * genome2.Genes.level.birdsAmount);
            //There are levels with -1 birds.
            //TODO Check later if ok
            if (genes2.level.birdsAmount < 0)
                genes2.level.birdsAmount = 0;
        }
        else
        {
            for (int i = 0; i < genome1.Genes.level.GetStacksAmount(); i++)
            {
                genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
            }

            genes1.level.birdsAmount = genome1.Genes.level.birdsAmount;

            for (int i = 0; i < genome2.Genes.level.GetStacksAmount(); i++)
            {
                genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
            }

            genes2.level.birdsAmount = genome2.Genes.level.birdsAmount;
        }

        genes1.level.FixLevelSize();
        genes2.level.FixLevelSize();

        child1.Genes = genes1;
        child2.Genes = genes2;
    }

    /**
     *  Mutates the genomes, checking for each stack  if the random value is lesser or equal the mutation rate.
     *  If so, clear the mutated stack, sets it to a new Linked List of shift AB game object
     *  and generate another stack passing the level playable width and height and the width of and empty stack.
     *  also mutates the birds amount (if the random generated number says so), generating a new random amount.
     *  Then fixes the level size.
     *  @param[out] genome  genome to be mutated
     */
    public void Mutate(ref Genome<AngryBirdsGen> genome)
    {

        for (int i = 0; i < genome.Genes.level.GetStacksAmount(); i++)
        {
            if (UnityEngine.Random.value <= _geneticAlgorithm.MutationRate)
            {
                genome.Genes.level.GetStack(i).Clear();
                genome.Genes.level.SetStack(i, new LinkedList<ShiftABGameObject>());

                // Generate new stacks
                genome.Genes.level.SetStack(i, GenerateStack(genome.Genes.level.LevelPlayableHeight,
                                                               genome.Genes.level.LevelPlayableWidth,
                                                               genome.Genes.level.WidthOfEmptyStack));
            }
        }

        if (UnityEngine.Random.value <= _geneticAlgorithm.MutationRate)
            genome.Genes.level.birdsAmount = UnityEngine.Random.Range(0, ABLevel.BIRDS_MAX_AMOUNT);

        genome.Genes.level.FixLevelSize();
    }

    /**
     *  Initializes a genome, setting its bird amount randomly and generating a random level.
     *  @param[out] genome  Genome to be generated randomly
     */
    public void InitAngryBirdsGenome(out AngryBirdsGen genome)
    {

        genome = new AngryBirdsGen();

        genome.level.birdsAmount = UnityEngine.Random.Range(0, ABLevel.BIRDS_MAX_AMOUNT);
        genome.level = GenerateRandomLevel();
    }
    /**
     *  Initializes a genome, setting its bird amount and the level based on a loaded level genome.
     *  @param[out] genome  Genome to be generated randomly
     */
    public void InitAngryBirdsGenomeWithPrePop(out AngryBirdsGen genome, ShiftABLevel level)
    {
        genome = new AngryBirdsGen();
        genome.level.birdsAmount = level.birdsAmount;
        genome.level = level;
    }

    /**
     *  Saves the log of the generation, saving the best fitness, the size of the cache, and the 
     *  Amount of times the fitness were recovered from the cache.
     */
    private void SaveGenerationLog()
    {
        float fitness = Mathf.Infinity;
        AngryBirdsGen genome = new AngryBirdsGen();
        _geneticAlgorithm.GetBest(out genome, out fitness);

        _genLog += fitness + " ";
        _cacheLog += _fitnessCache.Count + " ";
        _recoverLog += _fitnessRecovered + " ";
    }

    /**
     *  Saves the log for the experiment, writing the experiment time, the convergence, cache size,
     *  number of times fitness was calculated and times it was recovered, best fitness, best level linearity
     *  and density, best level frequency of pigs and birds, fitness, cache size and usage evolution.
     *  also updates the time of the last experiment.
     */
    private void SaveLog(ref String _logContent, int onQuit)
    {
        Debug.Log("Saving Log");
        float fitness = Mathf.Infinity;
        AngryBirdsGen genome = new AngryBirdsGen();
        _geneticAlgorithm.GetBest(out genome, out fitness);

        float experimentTime = Time.realtimeSinceStartup - _lastExperimentTime;

        if (onQuit == 1)
        {
            _logContent += "=========== SAVED WHEN PROGRAM EXITED ===============\n";
        }
        //Debug.Log("Size:" + fitnessToCompare.Count);
        _logContent += "====== RESULTS ======\n";
        _logContent += "Execution time: " + experimentTime + "\n";
        _logContent += "Convergence: " + _generationIdx + "\n";
        _logContent += "Cache size:" + _fitnessCache.Count + "\n";
        _logContent += "Fitness calculations: " + _fitnessEvaluation + "\n";
        _logContent += "Fitness skipped: " + _fitnessNotEvaluated + "\n";
        _logContent += "Fitness recovered: " + _fitnessRecovered + "\n";
        _logContent += "Best Fitness: " + fitness + "\n";
        _logContent += "Linearity: " + genome.level.GetLevelLinearity() + "\n";
        _logContent += "Density: " + genome.level.GetLevelDensity() + "\n";
        _logContent += "Frequency pig: " + genome.level.GetABGameObjectFrequency(GameWorld.Instance.Templates.Length) + "\n"; ;
        _logContent += "Frequency bird: " + genome.level.GetBirdsFrequency() + "\n";
        _logContent += "Fitness Evolution: " + _genLog + "\n";
        _logContent += "Cache Size Evolution: " + _cacheLog + "\n";
        _logContent += "Cache Usage Evolution: " + _recoverLog + "\n";
        _logContent += "Best Fitness per generation: \n";
        _logContent += "Index Fitness\n";

        //_logContent += fitnessToCompare[0];

        int index = 0;
        foreach (var _fitness in bestFitnesses)
        {
            /*if(useRegression)
                _logContent += index + " Regression: " + _fitness +" AI: "+ fitnessToCompare[index] + "\n";*/
            //else
            _logContent += index + " " + _fitness + "\n";
            index++;
        }
        _genLog = "";
        _cacheLog = "";
        _recoverLog = "";

        _lastExperimentTime = Time.realtimeSinceStartup;
    }

    /**
     *  Writes the log content to the desired file.
     *  @param[in]  filename    Name of the file to write the log to
     *  @param[in]  content     What to write on the file
     */
    private void WriteLogToFile(string filename, string content)
    {
        StreamWriter writer = new StreamWriter(filename, true);
        writer.WriteLine(content);
        writer.Close();
    }

    /**
     *  When quitting the game, writes the log to the file.
     */
    private void OnApplicationQuit()
    {
        //If 1, indicates that the log is being written while quitting the program
        int onQuit = 1;
        //Log using the original EA
        if (samePopExpIdx == 0)
        {
            SaveLog(ref _logOriginal, onQuit);
            WriteLogToFile(_logFileOrig, _logOriginal);
        }
        //Log using the classifier
        else if (samePopExpIdx == 1)
        {
            SaveLog(ref _logClassif, onQuit);
            WriteLogToFile(_logFileClass, _logClassif);
        }
        //Log using the initial population created by the classifier
        else if (samePopExpIdx == 2)
        {
            SaveLog(ref _logPrepop, onQuit);
            WriteLogToFile(_logFilePrepop, _logPrepop);
        }
        else
        {
            Debug.LogError("Wrong Same Population Experiment Index");
        }
    }

    /**
     *  Initializes all the delegate functions, and the initial population, based if is loading a initial population
     *  Or creating one randomly
     */
    private void InitializeGenAlg()
    {

        _geneticAlgorithm = new GeneticAlgorithm<AngryBirdsGen>(_crossoverRate, _mutationRate, _populationSize, _generationSize, _elitism);
        _geneticAlgorithm.InitGenome = new GeneticAlgorithm<AngryBirdsGen>.GAInitGenome(InitAngryBirdsGenome);
        _geneticAlgorithm.InitGenomePrePop = new GeneticAlgorithm<AngryBirdsGen>.GAInitGenomePrePop(InitAngryBirdsGenomeWithPrePop);
        _geneticAlgorithm.Mutation = new GeneticAlgorithm<AngryBirdsGen>.GAMutation(Mutate);
        _geneticAlgorithm.Crossover = new GeneticAlgorithm<AngryBirdsGen>.GACrossover(Crossover);
        _geneticAlgorithm.FitnessFunction = new GeneticAlgorithm<AngryBirdsGen>.GAFitnessFunction(EvaluateUsingAI);
        //Starts the evolution based on a loaded initial population
        if (maintainInitPop)
        {
            _geneticAlgorithm.StartEvolutionPrePop();
        }
        //Starts the evolution based on a random generated initial population
        else
        {
            _geneticAlgorithm.StartEvolution();
        }
    }
    /**
     *  Initializes the variables for the genetic algorithm and calls the method to initialize the delegate functions and 
     *  Starts the algorithm from a initial population
     */
    private void InitializeGeneticLG()
    {
        //If using classifier
        if (samePopExpIdx == 1)
        {
            useClassifier = true;
            createPrePopulation = false;
        }
        //If using a initial population created by the classifier
        else if (samePopExpIdx == 2)
        {
            useClassifier = false;
            createPrePopulation = true;
        }
        //If using the original EA
        else
        {
            useClassifier = false;
            createPrePopulation = false;
        }
        //Constructs the classifier object
        if (useClassifier)
            cl = (weka.classifiers.trees.RandomForest)weka.core.SerializationHelper.read(_dataMiningManager.modelClassifierPath);
        //Constructs the regression object
        if (useRegression)
            cl = (weka.classifiers.functions.MLPRegressor)weka.core.SerializationHelper.read(_dataMiningManager.modelRegressionPath);
        //Used so the createPrePopulation button set in the Unity HUD value can be passed to another class
        setCreatePrePopulation = createPrePopulation;

        _fitnessCache = new Dictionary<AngryBirdsGen, float>();

        // Generate a population of feaseble levels evaluated by an inteligent agent
        InitializeGenAlg();

        //Set the initial variables to their initial states
        _isRankingGenome = false;
        _isRankingWithRegression = false;
        _generationIdx = 0;
        _genomeIdx = 0;

        //Set time scale to acelerate evolution
        Time.timeScale = 20f;

        // Totally zoom out
        GameWorld.Instance._camera.SetCameraWidth(Mathf.Infinity);

        // Remove all objects from level before start
        GameWorld.Instance.ClearWorld();
    }

}
