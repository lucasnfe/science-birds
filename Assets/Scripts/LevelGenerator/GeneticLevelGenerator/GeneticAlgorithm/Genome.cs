using System;
using System.Collections;

public class Genome<T> {
	
	private double _fitness;

	private static double _mutationRate;

	// Genetic Algorithm codification
	private T _genes;

	public double Fitness {

		get {
			return _fitness;
		}
		set {
			_fitness = value;
		}
	}
	
	public static double MutationRate {

		get {
			return _mutationRate;
		}
		set {
			_mutationRate = value;
		}
	}

	public T Genes {

		get {
			return _genes;
		}
		set {
			_genes = value;
		}
	}
}
