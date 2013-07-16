using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

/// <summary>
/// Controls the audio for the game
/// </summary>
public static class SoundManager
{
    //the sound effects for our game
    static Dictionary<string, SoundEffect> sounds =
        new Dictionary<string, SoundEffect>();

    //the music for our game
    static Song currentSong;

    //a content manager for later loading sound effects
    static ContentManager content;

    //we now store the sound volume in here
    static float soundVolume = 1f;

    /// <summary>
    /// Initializes the manager.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public static void Initialize(Game game)
    {
        content = game.Content;
    }

    /// <summary>
    /// Plays a sound with a given name.
    /// </summary>
    /// <param name="name">The name of the sound to play.</param>
    public static void PlaySound(string name)
    {
        SoundEffect effect;
        if (sounds.TryGetValue(name, out effect))
        {
            effect.Play(soundVolume,0.0f,0.0f);
        }
            
    }

    /// <summary>
    /// Sets the background music to the sound with the given name.
    /// </summary>
    /// <param name="name">The name of the music to play.</param>
    public static void PlayMusic(string name)
    {
        currentSong = null;

        try
        {
            if (currentSong == null)
                currentSong = content.Load<Song>(name);
        }
        catch (Exception e)
        {
            //if we didn't find the song, rethrow the exception
            if (currentSong == null)
                throw e;
        }

        if (MediaPlayer.State == MediaState.Stopped)
        {
            MediaPlayer.Play(currentSong);
        }
    }

    /// <summary>
    /// Loads a sound for playing
    /// </summary>
    /// <param name="assetName">The asset name of the sound</param>
    public static void LoadSound(string assetName)
    {
        //load the sound into our dictionary
        sounds.Add(assetName, content.Load<SoundEffect>(assetName));
    }

    /// <summary>
    /// Stops the background music.
    /// </summary>
    public static void StopMusic()
    {
        MediaPlayer.Stop();
    }

    /// <summary>
    /// Sets the sound FX volume level.
    /// </summary>
    /// <param name="volume">A volume level in the range [0, 1].</param>
    public static void SetSoundFXVolume(float volume)
    {
        soundVolume = volume;
    }

    /// <summary>
    /// Sets the music volume level.
    /// </summary>
    /// <param name="volume">A volume level in the range [0, 1].</param>
    public static void SetMusicVolume(float volume)
    {
        MediaPlayer.Volume = volume;
    }
}