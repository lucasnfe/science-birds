using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ABAudioController : ABSingleton<ABAudioController> {
	
	private List<AudioSource> _independentAudioSources;

	void Awake() 
	{
		_independentAudioSources = new List<AudioSource>();
	}

	void Update()
	{
		for(int i = 0; i < _independentAudioSources.Count; i++)
		{
			if(!_independentAudioSources[i].isPlaying) {

				Destroy(_independentAudioSources[i].gameObject);
				_independentAudioSources.Remove(_independentAudioSources[i]);
			}
		}
	}

	public void PlayMusic(AudioClip music, bool loop = true)
	{
		gameObject.GetComponent<AudioSource>().clip = music;
		gameObject.GetComponent<AudioSource>().loop = loop;
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

	public void PlayIndependentSFX(AudioClip music, bool loop = false)
	{
		GameObject independentSFX = new GameObject();
		independentSFX.transform.parent = transform;
		AudioSource indAudioSource = independentSFX.AddComponent<AudioSource>();
		indAudioSource.clip = music;
		indAudioSource.loop = loop;
		indAudioSource.Play();

		_independentAudioSources.Add (indAudioSource);
	}
}
