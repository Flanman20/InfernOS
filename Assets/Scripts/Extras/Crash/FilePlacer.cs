using System.IO;
using UnityEngine;

public class FilePlacer : MonoBehaviour
{
    public string zipFileName = "entries.zip";

    void Start()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string destinationPath = Path.Combine(desktopPath, zipFileName);

        if (!File.Exists(destinationPath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, zipFileName);

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destinationPath);
                Debug.Log("Zip file placed on Desktop.");
            }
            else
            {
                Debug.LogWarning("Source file does not exist in the specified path.");
            }
        }
        else
        {
            Debug.Log("Zip file already exists on Desktop.");
        }
    }
}