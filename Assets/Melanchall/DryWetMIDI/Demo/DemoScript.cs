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
using System.Collections;
using UnityEngine.VFX;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

public class DemoScript : MonoBehaviour
{

    public List<TMP_Text> channelsNoteText;
    private const string OutputDeviceName = "Microsoft GS Wavetable Synth";
    private OutputDevice _outputDevice;
    private Playback _playback;
    private Gradient[] _channelGradients;
    public bool[] _channelMuteStates = new bool[16];
    private Vector3 _currentNotePosition;
    public Transform cameraTransform;
    public List<LightsChannel> lightsChannels;
    public NoteParse noteParse;
    public TMP_Text timeText;
    public UnityEngine.UI.Slider timeSlider;
    public FileBrowser fileBrowser;
    public GameObject noteButton;
    public Transform noteButtonParrent;
    public List<int> NoteButtonChannels { get; private set; } = new List<int>();
    public List<GameObject> activeNotes = new List<GameObject>();
    public VisualEffect spawnCubeEffect;
    public Gradient vfxGradient;
    public Transform VfxSpawnPosition;
    public GameObject NoteCubePrefab;
    public Transform cubeParent;
    private GameConfig gameConfig;
    public CameraController cameraController;
    public AnimationCurve shakeCurve;
    [Serializable]
    public class LightsChannel
    {
        public int channel;
        public List<VolumetricLightBeamSD> volumetricLightBeamHDs = new List<VolumetricLightBeamSD>();
    }

