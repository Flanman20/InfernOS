using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
public class Startup : MonoBehaviour
{
    public AudioClip finished;
    public AudioSource audioSource;
    public GameObject input;
    public TMP_Text output;
    public List<string> lines = new List<string>();
    public BIOS bios;
    bool running = false;
    public void Start()
    {
        input.SetActive(false);
        audioSource.Play();
        StartCoroutine(TypeDots());
    }
    private void Update()
    {
        if (running && Input.GetKeyDown(KeyCode.Return)) {
            running = false;
            StopAllCoroutines();
            audioSource.Stop();
            audioSource.PlayOneShot(finished);
            input.SetActive(true);
            output.text = "Welcome to <color=yellow>InfernOS</color>! Type *help to get started.";
        }
    } 
    public IEnumerator TypeDots()
    {
        running = true;
        string path = Path.Combine(Application.persistentDataPath, "saves");
        int count = 0; //i used this to like do the load screen after a few itterations
        foreach (var line in lines) { 
            output.text = line;
            count++;
            if (count == 3)
            {
                output.text = "Loading Data";
                for (int i = 0; i < 4; i++)
                {
                    yield return new WaitForSeconds(Random.Range(1f, 2f));
                    output.text += ".";
                }
                if (!Directory.Exists(path)) 
                {
                    output.text = "<color=red>Save not found. Creating new instance.";
                    yield return new WaitForSeconds(3f);
                }
                else if (bios.loaded == false && Directory.Exists(path))
                {
                    output.text = "<color=red>Could not load save.";
                    yield return new WaitForSeconds(3f);
                }
                else if (bios.loaded == true)
                {
                    output.text = "<color=green>Loaded Save.</color>";
                    yield return new WaitForSeconds(3f);
                } 

            } else 
            {
                for (int i = 0; i < 4; i++)
                {
                    yield return new WaitForSeconds(Random.Range(1f, 2f));
                    output.text += ".";
                }
            }

        }
        audioSource.Stop();
        audioSource.PlayOneShot(finished);
        output.text = "Welcome to <color=yellow>InfernOS</color>! Type *help to get started.";
        input.SetActive(true);
        running = false;
    }
}

