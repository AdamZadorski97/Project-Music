using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public GameConfig settings = new GameConfig();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings(); // Optionally load settings on awake
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(Application.persistentDataPath + "/settings.json", json);
        Debug.Log("Settings saved");
    }

    public void LoadSettings()
    {
        string path = Application.persistentDataPath + "/settings.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            settings = JsonUtility.FromJson<GameConfig>(json);
            Debug.Log("Settings loaded");
        }
        else
        {
            settings.ResetToDefault();
            SaveSettings();  // Save the default settings automatically
            Debug.Log("No settings file found, default settings loaded and saved");
        }
    }
    [Button]
    public void ResetSettings()
    {
        settings.ResetToDefault();
        SaveSettings();
    }
}
