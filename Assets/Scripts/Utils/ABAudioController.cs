using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** \class  ABAudioController.
 *  \brief  Manages the audio of the game.
 *
 *  Contains list of SFXs to play, plays and stops musics, returns information about music being played
 *  And creates game objects for the SFX and plays it.
 */
[RequireComponent(typeof(AudioSource))]
public class ABAudioController : ABSingleton<ABAudioController> {
	/**
     *  List of SFXs to be played.
     */
	private List<AudioSource> _independentAudioSources;

    /**
     *  creates the list of audio sources when the instance of this script is being loaded.
     */
	void Awake() 
	{
		_independentAudioSources = new List<AudioSource>();
	}

    /**
     *  if at any update of the game there is an SFXs not playing, destroy it and remove from list.
     */
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

    /**
     * Plays the passed music.
     * @param[in]   music   Music to be played. AudioClip object
     * @param[in]   loop    Boolean if music should be played in a loop. Default true.
     */
	public void PlayMusic(AudioClip music, bool loop = true)
	{
		gameObject.GetComponent<AudioSource>().clip = music;
		gameObject.GetComponent<AudioSource>().loop = loop;
		gameObject.GetComponent<AudioSource>().Play();
	}

    /**
     *  Returns true if passed music is currently being played, false otherwise.
     *  @param[in]  music   Music to check if is being played now. AudioClip object.
     *  @return bool    true if music is being played, false otherwise.
     */
	public bool IsPlayingMusic(AudioClip music) 
	{
		return gameObject.GetComponent<AudioSource>().isPlaying && 
			   gameObject.GetComponent<AudioSource>().clip == music;
	}

    /**
     *  Stops playing music
     */
	public void StopMusic()
	{
		gameObject.GetComponent<AudioSource>().Stop();
	}

    /**
     *  Creates a game object for the music passed and plays it as an independet audio source.
     *  @param[in]  music   Music to be played. AudioClip object
     *  @param[in]  loop    Boolean if music should be played on loop. Default true.
     */
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
