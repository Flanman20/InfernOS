using System.Collections;
using TMPro;
using UnityEngine;

public class Progression : MonoBehaviour
{
    [SerializeField] public PasswordDatabase passwords;
    [SerializeField] public TMP_Text output;
    [SerializeField] public GameObject input;
    [SerializeField] public AudioSource source;
    [SerializeField] public AudioClip success;
    [SerializeField] public AudioClip failure;

    // Data we want to save
    public int progressionLevel = 0;
    public bool seenHell = false;
    public bool seenDisruption = false;
    public bool timeUpdated = false;
    private bool found = false;

    // Method to get serializable data
    public ProgressionData GetSaveData()
    {
        var data = new ProgressionData();
        data.progressionLevel = this.progressionLevel;
        data.seenHell = this.seenHell;
        data.seenDisruption = this.seenDisruption;
        data.timeUpdated = this.timeUpdated;
        return data;
    }

    // Method to load from serializable data
    public void LoadFromData(ProgressionData data)
    {
        this.progressionLevel = data.progressionLevel;
        this.seenHell = data.seenHell;
    }
    public bool CheckPasswords(string input)
    {
        if (passwords == null)
        {
            Debug.LogError("PasswordDatabase not assigned!");
            passwords = new PasswordDatabase();
            return false;
        }

        Debug.Log("Password called");
        foreach (var i in passwords.entries)
        {
            if (input.ToLower().Contains(i.password) && !found)
            {
                StartCoroutine(UpdateCutscene(i));
                found = true;
            }
        }
        Debug.Log("found: " + found);
        return found;
    }

    IEnumerator UpdateCutscene(Password password)
    {
        if (input == null || output == null || source == null)
        {
            Debug.LogError("Required components not assigned!");
            yield break;
        }

        input.SetActive(false);
        output.text = "<color=green>Confirming Password</color>";
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(Random.Range(1f, 2f));
            output.text += ".";
        }

        if (password.isUsed)
        {
            output.text = "<color=red>Password Already Used.</color>";
            source.PlayOneShot(failure);
            input.SetActive(true);
            found = false;
            yield break;
        }
        else
        {
            password.isUsed = true;
            progressionLevel++;
            output.text = "<color=green>Access Granted.</color>";
            input.SetActive(true);
            source.PlayOneShot(success);
            found = false;
            yield break;
        }
    }
}