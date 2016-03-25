using System;
using System.Collections;

/** \class Genome
 *  \brief  Contains a template for the genome
 *
 *  Contains genome fitness, mutation rate and the genes template.
 */
public class Genome<T> {
	/**fitness value for the genome*/
	private float _fitness = UnityEngine.Mathf.Infinity;
    /**mutation rate for the genome*/
	private static float _mutationRate;

	/**Genetic Algorithm codification*/
	private T _genes;
    /**Accessor for the fitness variable*/
	public float Fitness {

		get {
			return _fitness;
		}
		set {
			_fitness = value;
		}
	}
	/**Accessor for the mutation rate variable*/
	public static float MutationRate {

		get {
			return _mutationRate;
		}
		set {
			_mutationRate = value;
		}
	}
    /**Accessor for the genes template*/
	public T Genes {

		get {
			return _genes;
		}
		set {
			_genes = value;
		}
	}
}
