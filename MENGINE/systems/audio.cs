using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Media;
using System.Linq.Expressions;

namespace MEngine;

public static class SoundSystem
{
    public static List<Sound> ActiveSounds = new();

    public static void DisposeAllSounds()
    {
        var sounds = new List<Sound>(ActiveSounds);
        foreach (Sound sound in sounds)
        {
            sound.Dispose();
        }
    }

    /// <summary>
    /// Creates a single use non-returned sound instance and plays it, then instantly gets rid of it.
    /// </summary>
    /// <param name="FileName">Name of .wav sound file <b>inside of \game\audio</b></param>
    /// <remarks><i>Created sound uses default SoundData parameters. If this isn't desired behaviour, invoke function with a SoundData parameter instead.</i></remarks>
    public static void PlaySound(string FileName)
    {
        Sound sound = new(new SoundData { 
                FileName = FileName,
            });
        sound.Play();
        sound.Dispose();
    }
    /// <summary>
    /// Creates a single use non-returned sound instance and plays it, then instantly gets rid of it.
    /// </summary>
    /// <param name="data">SoundData associated with the sound.</param>
    public static void PlaySound(SoundData data)
    {
        Sound sound = new(data);
        sound.Play();
        sound.Dispose();
    }
    
    /// <summary>
    /// Creates a sound from provided parameters.
    /// </summary>
    /// <param name="data">SoundData parameters to be applied to new sound.</param>
    /// <returns>Created Sound instance.</returns>
    public static Sound CreateSound(SoundData data)
    {
        return new Sound(data);
    }
    /// <summary>
    /// Creates a sound from provided parameters.
    /// </summary>
    /// <param name="FileName">Name of .wav sound file <b>inside of \game\audio</b></param>
    /// <returns>Created Sound instance.</returns>
    /// <remarks><i>Created sound uses default SoundData parameters. If this isn't desired behaviour, invoke function with a SoundData parameter instead.</i></remarks>
    public static Sound CreateSound(string FileName)
    {
        return new Sound(new SoundData { FileName = FileName });
    }
}

public class SoundData
{
    public string FileName = null;
    public bool IsLooped = false;
    public float Volume = 1;
    public bool AutoLoad = true;
}

/// <summary>
/// Sound instance class. Cannot be created manually, use SoundSystem.CreateSound().
/// </summary>
public sealed class Sound
{
    internal Sound(SoundData data)
    {
        player = new();

        OriginalFileName = Path.Combine(Directory.GetCurrentDirectory(), "audio" , data.FileName);
        IsLooped = data.IsLooped;

        SoundUtil.LoadToTemp(this);

        if (data.Volume != 1f)
            SetVolume(data.Volume);

        player.SoundLocation = TempFile;

        if (data.AutoLoad)
            player.Load();

        SoundSystem.ActiveSounds.Add(this);
    }

    public void Load()
    {
        player.LoadAsync();
    }
    public void Play()
    {
        if (IsLooped)
            player.PlayLooping();
        else
        {
            player.Play();
        }
    }

    public void Stop()
    {
        player.Stop();
    }

    public void Dispose()
    {
        player = null;
        SoundSystem.ActiveSounds.Remove(this);
        File.Delete(TempFile);
    }

    private SoundPlayer player;// = new();
    public string TempFile;
    public string OriginalFileName;
    public bool IsPlaying;
    public bool IsLooped;
    public bool IsLoaded;
    public float Volume;

    /// <summary>
    /// Sets the volume of a sound.
    /// </summary>
    /// <param name="volume">A float value representing the volume from 0.0 to 1.0.</param>
    /// <remarks>You cannot change the volume of a sound while it's playing, as it creates a new instance of the audio file. Use Sound.Play() again after changing the volume.</remarks>
    public void SetVolume(float volume)
    {
        Volume = volume;
        SoundUtil.ChangeAmplitudes(this, Volume);
    }

