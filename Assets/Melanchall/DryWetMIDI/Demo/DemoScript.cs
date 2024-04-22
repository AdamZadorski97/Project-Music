using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Standards;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using VLB;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Crosstales.FB;
using Crosstales.FB.Wrapper;
using System.Collections;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class DemoScript : MonoBehaviour
{

    public List<TMP_Text> channelsNoteText;
    private const string OutputDeviceName = "Microsoft GS Wavetable Synth";
    private const string MidiFilePath = "StreamingAssets/eiffel-65-i-m-blue-20220916155855-nonstop2k.com.mid";
    private OutputDevice _outputDevice;
    private Playback _playback;
    private Gradient _colorGradient;
    private Gradient[] _channelGradients;
    public bool[] _channelMuteStates = new bool[16];
    private Vector3 _currentNotePosition;
    public Transform cameraTransform;
    public List<VolumetricLightBeamSD> volumetricLightBeamHDs = new List<VolumetricLightBeamSD>();
    public NoteParse noteParse;
    public float lightTurnOffSpeed;
    public Slider intensitySlider;
    public TMP_Text timeText;
    public Slider timeSlider;
    public FileBrowser fileBrowser;
    public GameObject noteButton;
    public Transform noteButtonParrent;
    public int noteButtonChannel;
    public List<GameObject> activeNotes = new List<GameObject>();
    public VisualEffect spawnCubeEffect;
    public Gradient vfxGradient;
    public Transform VfxSpawnPosition;
    void Awake()
    {

        InitializeChannelGradients();
    }
    private MidiEvent HandleMidiEventPlayback(MidiEvent midiEvent, long absoluteTime)
    {
        // Filter out the event based on channel mute state
        if (midiEvent is ChannelEvent channelEvent)
        {
            if (_channelMuteStates[channelEvent.Channel])
                return null; // Mute the event by returning null
        }
        return midiEvent; // Return the event unmodified if not muted
    }
    private void Start()
    {
        InitializeOutputDevice();

        InitializeFilePlayback(LoadMidiFileFromStreamingAssets(fileBrowser.OpenSingleFile()));

        StartPlayback();
    }

    public void NewTrack()
    {

        StartCoroutine(ResetTrackCoroutine());

    }
    IEnumerator ResetTrackCoroutine()
    {

        _playback.Stop();
        ResetTrack();
        yield return new WaitForSeconds(1f);
        InitializeFilePlayback(LoadMidiFileFromStreamingAssets(fileBrowser.OpenSingleFile()));
        yield return new WaitForSeconds(0.1f);
        StartPlayback();
    }




    private MidiFile LoadMidiFileFromStreamingAssets(string path)
    {
        Debug.Log("Loading MIDI file from StreamingAssets..." + path);

        string filePath = path;
        if (!File.Exists(filePath))
        {
            Debug.LogError($"MIDI file not found at {filePath}");
            return null;
        }

        var midiFile = MidiFile.Read(filePath);
        Debug.Log("MIDI file loaded from StreamingAssets.");

        return midiFile;
    }

    IEnumerator WaitForFile(MidiFile midiFile)
    {
        yield return new WaitForSeconds(0.1f);

    }
    private ConcurrentQueue<Note> _notesToProcess = new ConcurrentQueue<Note>();
    void Update()
    {
        try
        {
            // Manually parse current time and duration
            TimeSpan currentTime = ParseTimeSpan(_playback.GetCurrentTime(TimeSpanType.Metric).ToString());
            TimeSpan endTime = ParseTimeSpan(_playback.GetDuration(TimeSpanType.Metric).ToString());

            // Calculate the slider value
            if (endTime.TotalSeconds > 0) // Ensure there is a duration to avoid division by zero
            {
                float fraction = (float)(currentTime.TotalSeconds / endTime.TotalSeconds);
                timeSlider.value = fraction;
            }

            // Update the time display text with zero-padded minutes and seconds
            timeText.text = $"{currentTime.Minutes:00}:{currentTime.Seconds:00}/{endTime.Minutes:00}:{endTime.Seconds:00}";


            if ($"{currentTime.Minutes:00}:{currentTime.Seconds:00}" == $"{endTime.Minutes:00}:{endTime.Seconds:00}")
            {
                ResetTrack();
            }
        }





        catch (Exception ex)
        {
            Debug.LogError("Failed to parse time: " + ex.Message);
        }

        while (_notesToProcess.TryDequeue(out Note note))
        {
            CreateNoteCube(note);
        }
        if (cameraTransform != null)
        {
            Vector3 targetPosition = new Vector3(_currentNotePosition.x, cameraTransform.position.y, cameraTransform.position.z);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * 2.0f);  // Adjust speed as necessary
        }
        foreach (VolumetricLightBeamSD light in volumetricLightBeamHDs)
        {
            if (light.intensityGlobal > 0.2f)
                light.intensityGlobal -= lightTurnOffSpeed * Time.deltaTime;
            else
            {
                light.intensityGlobal -= lightTurnOffSpeed * Time.deltaTime / 10;
            }
        }

    }

    TimeSpan ParseTimeSpan(string timeString)
    {
        string[] parts = timeString.Split(':');
        int hours = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        int seconds = int.Parse(parts[2]);
        int milliseconds = int.Parse(parts[3]);

        return new TimeSpan(0, hours, minutes, seconds, milliseconds);
    }



    private void OnApplicationQuit()
    {
        Debug.Log("Releasing playback and device...");

        if (_playback != null)
        {
            _playback.NotesPlaybackStarted -= OnNotesPlaybackStarted;
            _playback.NotesPlaybackFinished -= OnNotesPlaybackFinished;
            _playback.Dispose();
        }

        if (_outputDevice != null)
            _outputDevice.Dispose();

        Debug.Log("Playback and device released.");
    }

    private void InitializeOutputDevice()
    {
        Debug.Log($"Initializing output device [{OutputDeviceName}]...");

        var allOutputDevices = OutputDevice.GetAll();
        if (!allOutputDevices.Any(d => d.Name == OutputDeviceName))
        {
            var allDevicesList = string.Join(Environment.NewLine, allOutputDevices.Select(d => $"  {d.Name}"));
            Debug.Log($"There is no [{OutputDeviceName}] device presented in the system. Here the list of all device:{Environment.NewLine}{allDevicesList}");
            return;
        }

        _outputDevice = OutputDevice.GetByName(OutputDeviceName);
        Debug.Log($"Output device [{OutputDeviceName}] initialized.");
    }

    private MidiFile CreateTestFile()
    {
        Debug.Log("Creating test MIDI file...");

        var patternBuilder = new PatternBuilder()
            .SetNoteLength(MusicalTimeSpan.Eighth)
            .SetVelocity(SevenBitNumber.MaxValue)
            .ProgramChange(GeneralMidiProgram.Harpsichord);

        foreach (var noteNumber in SevenBitNumber.Values)
        {
            patternBuilder.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get(noteNumber));
        }

        var midiFile = patternBuilder.Build().ToFile(TempoMap.Default);
        // Debug.Log("Test MIDI file created.");

        return midiFile;
    }

    private void InitializeFilePlayback(MidiFile midiFile)
    {
        _playback = midiFile.GetPlayback(_outputDevice);
        _playback.Loop = true;
        _playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
        _playback.NotesPlaybackFinished += OnNotesPlaybackFinished;

        // Debug.Log("Playback initialized.");
    }


    private void StartPlayback()
    {
        // Debug.Log("Starting playback...");
        _playback.Start();
        _playback.Speed = 1f;
    }

    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e)
    {
        //   LogNotes("Notes finished:", e);
    }
    private void InitializeChannelGradients()
    {
        _channelGradients = new Gradient[16]; // MIDI has 16 channels
        for (int i = 0; i < _channelGradients.Length; i++)
        {
            _channelGradients[i] = new Gradient();

            // Example gradient initialization, customize this part as needed
            GradientColorKey[] colorKey = {
                new GradientColorKey(Color.red, 0.2f),
                new GradientColorKey(Color.green, 0.5f),
                new GradientColorKey(Color.blue, 0.7f)
            };

            GradientAlphaKey[] alphaKey = {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.5f),
                new GradientAlphaKey(1.0f, 1.0f)
            };

            _channelGradients[i].SetKeys(colorKey, alphaKey);
        }
    }
    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
    {
        foreach (var note in e.Notes)
        {
            if (_channelMuteStates[note.Channel]) // Check if the channel is active
            {
                _notesToProcess.Enqueue(note);
            }
        }
    }
    [Button]
    public void ResetTrack()
    {
        // Assuming 'cubeParent' is the parent GameObject and has children that need to be destroyed
        int childCount = cubeParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = cubeParent.transform.GetChild(i).gameObject;
            Destroy(child);
        }
    }



    public GameObject NoteCubePrefab;
    public Transform cubeParent;
    private void CreateNoteCube(Note note)
    {
        Debug.Log("x");
        float noteLengthInSeconds = (float)note.LengthAs<MetricTimeSpan>(_playback.TempoMap).TotalMicroseconds / 1000000f;
        int noteHeight = note.NoteNumber - 60;  // Middle C (C4) is considered as the center line

        // Create and position the cube
        GameObject cube = Instantiate(NoteCubePrefab, new Vector3(note.TimeAs<MetricTimeSpan>(_playback.TempoMap).TotalMicroseconds / 1000000f, noteHeight, 0), Quaternion.identity);
        cube.transform.localScale = Vector3.zero;
        cube.transform.DOScale(new Vector3(noteLengthInSeconds, 1, 1), 0.5f);  // Scale the cube based on note length
        cube.transform.DOLocalJump(cube.transform.position, 2, 0, 0.5f);
        cube.name = $"Note {note.NoteName}";  // Rename for easier identification
        cube.transform.SetParent(cubeParent);

        float normalizedIntensity = 0;
        // Normalize the note index and set initial light intensity
        if (note.NoteName.ToString().IndexOf("sharp", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            string updatedNoteName = System.Text.RegularExpressions.Regex.Replace(note.NoteName.ToString(), "sharp", "#", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            channelsNoteText[(int)note.Channel].text = $"CH{note.Channel} {updatedNoteName} {note.Octave}";
            normalizedIntensity = noteParse.NormalizeNoteIndex(updatedNoteName + note.Octave);
        }
        else
        {
            channelsNoteText[(int)note.Channel].text = $"CH{note.Channel} {note.NoteName}{note.Octave}";
            normalizedIntensity = noteParse.NormalizeNoteIndex(note.NoteName.ToString() + note.Octave);
        }

        // Set initial intensity based on normalized note value and then fade out
        var lightComponent = volumetricLightBeamHDs[UnityEngine.Random.Range(note.Channel, note.Channel + 2)];
        lightComponent.intensityGlobal = normalizedIntensity * 4 * intensitySlider.value;

        if (noteButtonChannel == note.Channel)
            SpawnNote(note);
        // Set cube color based on note
        Gradient gradient = _channelGradients[note.Channel];
        float colorPosition = Mathf.InverseLerp(21, 108, note.NoteNumber);
        Color noteColor = gradient.Evaluate(colorPosition);
        cube.GetComponent<Renderer>().material.color = noteColor;
        _currentNotePosition = cube.transform.position;
        Gradient vfxGradient = new Gradient();
   
        GradientColorKey[] colorKey = {
                new GradientColorKey(noteColor, 0),
                new GradientColorKey(noteColor, 1),
            };

        GradientAlphaKey[] alphaKey = {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            };
        vfxGradient.SetKeys(colorKey, alphaKey);
        spawnCubeEffect.SetGradient("Color", vfxGradient);
        VfxSpawnPosition.position = _currentNotePosition;
        spawnCubeEffect.Play();

        //   cube.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = noteColor;
    }
    bool wasOffset;

    public void SpawnNote(Note note)
    {
        GameObject newNoteButton = Instantiate(noteButton, noteButtonParrent.position, Quaternion.identity, noteButtonParrent);
        newNoteButton.GetComponent<NoteButtonController>(); // Attach the NoteButtonController script
        newNoteButton.GetComponent<NoteButtonController>().lifetime = 2f;
        newNoteButton.GetComponent<NoteButtonController>().noteSpawner = this;

        string noteName = note.NoteName.ToString();
        if (noteName.IndexOf("sharp", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            noteName = System.Text.RegularExpressions.Regex.Replace(noteName, "sharp", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        newNoteButton.GetComponent<NoteButtonController>().noteKey = noteName.ToUpper(); // Use the modified note name as the key
        newNoteButton.transform.GetChild(0).GetComponent<TMP_Text>().text = noteName;
        if (wasOffset)
        {
            newNoteButton.transform.position += new Vector3(80, 0, 0);
            wasOffset = false;
        }
        else
        {
            wasOffset = true;
        }
        activeNotes.Add(newNoteButton); // Add the note button to the list of active notes
    }

    public void RemoveNote(GameObject note)
    {
        if (note != null)
        {
            activeNotes.Remove(note);
        }
        activeNotes.RemoveAll(item => item == null);  // Additional cleanup to remove any null references
    }
    public void ToggleChannelMute(int channel)
    {
        if (channel >= 0 && channel < 16)
        {
            _channelMuteStates[channel] = !_channelMuteStates[channel]; // Toggle the state
            Debug.Log($"Channel {channel} is now {(_channelMuteStates[channel] ? "muted" : "active")}");
        }
    }



    public class NoteParser
    {
        // Dictionary to convert note names to their respective indices
        private readonly Dictionary<string, int> noteIndices = new Dictionary<string, int>
    {
        {"C", 0}, {"C#", 1}, {"D", 2}, {"D#", 3}, {"E", 4},
        {"F", 5}, {"F#", 6}, {"G", 7}, {"G#", 8}, {"A", 9},
        {"A#", 10}, {"B", 11}
    };

        // Normalize a note name and octave to a float between 0 and 1
        public float NormalizeNote(string noteName, int octave)
        {
            int noteIndex = noteIndices[noteName] + octave * 12; // Calculate overall index for the note
            int totalNotes = 12 * 9; // Total notes from C0 to B8
            return (float)noteIndex / (totalNotes - 1); // Normalize index to a float between 0 and 1
        }
    }






    private void LogNotes(string title, NotesEventArgs e)
    {
        var message = new StringBuilder()
            .AppendLine(title)
            .AppendLine(string.Join(Environment.NewLine, e.Notes.Select(n => $"  {n}")))
            .ToString();
        Debug.Log(message.Trim());
    }
}