    void Awake()
    {

    }
    private void Start()
    {
        SettingsManager.Instance.LoadSettings();
        gameConfig = SettingsManager.Instance.settings;
        InitializeOutputDevice();
        StartCoroutine(DequeueVfx());
        InitializeFilePlayback(LoadMidiFileFromStreamingAssets(fileBrowser.OpenSingleFile("mid")));
        InitializeChannelGradients();
        Invoke("StartPlayback", 1);
       
    }
    void Update()
    {
        HandleUITime();
        HandleLightsDims();
        HandleCameraPosition();
        while (_notesToProcess.TryDequeue(out Note note))
        {
            CreateNoteCube(note);
        }
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

    private List<int> ExtractChannels(string filePath)
    {
        // Extract the file name from the path
        string fileName = Path.GetFileName(filePath);

        // Regular expression to find the channel part in the file name
        Regex regex = new Regex(@"Channels(\d+(,\d+)*)\)");
        Match match = regex.Match(fileName);

        List<int> channels = new List<int>();
        if (match.Success)
        {
            // Extract the channel numbers
            string channelPart = match.Groups[1].Value;

            // Split the channel numbers and convert them to integers
            string[] channelNumbers = channelPart.Split(',');
            foreach (string number in channelNumbers)
            {
                if (int.TryParse(number, out int channel))
                {
                    channels.Add(channel);
                }
            }
        }

        return channels;
    }

    public void NewTrack()
    {
        StopAllCoroutines();
        StartCoroutine(ResetTrackCoroutine());

    }
    IEnumerator ResetTrackCoroutine()
    {

        _playback.Stop();
        if (_playback != null)
            _playback.Dispose();
        if (_outputDevice != null)
            _outputDevice.Dispose();


        InitializeOutputDevice();
        ResetTrack();

        yield return new WaitForSeconds(1f);
        InitializeFilePlayback(LoadMidiFileFromStreamingAssets(fileBrowser.OpenSingleFile()));
        yield return new WaitForSeconds(1f);
        StartPlayback();
    }




    private MidiFile LoadMidiFileFromStreamingAssets(string path)
    {
        NoteButtonChannels = ExtractChannels(path);
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

    private ConcurrentQueue<Note> _notesToProcess = new ConcurrentQueue<Note>();


    private void HandleLightsDims()
    {
        foreach (LightsChannel lightsChannel in lightsChannels)
        {
            foreach (VolumetricLightBeamSD light in lightsChannel.volumetricLightBeamHDs)
            {
                if (light.intensityGlobal > gameConfig.lightsMinimumDim)
                    light.intensityGlobal -= gameConfig.lightsDimsSpeed * Time.deltaTime;
                else
                {
                    light.intensityGlobal -= gameConfig.lightsDimsSpeed * Time.deltaTime / 10;
                }
            }
        }

    }

    private void HandleCameraPosition()
    {
        if (cameraTransform != null)
        {
            Vector3 targetPosition = new Vector3(_currentNotePosition.x, cameraTransform.position.y, cameraTransform.position.z);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * 2.0f);  // Adjust speed as necessary
        }
    }
    private void HandleUITime()
    {
        try
        {
            TimeSpan currentTime = ParseTimeSpan(_playback.GetCurrentTime(TimeSpanType.Metric).ToString());
            TimeSpan endTime = ParseTimeSpan(_playback.GetDuration(TimeSpanType.Metric).ToString());
            if (endTime.TotalSeconds > 0) // Ensure there is a duration to avoid division by zero
            {
                float fraction = (float)(currentTime.TotalSeconds / endTime.TotalSeconds);
                timeSlider.value = fraction;
            }
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

    private void InitializeFilePlayback(MidiFile midiFile)
    {
        _playback = midiFile.GetPlayback(_outputDevice);
        _playback.Loop = true;
        _playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
        _playback.NotesPlaybackFinished += OnNotesPlaybackFinished;
        DebugMidiNotes(midiFile);
    }

    public void DebugMidiNotes(MidiFile midiFile)
    {
        try
        {
            TempoMap tempoMap = midiFile.GetTempoMap();
            var notes = midiFile.GetNotes();
            foreach (var note in notes)
            {
                if (NoteButtonChannels.Contains(note.Channel))
                    StartCoroutine(SpawnNoteWithDelay(note, tempoMap));

            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to read MIDI file or process notes: {ex.Message}");
        }
    }
    private float TicksToSeconds(long ticks, TempoMap tempoMap)
    {
        // Get the time in microseconds
        long microseconds = TimeConverter.ConvertTo<MetricTimeSpan>(ticks, tempoMap).TotalMicroseconds;
        return microseconds / 1_000_000f; // Convert microseconds to seconds
    }
    private float lastSeconds;
    IEnumerator SpawnNoteWithDelay(Note note, TempoMap tempoMap)
    {
        if (lastSeconds == TicksToSeconds(note.Time, tempoMap)) yield break;
        float seconds = TicksToSeconds(note.Time, tempoMap);
        lastSeconds = seconds;
        yield return new WaitForSeconds(seconds);
        Debug.Log($"Note: {note.NoteName}, Time: {note.Time}, Length: {note.Length}, Velocity: {note.Velocity}");
        SpawnNote(note);
    }
    private void StartPlayback()
    {
        _playback.Start();
        _playback.Speed = 1f;
    }

    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e)
    {

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
    private void InitializeChannelGradients()
    {
        _channelGradients = new Gradient[16]; // MIDI has 16 channels
        for (int i = 0; i < _channelGradients.Length; i++)
        {
            _channelGradients[i] = new Gradient();
            GradientColorKey[] colorKey = {
                new GradientColorKey(gameConfig.cubeNoteGradientColor1, gameConfig.cubeNoteGradientTime1),
                  new GradientColorKey(gameConfig.cubeNoteGradientColor2, gameConfig.cubeNoteGradientTime2),
                   new GradientColorKey(gameConfig.cubeNoteGradientColor3, gameConfig.cubeNoteGradientTime3),
            };

            GradientAlphaKey[] alphaKey = {
                new GradientAlphaKey(gameConfig.cubeNoteGradientColor1.w,gameConfig.cubeNoteGradientTime1),
                new GradientAlphaKey(gameConfig.cubeNoteGradientColor2.w, gameConfig.cubeNoteGradientTime2),
                new GradientAlphaKey(gameConfig.cubeNoteGradientColor3.w, gameConfig.cubeNoteGradientTime3)
            };
            _channelGradients[i].SetKeys(colorKey, alphaKey);
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




    private void CreateNoteCube(Note note)
    {
        float noteLengthInSeconds = (float)note.LengthAs<MetricTimeSpan>(_playback.TempoMap).TotalMicroseconds / 1000000f;
        float noteHeight = note.NoteNumber - 60 * gameConfig.noteYPositionMultiplier;  // Middle C (C4) is considered as the center line

        GameObject instantiatedNoteCube = Instantiate(NoteCubePrefab, new Vector3(note.TimeAs<MetricTimeSpan>(_playback.TempoMap).TotalMicroseconds / 1000000f, noteHeight, 0), Quaternion.identity);
        instantiatedNoteCube.transform.localScale = Vector3.zero;
        instantiatedNoteCube.transform.DOScale(new Vector3(noteLengthInSeconds, 1 * gameConfig.cubeNoteScale, 1 * gameConfig.cubeNoteScale), gameConfig.cubeNoteScaleTime);  // Scale the cube based on note length
        instantiatedNoteCube.transform.DOLocalJump(instantiatedNoteCube.transform.position, gameConfig.cubeNoteJumpPower, 0, gameConfig.cubeNoteJumpTime);
        instantiatedNoteCube.name = $"Note {note.NoteName}";  // Rename for easier identification
        instantiatedNoteCube.transform.SetParent(cubeParent);


        // Set cube color based on note
        Gradient gradient = _channelGradients[note.Channel];
        float colorPosition = Mathf.InverseLerp(21, 108, note.NoteNumber);
        Color noteColor = gradient.Evaluate(colorPosition);
        instantiatedNoteCube.GetComponent<Renderer>().material.color = noteColor;
        instantiatedNoteCube.GetComponent<Renderer>().material.SetColor("_EmissionColor", noteColor);
        _currentNotePosition = instantiatedNoteCube.transform.position;
        lastVFXPosition = _currentNotePosition;

        float lightIntensity;
        // Normalize the note index and set initial light intensity
        if (note.NoteName.ToString().IndexOf("sharp", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            string updatedNoteName = System.Text.RegularExpressions.Regex.Replace(note.NoteName.ToString(), "sharp", "#", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            channelsNoteText[(int)note.Channel].text = $"CH{note.Channel} {updatedNoteName} {note.Octave}";
   
            lightIntensity = noteParse.NormalizeNoteIndex(updatedNoteName + note.Octave);
        }
        else
        {
            channelsNoteText[(int)note.Channel].text = $"CH{note.Channel} {note.NoteName}{note.Octave}";
            lightIntensity = noteParse.NormalizeNoteIndex(note.NoteName.ToString() + note.Octave);
        }

        foreach (LightsChannel lightsChannel in lightsChannels)
        {
            if (lightsChannel.channel == note.Channel)
            {
                foreach (VolumetricLightBeamSD light in lightsChannel.volumetricLightBeamHDs)
                {

                    DOTween.To(() => light.intensityGlobal, x => light.intensityGlobal = x, lightIntensity * gameConfig.lightsIntencity, gameConfig.lightsTurnOnSpeed);
                    light.spotAngle = gameConfig.lightsAngle;
                    light.color = noteColor;
                }
            }
        }

        if (NoteButtonChannels.Contains(note.Channel))
        {
            cameraController.ShakeCamera(gameConfig.cameraShakeTime, gameConfig.cameraShakePower, shakeCurve);
            cameraController.MoveCamera(new Vector3(0,-2, -30), gameConfig.cameraMoveForwardOnNoteSpawnSpeed, shakeCurve);
        }




            VFXRequest newVfxRequest = new VFXRequest();
        newVfxRequest.Position = _currentNotePosition;
        newVfxRequest.Color = noteColor;
        vFXRequests.Add(newVfxRequest);
    }

    IEnumerator DequeueVfx()
    {
        do
        {
            if (vFXRequests.Count > 0)
            {
                Debug.Log("PlayVfx");
                VFXSpawnCoroutine(vFXRequests[0].Position, vFXRequests[0].Color);
                vFXRequests.Remove(vFXRequests[0]);
            }
            yield return new WaitForSeconds(0.01f);
        } while (true);


    }



    private Vector3 lastVFXPosition;
    private bool waitForFrame;

    public List<VFXRequest> vFXRequests = new List<VFXRequest>();

    [Serializable]
    public class VFXRequest
    {
        public Vector3 Position;
        public Color Color;
    }

    void VFXSpawnCoroutine(Vector3 position, Color noteColor)
    {
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
        VfxSpawnPosition.position = position;
        spawnCubeEffect.Play();
    }





    bool wasOffset;

    public void SpawnNote(Note note)
    {

        GameObject newNoteButton = Instantiate(noteButton, noteButtonParrent.position, Quaternion.identity, noteButtonParrent);
        newNoteButton.GetComponent<NoteButtonController>(); // Attach the NoteButtonController script
        newNoteButton.GetComponent<NoteButtonController>().lifetime = 1.6f;
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