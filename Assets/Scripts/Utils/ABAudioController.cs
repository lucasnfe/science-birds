using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ABAudioController : ABSingleton<ABAudioController> {
	
	public void PlayMusic(AudioClip music)
	{
		gameObject.GetComponent<AudioSource>().clip = music;
		gameObject.GetComponent<AudioSource>().Play();
	}

	public bool IsPlayingMusic(AudioClip music) 
	{
		return gameObject.GetComponent<AudioSource>().isPlaying && 
			   gameObject.GetComponent<AudioSource>().clip == music;
	}

	public void StopMusic()
	{
		gameObject.GetComponent<AudioSource>().Stop();
	}
}
