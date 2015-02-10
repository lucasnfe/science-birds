using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AngryBirdsGen 
{
	public int birdsAmount;
	public List<LinkedList<ShiftABGameObject>> gameObjects;

	public AngryBirdsGen()
	{
		gameObjects = new List<LinkedList<ShiftABGameObject>>();
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;

			// get hash code for the birds amount
			hash = hash * 23 + birdsAmount.GetHashCode();

			// get hash code for all items in array
			foreach (var stack in gameObjects)
			{
				foreach(var block in stack)
				{
					hash = hash * 23 + ((block != null) ? block.Label.GetHashCode() : 0);
					hash = hash * 23 + ((block != null) ? block.IsDouble.GetHashCode() : 0);
				}
			}
			
			return hash;
		}
	}
}

public class GeneticLG : RandomLG 
{	
	private int _generationIdx;
	private int _genomeIdx;

	private bool _isRankingGenome;

	// Fitness function variables
	private float _pk, _pi, _lk, _li, _bi, _bk;

	public int _populationSize, _generationSize;
	public float _mutationRate, _crossoverRate;
	public bool _elitism;

	private List<float> _fitnessTable;
	private Hashtable _fitnessCache;

	private AngryBirdsGen _lastgenome;

	GeneticAlgorithm<AngryBirdsGen> _geneticAlgorithm;

	public override ABLevel GenerateLevel()
	{
		return new ABLevel();
	}
	
	public override void Start()
	{
		base.Start();

		_fitnessTable = new List<float>();
		_fitnessCache = new Hashtable();

		GameWorld.Instance.ClearWorld();

		// Generate a population of feaseble levels evaluated by an inteligent agent
		_geneticAlgorithm = new GeneticAlgorithm<AngryBirdsGen>(_crossoverRate, _mutationRate, _populationSize, _generationSize, _elitism);

		_geneticAlgorithm.InitGenome = new GeneticAlgorithm<AngryBirdsGen>.GAInitGenome(InitAngryBirdsGenome);
		_geneticAlgorithm.Mutation = new GeneticAlgorithm<AngryBirdsGen>.GAMutation(Mutate);
		_geneticAlgorithm.Crossover = new GeneticAlgorithm<AngryBirdsGen>.GACrossover(Crossover);
		_geneticAlgorithm.FitnessFunction = new GeneticAlgorithm<AngryBirdsGen>.GAFitnessFunction(EvaluateUsingAI);
		
		_geneticAlgorithm.StartEvolution();

		_generationIdx = 0;
		_genomeIdx = 0;

		_isRankingGenome = false;

		// Set time scale to acelerate evolution
		Time.timeScale = 100f;
	}

	void Update()
	{
		if(!_isRankingGenome)
		{
			double fitness = 0f;
			_lastgenome = new AngryBirdsGen();
			_geneticAlgorithm.GetNthGenome(_genomeIdx, out _lastgenome, out fitness);

			if(!_fitnessCache.ContainsKey(_lastgenome))
			{
				StartEvaluatingGenome();
			}
			else
			{
				_fitnessTable.Add((float)_fitnessCache[_lastgenome]);

				_genomeIdx++;
				_isRankingGenome = false;
			}
		}
	
		if(_isRankingGenome && GameWorld.Instance.IsLevelStable() && 
		   (GameWorld.Instance.GetBirdsAvailableAmount() == 0 || 
		    GameWorld.Instance.GetPigsAvailableAmount() == 0))
		{
			_bk = GameWorld.Instance.GetBirdsAvailableAmount();
			_pk = GameWorld.Instance.GetPigsAvailableAmount();
			_lk = GameWorld.Instance.GetBlocksAvailableAmount();

			float fitness = Fitness((int)_pk, (int)_pi, (int)_li, (int)_lk,(int)_bi, (int)_bk);

			_fitnessTable.Add(fitness);
			_fitnessCache.Add(_lastgenome, fitness);

			GameWorld.Instance.ClearWorld();

			_genomeIdx++;
			_isRankingGenome = false;
		}

		if(_genomeIdx == _geneticAlgorithm.PopulationSize)
		{
			Debug.Log("====== GENERATION " + _generationIdx +  " ======");
			
			_geneticAlgorithm.RankPopulation();
			_fitnessTable.Clear();
			
			AngryBirdsGen genome = GetCurrentBest();
			
			if(_generationIdx < _geneticAlgorithm.Generations)
			{
				_geneticAlgorithm.CreateNextGeneration();
			}
			else
			{
				Debug.Log("====== END EVOLUTION ======");
				Time.timeScale = 1f;
				
				// Clear the level and decode the best genome of the last generation
				GameWorld.Instance.ClearWorld();			
				DecodeLevel(ConvertShiftGBtoABGB(genome.gameObjects), genome.birdsAmount);				
				
				// Disable AI and allow player to test the level
				GameWorld.Instance._birdAgent.gameObject.SetActive(false);

				// Disable simulation
				GameWorld.Instance._isSimulation = false;

				Destroy(this.gameObject);
			}
			
			_genomeIdx = 0;
			_generationIdx++;
		}
	}

	private float Fitness(int pk, int pi, int li, int lk, int bi, int bk)
	{
		float fitness;
		
		if(pk != 0)
			
			fitness = 1f/pk;
		else
			fitness = (bi - bk) + (li - lk);
		
		return fitness;
	}

	private void StartEvaluatingGenome()
	{
		DecodeLevel(ConvertShiftGBtoABGB(_lastgenome.gameObjects), _lastgenome.birdsAmount);

		_bi = GameWorld.Instance.GetBirdsAvailableAmount();
		_pi = GameWorld.Instance.GetPigsAvailableAmount();
		_li = GameWorld.Instance.GetBlocksAvailableAmount();

		_isRankingGenome = true;
	}

