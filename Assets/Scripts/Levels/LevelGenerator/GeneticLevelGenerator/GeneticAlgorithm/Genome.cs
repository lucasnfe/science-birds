using System;
using System.Collections;

public class Genome<T> {
	
	private float _fitness = UnityEngine.Mathf.Infinity;

	private static float _mutationRate;

	// Genetic Algorithm codification
	private T _genes;

	public float Fitness {

		get {
			return _fitness;
		}
		set {
			_fitness = value;
		}
	}
	
	public static float MutationRate {

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
