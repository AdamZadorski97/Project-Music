using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
//    private AudioSource audioSource;

//    private Dictionary<string, float> noteFrequencies = new Dictionary<string, float>
//{
//    {"A0", 27.5f},
//    {"A#0/Bb0", 29.1352f},
//    {"B0", 30.8677f},
//    {"C1", 32.7032f},
//    {"C#1/Db1", 34.6478f},
//    {"D1", 36.7081f},
//    {"D#1/Eb1", 38.8909f},
//    {"E1", 41.2034f},
//    {"F1", 43.6535f},
//    {"F#1/Gb1", 46.2493f},
//    {"G1", 48.9994f},
//    {"G#1/Ab1", 51.9131f},
//    {"A1", 55.0f},
//    {"A#1/Bb1", 58.2705f},
//    {"B1", 61.7354f},
//    {"C2", 65.4064f},
//    {"C#2/Db2", 69.2957f},
//    {"D2", 73.4162f},
//    {"D#2/Eb2", 77.7817f},
//    {"E2", 82.4069f},
//    {"F2", 87.3071f},
//    {"F#2/Gb2", 92.4986f},
//    {"G2", 97.9989f},
//    {"G#2/Ab2", 103.826f},
//    {"A2", 110.0f},
//    {"A#2/Bb2", 116.541f},
//    {"B2", 123.471f},
//    {"C3", 130.813f},
//    {"C#3/Db3", 138.591f},
//    {"D3", 146.832f},
//    {"D#3/Eb3", 155.563f},
//    {"E3", 164.814f},
//    {"F3", 174.614f},
//    {"F#3/Gb3", 184.997f},
//    {"G3", 195.998f},
//    {"G#3/Ab3", 207.652f},
//    {"A3", 220.0f},
//    {"A#3/Bb3", 233.082f},
//    {"B3", 246.942f},
//    {"C4", 261.626f},
//    {"C#4/Db4", 277.183f},
//    {"D4", 293.665f},
//    {"D#4/Eb4", 311.127f},
//    {"E4", 329.628f},
//    {"F4", 349.228f},
//    {"F#4/Gb4", 369.994f},
//    {"G4", 391.995f},
//    {"G#4/Ab4", 415.305f},
//    {"A4", 440.0f},
//    {"A#4/Bb4", 466.164f},
//    {"B4", 493.883f},
//    {"C5", 523.251f},
//    {"C#5/Db5", 554.365f},
//    {"D5", 587.330f},
//    {"D#5/Eb5", 622.254f},
//    {"E5", 659.255f},
//    {"F5", 698.456f},
//    {"F#5/Gb5", 739.989f},
//    {"G5", 783.991f},
//    {"G#5/Ab5", 830.609f},
//    {"A5", 880.0f},
//    {"A#5/Bb5", 932.328f},
//    {"B5", 987.767f},
//    {"C6", 1046.50f},
//    {"C#6/Db6", 1108.73f},
//    {"D6", 1174.66f},
//    {"D#6/Eb6", 1244.51f},
//    {"E6", 1318.51f},
//    {"F6", 1396.91f},
//    {"F#6/Gb6", 1479.98f},
//    {"G6", 1567.98f},
//    {"G#6/Ab6", 1661.22f},
//    {"A6", 1760.0f},
//    {"A#6/Bb6", 1864.66f},
//    {"B6", 1975.53f},
//    {"C7", 2093.0f},
//    {"C#7/Db7", 2217.46f},
//    {"D7", 2349.32f},
//    {"D#7/Eb7", 2489.02f},
//    {"E7", 2637.02f},
//    {"F7", 2793.83f},
//    {"F#7/Gb7", 2959.96f},
//    {"G7", 3135.96f},
//    {"G#7/Ab7", 3322.44f},
//    {"A7", 3520.0f},
//    {"A#7/Bb7", 3729.31f},
//    {"B7", 3951.07f},
//    {"C8", 4186.01f}
//};

//   // public List<Melody> melodies = new List<Melody>();
//    public float pauseBetweenMelodies = 0.1f; // Pause between melodies in seconds

//    void Start()
//    {
//        audioSource = GetComponent<AudioSource>();
//        StartCoroutine(PlayMelodies());
//    }

//    private IEnumerator PlayMelodies()
//    {
//        foreach (var melody in melodies)
//        {
//            foreach (var note in melody.notes)
//            {
//                if (noteFrequencies.TryGetValue(note.noteName, out float frequency))
//                {
//                    audioSource.clip = CreateToneAudioClip(frequency, note.duration);
//                    audioSource.Play();
//                    yield return new WaitForSeconds(note.duration);
//                }
//            }
//            yield return new WaitForSeconds(pauseBetweenMelodies); // Wait after each melody
//        }
//    }

//    private AudioClip CreateToneAudioClip(float frequency, float duration)
//    {
//        int sampleRate = 44100;
//        int sampleLength = (int)(sampleRate * duration);
//        float[] samples = new float[sampleLength];

//        for (int i = 0; i < sampleLength; i++)
//        {
//            float t = i / (float)sampleRate;
//            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t);
//        }

//        AudioClip audioClip = AudioClip.Create("Tone", sampleLength, 1, sampleRate, false);
//        audioClip.SetData(samples, 0);
//        return audioClip;
//    }
}