	public double EvaluateUsingAI(AngryBirdsGen genome, int genomeIdx)
	{
		return _fitnessTable[genomeIdx];
	}

	public void Crossover(ref Genome<AngryBirdsGen> genome1, ref Genome<AngryBirdsGen> genome2, 
	                      out Genome<AngryBirdsGen> child1,  out Genome<AngryBirdsGen> child2) {

		int maxGenomeSize = Mathf.Max (genome1.Genes.gameObjects.Count, 
		                               genome2.Genes.gameObjects.Count);

		child1 = new Genome<AngryBirdsGen>();
		child2 = new Genome<AngryBirdsGen>();

		AngryBirdsGen genes1 = new AngryBirdsGen();
		AngryBirdsGen genes2 = new AngryBirdsGen();
	
		for(int i = 0; i < maxGenomeSize; i++)
		{
			if(genome1.Genes.gameObjects.Count == genome2.Genes.gameObjects.Count)
			{
				if(UnityEngine.Random.value < 0.5f)

					genes1.gameObjects.Add(CopyStack(genome1.Genes.gameObjects[i]));
				else
					genes1.gameObjects.Add(CopyStack(genome2.Genes.gameObjects[i]));

				if(UnityEngine.Random.value < 0.5f)

					genes2.gameObjects.Add(CopyStack(genome1.Genes.gameObjects[i]));
				else
					genes2.gameObjects.Add(CopyStack(genome2.Genes.gameObjects[i]));
			}
			else if(genome1.Genes.gameObjects.Count < genome2.Genes.gameObjects.Count)
			{
				if(i < genome1.Genes.gameObjects.Count)
				{
					if(UnityEngine.Random.value < 0.5f)

						genes1.gameObjects.Add(CopyStack(genome1.Genes.gameObjects[i]));
					else
						genes1.gameObjects.Add(CopyStack(genome2.Genes.gameObjects[i]));

					if(UnityEngine.Random.value < 0.5f)

						genes2.gameObjects.Add(CopyStack(genome1.Genes.gameObjects[i]));
					else
						genes2.gameObjects.Add(CopyStack(genome2.Genes.gameObjects[i]));
				}
				else
				{
					if(UnityEngine.Random.value < 0.5f)

						genes1.gameObjects.Add(CopyStack(genome2.Genes.gameObjects[i]));
					else
						genes1.gameObjects.Add(new LinkedList<ShiftABGameObject>());
					
					if(UnityEngine.Random.value < 0.5f)

						genes2.gameObjects.Add(CopyStack(genome2.Genes.gameObjects[i]));
					else
						genes2.gameObjects.Add(new LinkedList<ShiftABGameObject>());
				}
			}
			else
			{
				if(i < genome2.Genes.gameObjects.Count)
				{
					if(UnityEngine.Random.value < 0.5f)

						genes1.gameObjects.Add(CopyStack(genome1.Genes.gameObjects[i]));
					else
						genes1.gameObjects.Add(CopyStack(genome2.Genes.gameObjects[i]));

					if(UnityEngine.Random.value < 0.5f)

						genes2.gameObjects.Add(CopyStack(genome1.Genes.gameObjects[i]));
					else
						genes2.gameObjects.Add(CopyStack(genome2.Genes.gameObjects[i]));
				}
				else
				{
					if(UnityEngine.Random.value < 0.5f)

						genes1.gameObjects.Add(CopyStack(genome1.Genes.gameObjects[i]));
					else
						genes1.gameObjects.Add(new LinkedList<ShiftABGameObject>());
					
					if(UnityEngine.Random.value < 0.5f)

						genes2.gameObjects.Add(CopyStack(genome1.Genes.gameObjects[i]));
					else
						genes2.gameObjects.Add(new LinkedList<ShiftABGameObject>());
				}
			}
		}

		// Integer crossover for birds
		genes1.birdsAmount = (int)(0.5f * genome1.Genes.birdsAmount + 0.5f * genome2.Genes.birdsAmount);
		genes2.birdsAmount = (int)(1.5f * genome1.Genes.birdsAmount - 0.5f * genome2.Genes.birdsAmount);

		child1.Genes = genes1;
		child2.Genes = genes2;
	}
	
	public void Mutate(ref Genome<AngryBirdsGen> genome) {

		List<LinkedList<ShiftABGameObject>> gameObjects = genome.Genes.gameObjects;

		for(int i = 0; i < genome.Genes.gameObjects.Count; i++)
		{
			if(UnityEngine.Random.value <= _geneticAlgorithm.MutationRate)
			{
				gameObjects[i] = new LinkedList<ShiftABGameObject>();
				GenerateNextStack(i, ref gameObjects);
				InsertPigs(i, ref gameObjects);
				genome.Genes.gameObjects[i] = gameObjects[i];
			}
		}
	}

	public void InitAngryBirdsGenome(out AngryBirdsGen genome) {

		genome = new AngryBirdsGen();

		genome.birdsAmount = UnityEngine.Random.Range(0, _birdsMaxAmount);
		genome.gameObjects = GenerateRandomLevel();
	}

	private AngryBirdsGen GetCurrentBest() {

		double fitness = 0f;
		AngryBirdsGen genome = new AngryBirdsGen();
		_geneticAlgorithm.GetBest(out genome, out fitness);

		UnityEngine.Debug.Log ("Best fitness " + fitness);

		return genome;
	}
}
