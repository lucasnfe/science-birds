
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class GeneticAlgorithm<T> {

	// Genetic Algorithm delegates
	public delegate void GAInitGenome(out T values);
	public delegate void GACrossover(ref Genome<T> genome1, ref Genome<T> genome2, out Genome<T> child1, out Genome<T> child2);
	public delegate void GAMutation (ref Genome<T> genome1);

	public delegate double GAFitnessFunction(T values, int genomeIdx);

	// Genetic Algorithm attributes
	private int _populationSize;
	private int _generationSize;

	private double _mutationRate;
	private double _crossoverRate;

	private bool _elitism;

	private double _totalFitness;
	private string _strFitness;

	// Genetic Algorithm data structures
	private ArrayList _thisGeneration;
	private ArrayList _nextGeneration;
	private ArrayList _fitnessTable;
	
	static System.Random m_random = new System.Random();
	
	static private GAFitnessFunction getFitness;

	static private GAInitGenome getInitGenome;
	static private GACrossover  getCrossover;
	static private GAMutation   getMutation;

	public GeneticAlgorithm() {

		_mutationRate = 0.05;
		_crossoverRate = 0.80;
		_populationSize = 100;
		_generationSize = 2000;

		_strFitness = "";
	}
	
	public GeneticAlgorithm(double crossoverRate, double mutationRate, int populationSize, int generationSize, bool elitism = false) {

		_mutationRate = mutationRate;
		_crossoverRate = crossoverRate;
		_populationSize = populationSize;
		_generationSize = generationSize;
		_elitism = elitism;

		_strFitness = "";
	}

	public GAFitnessFunction FitnessFunction {

		get  {
			return getFitness;
		}
		set {
			getFitness = value;
		}
	}

	public GACrossover Crossover {
		
		get  {
			return getCrossover;
		}
		set {
			getCrossover = value;
		}
	}

	public GAMutation Mutation {
		
		get  {
			return getMutation;
		}
		set {
			getMutation = value;
		}
	}

	public GAInitGenome InitGenome {
		
		get  {
			return getInitGenome;
		}
		set {
			getInitGenome = value;
		}
	}
	
	//  Properties
	public int PopulationSize {

		get {
			return _populationSize;
		}
		set {
			_populationSize = value;
		}
	}
	
	public int Generations {

		get {
			return _generationSize;
		}
		set {
			_generationSize = value;
		}
	}
	
	public double CrossoverRate {

		get {
			return _crossoverRate;
		}
		set {
			_crossoverRate = value;
		}
	}
	public double MutationRate {

		get {
			return _mutationRate;
		}
		set {
			_mutationRate = value;
		}
	}
	
	public string FitnessFile {

		get {
			return _strFitness;
		}
		set {
			_strFitness = value;
		}
	}
	
	/// Keep previous generation's fittest individual in place of worst in current
	public bool Elitism {

		get {
			return _elitism;
		}
		set {
			_elitism = value;
		}
	}
	
	public void GetBest(out T values, out double fitness) {

		Genome<T> g = ((Genome<T>)_thisGeneration[_populationSize - 1]);

		values = g.Genes;
		fitness = (double)g.Fitness;
	}

	public void GetWorst(out T values, out double fitness) {

		GetNthGenome(0, out values, out fitness);
	}
	
	public void GetNthGenome(int n, out T values, out double fitness) {

		if (n < 0 || n > _populationSize - 1)
			throw new ArgumentOutOfRangeException("n too large, or too small");

		Genome<T> g = ((Genome<T>)_thisGeneration[n]);

		values = g.Genes;
		fitness = (double)g.Fitness;
	}

	// Method which starts the GA executing.
	public void StartEvolution() {

		if (getFitness == null)
			throw new ArgumentNullException("Need to supply fitness function");

		if (getInitGenome == null)
			throw new ArgumentNullException("Need to supply initialization function");

		if (getCrossover == null)
			throw new ArgumentNullException("Need to supply crossover function");

		if (getMutation == null)
			throw new ArgumentNullException("Need to supply mutation function");
		
		//  Create the fitness table.
		_fitnessTable = new ArrayList();

		// Create current and next generations
		_thisGeneration = new ArrayList(_generationSize);
		_nextGeneration = new ArrayList(_generationSize);

		Genome<T>.MutationRate = _mutationRate;

		InitializePopulation();
		//RankPopulation();
		
		//for (int i = 0; i < _generationSize; i++) {

		//	CreateNextGeneration();
		//	RankPopulation();
		//}
	}
	
	// After ranking all the genomes by fitness, use a 'roulette wheel' selection
	// method.  This allocates a large probability of selection to those with the 
	// highest fitness.
	private int RouletteSelection() {

		double randomFitness = m_random.NextDouble() * _totalFitness;
		int idx = -1;
		int first = 0;
		int last = _populationSize -1;
		int mid = (last - first)/2;
		
		//  ArrayList's BinarySearch is for exact values only
		//  so do this by hand.	
		while (idx == -1 && first <= last) {

			if (randomFitness < (double)_fitnessTable[mid])
				last = mid;

			else if (randomFitness >= (double)_fitnessTable[mid])
				first = mid;

			mid = (first + last)/2;

			//  lies between i and i+1
			if ((last - first) == 1)
				idx = last;
		}

		return idx;
	}
	
	public void CreateNextGeneration()
	{
		_nextGeneration.Clear();
		Genome<T> g = null;

		if (_elitism)
			g = (Genome<T>)_thisGeneration[_populationSize - 1];

		for (int i = 0; i < _populationSize; i+=2) {

			int pidx1 = RouletteSelection();
			int pidx2 = RouletteSelection();
			
			Genome<T> parent1, parent2, child1, child2;
			
			parent1 = ((Genome<T>) _thisGeneration[pidx1]);
			parent2 = ((Genome<T>) _thisGeneration[pidx2]);
			
			if (m_random.NextDouble() < _crossoverRate) {
				
				Crossover(ref parent1, ref parent2, out child1, out child2);
			}
			else {
				
				child1 = parent1;
				child2 = parent2;
			}
			
			Mutation(ref child1);
			Mutation(ref child2);
			
			_nextGeneration.Add(child1);
			_nextGeneration.Add(child2);
		}

		if (_elitism && g != null)
			_nextGeneration[0] = g;
		
		_thisGeneration.Clear();
		
		for (int i = 0; i < _populationSize; i++)
			_thisGeneration.Add(_nextGeneration[i]);
	}

	// Rank population and sort in order of fitness.
	public void RankPopulation() {
		
		_totalFitness = 0;
		for (int i = 0; i < _populationSize; i++) {
			
			Genome<T> g = ((Genome<T>) _thisGeneration[i]);
			g.Fitness = FitnessFunction(g.Genes, i);
			_totalFitness += g.Fitness;
		}
		
		_thisGeneration.Sort(new GenomeComparer<T>());
		
		//  now sorted in order of fitness.
		double fitness = 0.0;
		_fitnessTable.Clear();
		
		for (int i = 0; i < _populationSize; i++) {
			
			fitness += ((Genome<T>)_thisGeneration[i]).Fitness;
			_fitnessTable.Add((double)fitness);
		}
	}
	
	// Create the initial genomes by repeated calling the supplied fitness function
	private void InitializePopulation() {
		
		for (int i = 0; i < _populationSize ; i++) {
			
			Genome<T> g = new Genome<T>();

			T genes = g.Genes;
			InitGenome(out genes);
			g.Genes = genes;

			_thisGeneration.Add(g);
		}
	}
}