using System;
using System.Collections;

// Compares genomes by fitness
public sealed class GenomeComparer<T> : IComparer
{
	public GenomeComparer() {

	}

	public int Compare(object x, object y) {

		if ( !(x is Genome<T>) || !(y is Genome<T>))
			throw new ArgumentException("Not of type Genome");
		
		if (((Genome<T>) x).Fitness > ((Genome<T>) y).Fitness)
			return 1;

		else if (((Genome<T>) x).Fitness == ((Genome<T>) y).Fitness)

			return 0;
		else
			return -1;
	}
}