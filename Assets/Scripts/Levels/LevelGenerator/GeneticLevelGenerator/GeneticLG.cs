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
    /**Max number of blocks on one level*/
	private const int _blocksMaxAmount = 100;
    /**Not used anywhere!!!!*/
	private const int _averagingAmount = 1;
	/**Total time passed from start to last experiment*/
	private static float _lastExperimentTime;
    /**Content for log and accumulated fitness*/
	private static string _logContent, _genLog;
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
    public bool createPrePopulation;
    /**List containing the best fitness of each generation*/
    private List<float> bestFitnesses = new List<float>();
    /**Classifier to be used to classify the levels*/
    private weka.classifiers.Classifier cl;

    private List<float> fitnessToCompare = new List<float>();

    private bool isEnding = false;

    /**
     *  At the start of this script, creates a cache for fitness, initializes the GA variables, 
     *  set is ranking genome to false, sets the generation and genome indexes to 0, set time scale to 1, 
     *  Sets camera width to infinity, and remove all objects from the world.
     */
    public void Start()
	{
        //Constructs the classifier object
        if(useClassifier)
            cl = (weka.classifiers.trees.RandomForest)weka.core.SerializationHelper.read(_dataMiningManager.modelClassifierPath);
        //Constructs the regression object
        if (useRegression)
            cl = (weka.classifiers.functions.MLPRegressor)weka.core.SerializationHelper.read(_dataMiningManager.modelRegressionPath);
        //Used so the createPrePopulation button set in the Unity HUD value can be passed to another class
        setCreatePrePopulation = createPrePopulation;
        //Debug.Log("Experiment n: " + _experimentsIdx);

        _fitnessCache = new Dictionary<AngryBirdsGen, float>();

		// Generate a population of feaseble levels evaluated by an inteligent agent
		_geneticAlgorithm = new GeneticAlgorithm<AngryBirdsGen>(_crossoverRate, _mutationRate, _populationSize, _generationSize, _elitism);
		_geneticAlgorithm.InitGenome = new GeneticAlgorithm<AngryBirdsGen>.GAInitGenome(InitAngryBirdsGenome);
		_geneticAlgorithm.Mutation = new GeneticAlgorithm<AngryBirdsGen>.GAMutation(Mutate);
		_geneticAlgorithm.Crossover = new GeneticAlgorithm<AngryBirdsGen>.GACrossover(Crossover);
		_geneticAlgorithm.FitnessFunction = new GeneticAlgorithm<AngryBirdsGen>.GAFitnessFunction(EvaluateUsingAI);	
		_geneticAlgorithm.StartEvolution();

		_isRankingGenome = false;
        _isRankingWithRegression = false;
		_generationIdx = 0;
		_genomeIdx = 0;

        //Debug.Log("Pop size: "+ _geneticAlgorithm.PopulationSize);
        //Debug.Log("Gen size: "+ _geneticAlgorithm.Generations);
		// Set time scale to acelerate evolution
		Time.timeScale = 1f;
		 
		// Totally zoom out
		GameWorld.Instance._camera.SetCameraWidth(Mathf.Infinity);
		
		// Remove all objects from level before start
		GameWorld.Instance.ClearWorld();
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

		if(!_isRankingGenome)
		{
			float fitness = Mathf.Infinity;
			_geneticAlgorithm.GetNthGenome(_genomeIdx, out _lastgenome, out fitness);

			_fitnessEvaluation++;

			if(!_fitnessCache.ContainsKey(_lastgenome))
			{

                /**Classifier tested here*/
                //CSVManager.SaveCSVLevel(_lastgenome.level, "Assets/Resources/GeneratedLevels/CSV_Test/levels.csv");
                if (useClassifier)
                {
                    if (_dataMiningManager.EvaluateUsingClassifier(_lastgenome.level, cl) == 0)
                    {
                        EndEvaluatingGenome(100);
                        _fitnessNotEvaluated++;
                        //Debug.Log("Not Evaluated: " + _fitnessNotEvaluated);
                    }
                    else
                    {
                        GameWorld.Instance.ClearWorld();
                        StartEvaluatingGenome();
                    }
                }
                else if(useRegression)
                {
                    float _lFitness = (float)_dataMiningManager.EvaluateUsingRegression(_lastgenome.level, cl);
                    Debug.Log("Fitness:" + _lFitness);
                    EndEvaluatingGenome(_lFitness);
                }
                else
                {
                    GameWorld.Instance.ClearWorld();
                    StartEvaluatingGenome();
                }
            }
            else
			{
			 	_genomeIdx++;
			 	_isRankingGenome = false;
			 	_fitnessRecovered++;
			}
		}
		else if(GameWorld.Instance.IsLevelStable() && 
		       (GameWorld.Instance.GetPigsAvailableAmount()  == 0 || 
				GameWorld.Instance.GetBirdsAvailableAmount() == 0 ))
		{
            
            EndEvaluatingGenome();
            
            GameWorld.Instance.ClearWorld();
			
			_isRankingGenome = false;
            if (isEnding)
                EndEvolution();
		}

		if(_genomeIdx == _geneticAlgorithm.PopulationSize)
		{			
			_geneticAlgorithm.RankPopulation();
			
			float bestFitness = CheckStopCriteria();

            SaveGenerationLog();

			//Debug.Log ("best = " + bestFitness);
            bestFitnesses.Add(bestFitness);

			if(_generationIdx < _geneticAlgorithm.Generations && _sameBestFitnessCount < 10 && bestFitness > 0f)
				_geneticAlgorithm.CreateNextGeneration();
			else
            {
                //If using regression evaluates the best individual to compare with the model's answer to check quality of regression model
                if (useRegression)
                {
                    float fitness = Mathf.Infinity;
                    AngryBirdsGen genome = new AngryBirdsGen();
                    _geneticAlgorithm.GetBest(out genome, out fitness);
                    StartEvaluatingGenome(ref genome);
                    isEnding = true;
                }
                else
                    EndEvolution();
            }


            _genomeIdx = 0;
			_generationIdx++;
		}
	}
	
    /**
     *  Starts evaluating a genome by first converting the level of the last genome to a 
     *  Playable level, and then decoding it as a game world level. Also sets is ranking genome to true.
     */
	private void StartEvaluatingGenome(ref AngryBirdsGen genome)
	{
        Debug.Log("Comparing");
        ConvertShiftGBtoABGB(ref genome.level);
		GameWorld.Instance.DecodeLevel(genome.level.gameObjects, _lastgenome.level.birdsAmount);
		_isRankingGenome = true;
        _isRankingWithRegression = true;
	}
    private void StartEvaluatingGenome()
    {
        //Debug.Log("Is Updating");
        ConvertShiftGBtoABGB(ref _lastgenome.level);
        GameWorld.Instance.DecodeLevel(_lastgenome.level.gameObjects, _lastgenome.level.birdsAmount);
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
        if(saveAllXML)
            LevelLoader.SaveXmlLevel(_lastgenome.level);
        if(saveAllCSV)
            CSVManager.SaveCSVLevel(_lastgenome.level, pk, fitness, "Assets/Resources/GeneratedLevels/CSV/levels.csv");
        //If using regression model, plays the level with best fitness in generation to compare with model's answer
        if (_isRankingWithRegression)
        {
            Debug.Log("IsCalculatingFitness");
            fitnessToCompare.Add(fitness);
            _isRankingWithRegression = false;
        }
        else
            _genomeIdx++;
	}

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
		float distAmountBlocks = Mathf.Abs((Mathf.Ceil(d*_blocksMaxAmount) - li));
		
		return distBirds + distAmountBlocks + (sk + pk);
	}

    /**
     *  Evaluates using the fitness contained at the cache
     *  @param[in]  genome      Genome to be evaluated.
     *  @param[in]  genomeIdx   Index of genome to be evaluated.
     *  @return     float       Fitness contained at the cache for the genome.
     */
	public float EvaluateUsingAI(AngryBirdsGen genome, int genomeIdx)
	{
		//Debug.Log ("fitness = " + _fitnessCache[genome]);

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
        Debug.Log("Ending Evolution");
		_experimentsIdx++;
		
		// Save the results	
		SaveLog();
        // Save results
        WriteLogToFile(_logFile, _logContent);

        float fitness = Mathf.Infinity;

        AngryBirdsGen genome = new AngryBirdsGen();
        _geneticAlgorithm.GetBest(out genome, out fitness);

        // Clear the level and decode the best genome of the last generation
        GameWorld.Instance.DecodeLevel(genome.level.gameObjects, genome.level.birdsAmount);

        // Save file in xml for future use
        LevelLoader.SaveXmlLevel(genome.level, useClassifier, useRegression, createPrePopulation);

        if (_experimentsIdx < _experimentsAmount)
		{
			// Run next experiment
			Application.LoadLevel(Application.loadedLevel);
		}
		else
		{		

			// Default time scale
			Time.timeScale = 1f;
		
			// Play level starting audio
			GameWorld.Instance.GetComponent<AudioSource>().PlayOneShot(GameWorld.Instance._clips[0]);
			GameWorld.Instance.GetComponent<AudioSource>().PlayOneShot(GameWorld.Instance._clips[1]);

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

		if(_lastGenerationBestFitness == fitness)
			
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
	                      out Genome<AngryBirdsGen> child1,  out Genome<AngryBirdsGen> child2) {

		child1 = new Genome<AngryBirdsGen>();
		child2 = new Genome<AngryBirdsGen>();

		AngryBirdsGen genes1 = new AngryBirdsGen();
		AngryBirdsGen genes2 = new AngryBirdsGen();
		
		if(UnityEngine.Random.value <= _geneticAlgorithm.CrossoverRate)
		{	
			int maxGenomeSize = Mathf.Max (genome1.Genes.level.GetStacksAmount(), 
			                               genome2.Genes.level.GetStacksAmount());
			
			for(int i = 0; i < maxGenomeSize; i++)
			{	
				if(genome1.Genes.level.GetStacksAmount() == genome2.Genes.level.GetStacksAmount())
				{				
					if(UnityEngine.Random.value < 0.5f)
							genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
					else
							genes1.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));

					if(UnityEngine.Random.value < 0.5f)
							genes2.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
					else
							genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
				}
				else if(genome1.Genes.level.GetStacksAmount() < genome2.Genes.level.GetStacksAmount())
				{
					if(i < genome1.Genes.level.GetStacksAmount())
					{					
						if(UnityEngine.Random.value < 0.5f)
								genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
						else
								genes1.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));

				
						if(UnityEngine.Random.value < 0.5f)
								genes2.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
						else
								genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
					}
					else
					{					
						if(UnityEngine.Random.value < 0.5f)
								genes1.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
					
						if(UnityEngine.Random.value < 0.5f)
								genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
					}
				}
				else
				{
					if(i < genome2.Genes.level.GetStacksAmount())
					{	
						if(UnityEngine.Random.value < 0.5f)
								genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
						else
								genes1.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));

						if(UnityEngine.Random.value < 0.5f)
								genes2.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
						else
								genes2.level.AddStack(CopyStack(genome2.Genes.level.GetStack(i)));
					}
					else
					{				
						if(UnityEngine.Random.value < 0.5f)
								genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
				
						if(UnityEngine.Random.value < 0.5f)
								genes2.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
					}
				}
			}
		
			// Integer crossover for birds
			genes1.level.birdsAmount = (int)(0.5f * genome1.Genes.level.birdsAmount + 0.5f * genome2.Genes.level.birdsAmount);
			genes2.level.birdsAmount = (int)(1.5f * genome1.Genes.level.birdsAmount - 0.5f * genome2.Genes.level.birdsAmount);
		}
		else
		{
			for(int i = 0; i < genome1.Genes.level.GetStacksAmount(); i++)
			{	
				genes1.level.AddStack(CopyStack(genome1.Genes.level.GetStack(i)));
			}
			
			genes1.level.birdsAmount = genome1.Genes.level.birdsAmount;
			
			for(int i = 0; i < genome2.Genes.level.GetStacksAmount(); i++)
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
	public void Mutate(ref Genome<AngryBirdsGen> genome) {

		for(int i = 0; i < genome.Genes.level.GetStacksAmount(); i++)
		{
			if(UnityEngine.Random.value <= _geneticAlgorithm.MutationRate)
			{
				genome.Genes.level.GetStack(i).Clear();
				genome.Genes.level.SetStack(i, new LinkedList<ShiftABGameObject>());
				
				// Generate new stacks
				genome.Genes.level.SetStack(i, GenerateStack(genome.Genes.level.LevelPlayableHeight, 
				              								 genome.Genes.level.LevelPlayableWidth, 
				              								 genome.Genes.level.WidthOfEmptyStack));
			}
		}
		
		if(UnityEngine.Random.value <= _geneticAlgorithm.MutationRate)
			genome.Genes.level.birdsAmount = UnityEngine.Random.Range(0, ABLevel.BIRDS_MAX_AMOUNT);
		
		genome.Genes.level.FixLevelSize();
	}

    /**
     *  Initializes a genome, setting its bird amount randomly and generating a random level.
     *  @param[out] genome  Genome to be generated randomly
     */
	public void InitAngryBirdsGenome(out AngryBirdsGen genome) {

		genome = new AngryBirdsGen();

		genome.level.birdsAmount = UnityEngine.Random.Range(0, ABLevel.BIRDS_MAX_AMOUNT);
		genome.level = GenerateRandomLevel();
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
		
		_genLog   += fitness + " ";
		_cacheLog += _fitnessCache.Count + " ";
		_recoverLog += _fitnessRecovered + " ";
		
		//Debug.Log("Best Fitness: " + fitness);
		//Debug.Log("Cache Size: " + _fitnessCache.Count);
		//Debug.Log("Cache Usage: " + _fitnessRecovered);
	}
	
    /**
     *  Saves the log for the experiment, writing the experiment time, the convergence, cache size,
     *  number of times fitness was calculated and times it was recovered, best fitness, best level linearity
     *  and density, best level frequency of pigs and birds, fitness, cache size and usage evolution.
     *  also updates the time of the last experiment.
     */
	private void SaveLog()
	{
		float fitness = Mathf.Infinity;
		AngryBirdsGen genome = new AngryBirdsGen();
		_geneticAlgorithm.GetBest(out genome, out fitness);
		
		float experimentTime = Time.realtimeSinceStartup - _lastExperimentTime;
        Debug.Log("Size:" + fitnessToCompare.Count);
		_logContent += "====== RESULTS ======\n";
		_logContent += "Execution time: "        + experimentTime + "\n";
		_logContent += "Convergence: "           + _generationIdx + "\n";
		_logContent += "Cache size:"             + _fitnessCache.Count + "\n";
		_logContent += "Fitness calculations: "  + _fitnessEvaluation + "\n";
        _logContent += "Fitness skipped: "       + _fitnessNotEvaluated + "\n";
        _logContent += "Fitness recovered: "     + _fitnessRecovered + "\n";
		_logContent += "Best Fitness: "          + fitness + "\n";
		_logContent += "Linearity: "             + genome.level.GetLevelLinearity() + "\n";
		_logContent += "Density: "               + genome.level.GetLevelDensity() + "\n";
		_logContent += "Frequency pig: "         + genome.level.GetABGameObjectFrequency(GameWorld.Instance.Templates.Length) + "\n";;	
		_logContent += "Frequency bird: "        + genome.level.GetBirdsFrequency() + "\n";
		_logContent += "Fitness Evolution: "     + _genLog + "\n";
		_logContent += "Cache Size Evolution: "  + _cacheLog + "\n";
		_logContent += "Cache Usage Evolution: " + _recoverLog + "\n";
        _logContent += "Best Fitness per generation: \n";
        _logContent += "Index Fitness\n";

        _logContent += fitnessToCompare[0];

        int index = 0;
        foreach (var _fitness in bestFitnesses)
        {
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
		WriteLogToFile(_logFile, _logContent);
	}

    
}
