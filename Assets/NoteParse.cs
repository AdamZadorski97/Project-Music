using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteParse : MonoBehaviour
{
    private Dictionary<string, int> fullNotesDictionary;
    private int totalNotes;

    private void Awake()
    {
        InitializeNotesDictionary();
    }
    private void InitializeNotesDictionary()
    {
        fullNotesDictionary = new Dictionary<string, int>();
        string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int index = 0;

        for (int octave = 0; octave <= 8; octave++)
        {
            foreach (string note in notes)
            {
                string fullNote = note + octave;
                fullNotesDictionary[fullNote] = index++;
            }
        }
        totalNotes = fullNotesDictionary.Count; // Store the total number of notes
    }
    [Button]
    public float NormalizeNoteIndex(string note)
    {
        if (fullNotesDictionary.ContainsKey(note))
        {
            return (float)fullNotesDictionary[note] / (totalNotes - 1);
        }
        else
        {
            Debug.LogError($"Note {note} does not exist in the dictionary.");
            return -1; // Error code for note not found
        }
    }
}