    /// <summary>
    /// Gets the current SoundPlayer of a sound for direct access.
    /// </summary>
    /// <returns>The SoundPlayer instance of the Sound instance.</returns>
    public SoundPlayer GetSoundPlayer() { return player; }
}

internal static class SoundUtil
{
    public static string TempFolder = Path.GetTempPath();

    public static void LoadToTemp(Sound sound)
    {
        string TempFile = Path.Combine(TempFolder, "MENGINEsound_" + Path.GetFileNameWithoutExtension(sound.OriginalFileName) + sound.GetHashCode() + ".wav");
        File.WriteAllBytes(TempFile, File.ReadAllBytes(sound.OriginalFileName));
        sound.TempFile = TempFile;
    }

    public static void ChangeAmplitudes(Sound sound, float multiplier)
    {
        // Read the input .wav file
        byte[] wavData = File.ReadAllBytes(sound.TempFile);

        // Extract the audio data
        int headerSize = 44; // Assuming a standard 44-byte header
        byte[] audioData = new byte[wavData.Length - headerSize];
        Buffer.BlockCopy(wavData, headerSize, audioData, 0, audioData.Length);

        // Convert the audio data to 16-bit samples
        short[] samples = new short[audioData.Length / 2];
        try
        {
            Buffer.BlockCopy(audioData, 0, samples, 0, audioData.Length);
        }
        catch
        {
            throw new Exception("Audio volume change threw a BlockCopy error, this is likely because the provided .wav file had a sample size of less than 16 bits.");
        }

        // Halve the amplitude of each sample
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (short)(samples[i]*multiplier);
        }

        // Convert the samples back to byte array
        byte[] modifiedAudioData = new byte[samples.Length * 2];
        Buffer.BlockCopy(samples, 0, modifiedAudioData, 0, modifiedAudioData.Length);

        // Write the modified audio data to the output .wav file
        byte[] modifiedWavData = new byte[wavData.Length];
        Buffer.BlockCopy(wavData, 0, modifiedWavData, 0, headerSize);
        Buffer.BlockCopy(modifiedAudioData, 0, modifiedWavData, headerSize, modifiedAudioData.Length);
        File.WriteAllBytes(sound.TempFile, modifiedWavData);
    }
}

/*
public static void HalveAmplitudes(string inputFile, string outputFile)
    {
        // Read the input .wav file
        byte[] wavData = File.ReadAllBytes(inputFile);

        // Extract the audio data
        int headerSize = 44; // Assuming a standard 44-byte header
        byte[] audioData = new byte[wavData.Length - headerSize];
        Buffer.BlockCopy(wavData, headerSize, audioData, 0, audioData.Length);

        // Convert the audio data to 16-bit samples
        short[] samples = new short[audioData.Length / 2];
        Buffer.BlockCopy(audioData, 0, samples, 0, audioData.Length);

        // Halve the amplitude of each sample
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] /= 2;
        }

        // Convert the samples back to byte array
        byte[] modifiedAudioData = new byte[samples.Length * 2];
        Buffer.BlockCopy(samples, 0, modifiedAudioData, 0, modifiedAudioData.Length);

        // Write the modified audio data to the output .wav file
        byte[] modifiedWavData = new byte[wavData.Length];
        Buffer.BlockCopy(wavData, 0, modifiedWavData, 0, headerSize);
        Buffer.BlockCopy(modifiedAudioData, 0, modifiedWavData, headerSize, modifiedAudioData.Length);
        File.WriteAllBytes(outputFile, modifiedWavData);
    } 


 * string tempFolderPath = Path.GetTempPath();
        Console.WriteLine("Temporary Folder Path: " + tempFolderPath);

        // Create a temporary file in the temporary folder
        string tempFilePath = Path.Combine(tempFolderPath, "tempfile.txt");
        File.WriteAllText(tempFilePath, "This is a temporary file.");

        Console.WriteLine("Temporary File Path: " + tempFilePath);

        // Use the temporary file...

        // Clean up the temporary file when you're done
        File.Delete(tempFilePath);

        Console.WriteLine("Temporary file has been deleted.");*/