using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct AngryBirdsGen {

	public int birdsAmount;
	public List<ABGameObject> gameObjects;
}

public class GeneticLG : LevelGenerator {

	public override int DefineBirdsAmount() {

		return 0;
	}

	public override List<ABGameObject> GenerateLevel() {

		List<ABGameObject> gameObjects = new List<ABGameObject>();

		// Generate a population of feaseble levels evaluated by an inteligent agent
		GeneticAlgorithm<AngryBirdsGen> geneticAlgorithm = new GeneticAlgorithm<AngryBirdsGen>(0.8f, 0.05f, 2, 10, true);

		geneticAlgorithm.InitGenome = new GeneticAlgorithm<AngryBirdsGen>.GAInitGenome(InitAngryBirdsGenome);
		geneticAlgorithm.Mutation = new GeneticAlgorithm<AngryBirdsGen>.GAMutation(Mutate);
		geneticAlgorithm.Crossover = new GeneticAlgorithm<AngryBirdsGen>.GACrossover(Crossover);
		geneticAlgorithm.FitnessFunction = new GeneticAlgorithm<AngryBirdsGen>.GAFitnessFunction(EvaluateUsingAI);

		geneticAlgorithm.StartEvolution();
		
		return gameObjects;
	}

	public static double EvaluateUsingAI(AngryBirdsGen genome)
	{
		// TODO: Implement fitness function

		return 0f;
	}

	public void Crossover(ref Genome<AngryBirdsGen> genome1, ref Genome<AngryBirdsGen> genome2, 
	                      out Genome<AngryBirdsGen> child1,  out Genome<AngryBirdsGen> child2) {

		child1 = new Genome<AngryBirdsGen>();
		child2 = new Genome<AngryBirdsGen>();

		// TODO: Implement crossover operation
	}
	
	public void Mutate(ref Genome<AngryBirdsGen> genome) {

		// TODO: Implement mutation operation
	}

	public void InitAngryBirdsGenome(AngryBirdsGen genome) {

		// TODO: Implement initialization operation
	}
}
