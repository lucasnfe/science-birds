using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ABMath {

	public static float RandomGaussian(float mean = 0f, float sigma = 1f)
	{
	    float u, v, S;

	    do
	    {
	        u = 2f * Random.value - 1f;
	        v = 2f * Random.value - 1f;
	        S = u * u + v * v;
	    }
	    while (S >= 1.0);

	    float fac = Mathf.Sqrt(-2f * Mathf.Log(S) / S);
	    return (u * fac * sigma) + mean;
	}
	
	public static float Average(float []data)
	{
		float sum = 0f;
		
		int n = data.Length;
		
		for(int i = 0; i < n; i++)
		{
			sum += data[i];
		}
		
		return sum/n;
	}
	
	public static float LinearRegression(Vector2 []data, float x)
	{
		float sum_x = 0f;
		float sum_y = 0f;
		float sum_xy = 0f;
		float sum_xx = 0f;
		
		int n = data.Length;
		
		for(int i = 0; i < n; i++)
		{
			sum_x += data[i].x;
			sum_y += data[i].y;
			sum_xy += data[i].x * data[i].y;
			sum_xx += data[i].x * data[i].x;
		}
			
		float b = (sum_xy - (sum_x * sum_y)/n)/(sum_xx - (sum_x*sum_x)/n);
		float a = (sum_y/n) - b * (sum_x/n);
		
		return a + b*x;
	}
}
