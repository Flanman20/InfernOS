using UnityEngine;
using System.IO;
using System;
public class Saving : MonoBehaviour
{
    public Database database;
    public History history;
    public PasswordDatabase passwords;
    public Progression progression;

    string historyPath;
    string passwordPath;
    string savesDirectory;
    string progressionPath;
    public void Awake()
    {
        savesDirectory = Path.Combine(Application.persistentDataPath, "saves");
        historyPath = Path.Combine(savesDirectory, "history.json");
        passwordPath = Path.Combine(savesDirectory, "passwords.json");
        progressionPath = Path.Combine(savesDirectory, "progression.json");

    }
    public void Save()
    {
        if (!Directory.Exists(savesDirectory))
        {
            Directory.CreateDirectory(savesDirectory);
        }

        string historySave = JsonUtility.ToJson(history);
        string passwordSave = JsonUtility.ToJson(passwords);

        // Create and save progression data
        ProgressionData progressionData = progression.GetSaveData();
        string progressionSave = JsonUtility.ToJson(progressionData);

        File.WriteAllText(historyPath, historySave);
        File.WriteAllText(passwordPath, passwordSave);
        File.WriteAllText(progressionPath, progressionSave);
    }

    public void Load()
    {
        try
        {
            // Create directories if they don't exist
            if (!Directory.Exists(savesDirectory))
            {
                Directory.CreateDirectory(savesDirectory);
                Save(); // This will create initial empty files
                return;
            }

            // Load history and passwords
            if (File.Exists(historyPath) && File.Exists(passwordPath))
            {
                string historyJson = File.ReadAllText(historyPath);
                string passwordJson = File.ReadAllText(passwordPath);

                JsonUtility.FromJsonOverwrite(historyJson, history);
                JsonUtility.FromJsonOverwrite(passwordJson, passwords);
            }

            // Load progression separately
            if (File.Exists(progressionPath))
            {
                Debug.Log(progressionPath);
                string progressionJson = File.ReadAllText(progressionPath);
                ProgressionData data = JsonUtility.FromJson<ProgressionData>(progressionJson);
                progression.LoadFromData(data);
                Debug.Log("Loaded progression level: " + progression.progressionLevel);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading save files: {e.Message}");
            Save();
        }
    }
}