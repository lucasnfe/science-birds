using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class GeneticLG : RandomLG 
{	
	// Experiments variables
	private int _fitnessEvaluation;
	private int _fitnessRecovered;
	
	private static int _experimentsIdx;
	
	// Control variables
	private int _genomeIdx, _generationIdx;
	private int _sameBestFitnessCount;
	private bool _isRankingGenome;
	private float _lastGenerationBestFitness;

	private const string _logFile = "Assets/Experiments/generations.txt";
	private const int _blocksMaxAmount = 100;
	private const int _averagingAmount = 1;
	
	private static float _lastExperimentTime;
	private static string _logContent, _genLog;
	private static string _cacheLog, _recoverLog;
	
	private GeneticAlgorithm<AngryBirdsGen> _geneticAlgorithm;
	
	// Hash table used to cache fitness calculation
	private Dictionary<AngryBirdsGen, float> _fitnessCache;
	private AngryBirdsGen _lastgenome;
	
	// Fitness function parameters
	public float _bn, _d;
	
	// Genetic Algorithm parameters
	public int _populationSize, _generationSize;
	public float _mutationRate, _crossoverRate;
	public bool _elitism;
	
	public int _experimentsAmount = 1;
		
	public void Start()
	{
		Debug.Log("Experiment n: " + _experimentsIdx);
		
		_fitnessCache = new Dictionary<AngryBirdsGen, float>();

		// Generate a population of feaseble levels evaluated by an inteligent agent
		_geneticAlgorithm = new GeneticAlgorithm<AngryBirdsGen>(_crossoverRate, _mutationRate, _populationSize, _generationSize, _elitism);
		_geneticAlgorithm.InitGenome = new GeneticAlgorithm<AngryBirdsGen>.GAInitGenome(InitAngryBirdsGenome);
		_geneticAlgorithm.Mutation = new GeneticAlgorithm<AngryBirdsGen>.GAMutation(Mutate);
		_geneticAlgorithm.Crossover = new GeneticAlgorithm<AngryBirdsGen>.GACrossover(Crossover);
		_geneticAlgorithm.FitnessFunction = new GeneticAlgorithm<AngryBirdsGen>.GAFitnessFunction(EvaluateUsingAI);	
		_geneticAlgorithm.StartEvolution();

		_isRankingGenome = false;
		_generationIdx = 0;
		_genomeIdx = 0;

		// Set time scale to acelerate evolution
		Time.timeScale = 1f;
		 
		// Totally zoom out
		GameWorld.Instance._camera.SetCameraWidth(Mathf.Infinity);
		
		// Remove all objects from level before start
		GameWorld.Instance.ClearWorld();
	}
	
	public override ABLevel GenerateLevel()
	{
		return new ABLevel();
	}

	void Update()
	{
		if(!_isRankingGenome)
		{
			float fitness = Mathf.Infinity;
			_geneticAlgorithm.GetNthGenome(_genomeIdx, out _lastgenome, out fitness);

			_fitnessEvaluation++;

			if(!_fitnessCache.ContainsKey(_lastgenome))
			{
				GameWorld.Instance.ClearWorld();
				StartEvaluatingGenome();
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
		}

		if(_genomeIdx == _geneticAlgorithm.PopulationSize)
		{			
			_geneticAlgorithm.RankPopulation();
			
			float bestFitness = CheckStopCriteria();
			SaveGenerationLog();

			Debug.Log ("best = " + bestFitness);

			if(_generationIdx < _geneticAlgorithm.Generations && _sameBestFitnessCount < 10 && bestFitness > 0f)
				_geneticAlgorithm.CreateNextGeneration();
			else
				EndEvolution();
			
			_genomeIdx = 0;
			_generationIdx++;
		}
	}
	
	private void StartEvaluatingGenome()
	{
		ConvertShiftGBtoABGB(ref _lastgenome.level);
		GameWorld.Instance.DecodeLevel(_lastgenome.level.gameObjects, _lastgenome.level.birdsAmount);
		_isRankingGenome = true;
	}
	
	private void EndEvaluatingGenome()
	{
		float bi = GameWorld.Instance.BirdsAtStart;
		float pi = GameWorld.Instance.PigsAtStart;
		float li = GameWorld.Instance.BlocksAtStart;
 
		float bk = GameWorld.Instance.GetBirdsAvailableAmount();
		float pk = GameWorld.Instance.GetPigsAvailableAmount();
		float lk = GameWorld.Instance.GetBlocksAvailableAmount();
		float sk = GameWorld.Instance.StabilityUntilFirstBird;
				
		_fitnessCache[_lastgenome] = Fitness(pk, pi, li, lk, bi, bk, _d, sk);		
		_genomeIdx++;
	}
	
	private float Fitness(float pk, float pi, float li, float lk, float bi, float bk, float d, float sk)
	{					
		float distBirds = Mathf.Abs(Mathf.Ceil(_bn * bi) - (bi - bk));
		float distAmountBlocks = Mathf.Abs((Mathf.Ceil(d*_blocksMaxAmount) - li));
		
		return distBirds + distAmountBlocks + (sk + pk);
	}

	public float EvaluateUsingAI(AngryBirdsGen genome, int genomeIdx)
	{
		Debug.Log ("fitness = " + _fitnessCache[genome]);

		return _fitnessCache[genome];
	}
	
	private void EndEvolution()
	{
		_experimentsIdx++;
		
		// Save the results	
		SaveLog();
		
		if(_experimentsIdx < _experimentsAmount)
		{
			// Run next experiment
			Application.LoadLevel(Application.loadedLevel);
		}
		else
		{		
			float fitness = Mathf.Infinity;
			AngryBirdsGen genome = new AngryBirdsGen();
			_geneticAlgorithm.GetBest(out genome, out fitness);

			// Save results
			WriteLogToFile(_logFile, _logContent);
		
			// Default time scale
			Time.timeScale = 1f;
		
			// Play level starting audio
			GameWorld.Instance.GetComponent<AudioSource>().PlayOneShot(GameWorld.Instance._clips[0]);
			GameWorld.Instance.GetComponent<AudioSource>().PlayOneShot(GameWorld.Instance._clips[1]);
		
			// Clear the level and decode the best genome of the last generation
			GameWorld.Instance.DecodeLevel(genome.level.gameObjects, genome.level.birdsAmount);	

			// Save file in xml for future use
			LevelLoader.SaveXmlLevel(genome.level);

			// Disable simulation
			GameWorld.Instance._isSimulation = false;
							
			// Disable AI and allow player to test the level
			Destroy(GameWorld.Instance._birdAgent.gameObject);
		
			// Destroy the generator
			Destroy(this.gameObject);
		}
	}
	
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

	public void InitAngryBirdsGenome(out AngryBirdsGen genome) {

		genome = new AngryBirdsGen();

		genome.level.birdsAmount = UnityEngine.Random.Range(0, ABLevel.BIRDS_MAX_AMOUNT);
		genome.level = GenerateRandomLevel();
	}
	
	private void SaveGenerationLog()
	{
		float fitness = Mathf.Infinity;
		AngryBirdsGen genome = new AngryBirdsGen();
		_geneticAlgorithm.GetBest(out genome, out fitness);
		
		_genLog   += fitness + " ";
		_cacheLog += _fitnessCache.Count + " ";
		_recoverLog += _fitnessRecovered + " ";
		
		Debug.Log("Best Fitness: " + fitness);
		Debug.Log("Cache Size: " + _fitnessCache.Count);
		Debug.Log("Cache Usage: " + _fitnessRecovered);
	}
	
	private void SaveLog()
	{
		float fitness = Mathf.Infinity;
		AngryBirdsGen genome = new AngryBirdsGen();
		_geneticAlgorithm.GetBest(out genome, out fitness);
		
		float experimentTime = Time.realtimeSinceStartup - _lastExperimentTime;
		
		_logContent += "====== RESULTS ======\n";
		_logContent += "Execution time: "        + experimentTime + "\n";
		_logContent += "Convergence: "           + _generationIdx + "\n";
		_logContent += "Cache size:"             + _fitnessCache.Count + "\n";
		_logContent += "Fitness calculations: "  + _fitnessEvaluation + "\n";
		_logContent += "Fitness recovered: "     + _fitnessRecovered + "\n";
		_logContent += "Best Fitness: "          + fitness + "\n";
		_logContent += "Linearity: "             + genome.level.GetLevelLinearity() + "\n";
		_logContent += "Density: "               + genome.level.GetLevelDensity() + "\n";
		_logContent += "Frequency pig: "         + genome.level.GetABGameObjectFrequency(GameWorld.Instance.Templates.Length) + "\n";;	
		_logContent += "Frequency bird: "        + genome.level.GetBirdsFrequency() + "\n";
		_logContent += "Fitness Evolution: "     + _genLog + "\n";
		_logContent += "Cache Size Evolution: "  + _cacheLog + "\n";
		_logContent += "Cache Usage Evolution: " + _recoverLog + "\n";
		
		_genLog = "";
		_cacheLog = "";
		_recoverLog = "";
		
		_lastExperimentTime = Time.realtimeSinceStartup;		
	}
	
	private void WriteLogToFile(string filename, string content)
	{
		StreamWriter writer = new StreamWriter(filename);
		writer.WriteLine(content);
		writer.Close();
	}
	
	private void OnApplicationQuit() 
	{
		WriteLogToFile(_logFile, _logContent);
	}
}
