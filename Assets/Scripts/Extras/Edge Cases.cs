using System.Collections;
using TMPro;
using UnityEngine;

public class EdgeCases : MonoBehaviour
{
    public TMP_Text output;
    public bool nullOrAsk = false;
    BIOS bios;
    Progression progression;
    public void HandleEdgeCases(string input)
    {
        progression = GetComponent<Progression>();
        bios = GetComponent<BIOS>();
        if (string.IsNullOrEmpty(input))
        {
            output.text = "Please also include an <color=yellow>entry</color><br><color=green>e.g. *ask where am I?</color>";
            nullOrAsk = true;
        }

        if (input.Contains("hell") && progression.seenHell == false && !input.Contains("hello") && progression.progressionLevel == 0)  
        {
            progression.seenHell = true;
            bios.audioManager.backgroundSource.Stop();
            Debug.Log("should stop now");
            StartCoroutine(Wait(5f));
        }
    }   
    //real shit code but idk ill just kms or smth
    IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        bios.audioManager.backgroundSource.volume = 0;
        bios.audioManager.backgroundSource.Play();
        StartCoroutine(bios.audioManager.FadeIn(bios.audioManager.backgroundSource, 0.001f));
        StopCoroutine(Wait(seconds));
    }
}
