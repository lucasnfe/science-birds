using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct AngryBirdsGen 
{
	public int birdsAmount;
	public List<LinkedList<ShiftABGameObject>> gameObjects;

	public AngryBirdsGen(int genomeSize)
	{
		birdsAmount = 0;

		gameObjects = new List<LinkedList<ShiftABGameObject>>();
		
		for(int i = 0; i < genomeSize; i++)
		{
			LinkedList<ShiftABGameObject> newStack = new LinkedList<ShiftABGameObject>();
			gameObjects.Add(newStack);
		}
	}
}

public class GeneticLG : RandomLG 
{	
	private int  _generationIdx;
	private int  _genomeIdx;
	private bool _isRankingGenome;

	private List<double> _fitnessTable;

	GeneticAlgorithm<AngryBirdsGen> _geneticAlgorithm;

	public override List<ABGameObject> GenerateLevel()
	{
		List<ABGameObject> gameObjects = new List<ABGameObject>();
		
		return gameObjects;
	}

	public override void Start()
	{
		base.Start();

		_fitnessTable = new List<double>();

		// Generate a population of feaseble levels evaluated by an inteligent agent
		_geneticAlgorithm = new GeneticAlgorithm<AngryBirdsGen>(0.8f, 0.05f, 2, 1, false);
		
		_geneticAlgorithm.InitGenome = new GeneticAlgorithm<AngryBirdsGen>.GAInitGenome(InitAngryBirdsGenome);
		_geneticAlgorithm.Mutation = new GeneticAlgorithm<AngryBirdsGen>.GAMutation(Mutate);
		_geneticAlgorithm.Crossover = new GeneticAlgorithm<AngryBirdsGen>.GACrossover(Crossover);
		_geneticAlgorithm.FitnessFunction = new GeneticAlgorithm<AngryBirdsGen>.GAFitnessFunction(EvaluateUsingAI);
		
		_geneticAlgorithm.StartEvolution();
	}

	void Update()
	{
		if(_generationIdx == 5)
		{
			Debug.Log("Acabou!");
			return;
		}

		if(!_isRankingGenome)
		{
			Debug.Log ("ranking genome " + _genomeIdx);

			double fitness = 0f;
			AngryBirdsGen genome = new AngryBirdsGen();
			_geneticAlgorithm.GetNthGenome(_genomeIdx, out genome, out fitness);

			StartEvaluatingGenome(genome);
		}
	
		if(_isRankingGenome && (GameWorld.Instance.GetBirdsAvailableAmount() == 0 || GameWorld.Instance.GetPigsAvailableAmount() == 0))
		{
			_genomeIdx++;
			_isRankingGenome = false;

			_fitnessTable.Add(0f);
		}

		if(_genomeIdx == _geneticAlgorithm.PopulationSize)
		{
			_genomeIdx = 0;
			_generationIdx++;

			_geneticAlgorithm.RankPopulation();
			_geneticAlgorithm.CreateNextGeneration();

			_fitnessTable.Clear();
		}
	}

	private void StartEvaluatingGenome(AngryBirdsGen genome)
	{
		GameWorld.Instance.ClearWorld();
		DecodeLevel(ConvertShiftGBtoABGB(genome.gameObjects), genome.birdsAmount);
		
		_isRankingGenome = true;
	}

	public double EvaluateUsingAI(AngryBirdsGen genome, int genomeIdx)
	{
		return _fitnessTable[genomeIdx];
	}

	public void Crossover(ref Genome<AngryBirdsGen> genome1, ref Genome<AngryBirdsGen> genome2, 
	                      out Genome<AngryBirdsGen> child1,  out Genome<AngryBirdsGen> child2) {

		int minGenomeSize = Mathf.Min (genome1.Genes.gameObjects.Count, genome2.Genes.gameObjects.Count);
		int pos = (int)(UnityEngine.Random.value * (float)minGenomeSize);

		child1 = new Genome<AngryBirdsGen>();
		child2 = new Genome<AngryBirdsGen>();

		AngryBirdsGen genes1 = new AngryBirdsGen(minGenomeSize);
		AngryBirdsGen genes2 = new AngryBirdsGen(minGenomeSize);

		child1.Genes = genes1;
		child2.Genes = genes2;

		Debug.Log(child1.Genes.gameObjects.Count);
		
		for(int i = 0; i < minGenomeSize; i++)
		{
			if (i < pos)
			{
				child1.Genes.gameObjects[i] = genome1.Genes.gameObjects[i];
				child2.Genes.gameObjects[i] = genome2.Genes.gameObjects[i];
			}
			else
			{
				child1.Genes.gameObjects[i] = genome2.Genes.gameObjects[i];
				child2.Genes.gameObjects[i] = genome1.Genes.gameObjects[i];
			}
		}
	}
	
	public void Mutate(ref Genome<AngryBirdsGen> genome) {

		// TODO: Implement mutation operation
	}

	public void InitAngryBirdsGenome(out AngryBirdsGen genome) {

		genome.birdsAmount = DefineBirdsAmount();
		genome.gameObjects = GenerateRandomLevel();
	}
}
