using System;
using System.Collections;

public class Genome<T> {
	
	private double _fitness;

	private static double _mutationRate;

	// Genetic Algorithm codification
	public T _genes;

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

	public T Genes() {

		return _genes;
	}
}
