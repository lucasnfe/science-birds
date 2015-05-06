using UnityEngine;
using System.Collections;

public class ABArrayUtils {

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
