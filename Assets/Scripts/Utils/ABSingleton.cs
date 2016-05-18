using UnityEngine;
using System.Collections;

public class ABSingleton<T> : MonoBehaviour where T : MonoBehaviour {

	//Here is a private reference only this class can access
	private static T _instance;
	
	// Avoid calling contructor with new
	protected ABSingleton () 
	{}

	//This is the public reference that other classes will use
	public static T Instance
	{
		get
		{
			//If _instance hasn't been set yet, we grab it from the scene!
			//This will only happen the first time this reference is used.
			if(_instance == null)
				_instance = (T) FindObjectOfType(typeof(T));
		
			if(_instance == null)
			{
				GameObject singleton = new GameObject();
				_instance = singleton.AddComponent<T>();
				singleton.name = typeof(T).ToString();
				DontDestroyOnLoad(singleton);
			}

			return _instance;
		}
	}
}
