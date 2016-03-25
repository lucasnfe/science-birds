using UnityEngine;
using System.Collections;

/** \class ABSingleton
 *  \brief  Singleston game class.
 * 
 *  Singleton class of the game, unabling to create multiple instances of the game at the same time
 */
public class ABSingleton<T> : MonoBehaviour where T : MonoBehaviour {

	/**Here is a private reference only this class can access*/
	private static T _instance;
	
	/**
     *Avoid calling contructor with new*/
	protected ABSingleton () 
	{}

    /** \brief _instance accessor.
     *  
     *  This is the public reference that other classes will use.
     *  If _instance hasn't been set yet, we grab it from the scene!
     *  This will only happen the first time this reference is used.
     */
    public static T Instance
	{
        get
        {
			
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
