using System;
using System.Collections;

/** \class GenomeComparer
 *  \brief  Compares two genomes by their fitness
 *
 *  Compares the fitness of two genomes, looking which is greater of if they are equal.
 */
public sealed class GenomeComparer<T> : IComparer
{
    /**
     *  Empty constructor
     */
	public GenomeComparer() {

	}
    /**
     *  Compare two objects of genome type by their fitness, if at least one of them is not
     *  of genome type, throws ArgumentException
     *  @param[in]  x   object to be compared first, should be of Genome type
     *  @param[in]  y   object to be compared second, should be of Genome type
     *  @return int 1 if x fitness > y fitness, 0 if they are equal, -1 otherwise
     */
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