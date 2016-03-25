using UnityEngine;
using System.Collections;

/** \class ABArrayUtils
 *  \brief  Array Utilites.
 *
 *  Contains a function to shuffle the items in an array
 */
public class ABArrayUtils {

    /** Shuffles the position of items in an array, randomly selecting a new position for each item
    *   @param[in]  array    The array to be shuffled, can be of any type (Template). 
    */
    public static void Shuffle<T>(T[] array)
	{
		int n = array.Length;
		for (int i = 0; i < n; i++)
		{
			int r = i + (int)(Random.value * (n - i));
			T t = array[r];
			array[r] = array[i];
			array[i] = t;
		}
	}
}
