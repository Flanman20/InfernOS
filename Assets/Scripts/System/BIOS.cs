using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BIOS : MonoBehaviour
{
    public History history;
    public Database database;
    public TMP_InputField inp;
    public TMP_Text output;
    Saving saving;
    public Progression progression;
    EdgeCases edgeCases;
    public AudioManager audioManager;
    public GameObject settings;
    public bool loaded = false;
    bool ignore = false; //allows some cases to return to Update() without displaying an error message like unrecognized input
    int backIndex = 1;
    private bool skipHistoryAdd = false;
    private int currentHistoryIndex = -1;
    public Entry currentEntry;
    private void Awake()
    {
        settings.SetActive(true);
        progression = GetComponent<Progression>();
        edgeCases = GetComponent<EdgeCases>();
        saving = GetComponent<Saving>();
        audioManager = GetComponent<AudioManager>();
    }

    private void Start()
    {
        Debug.Log("Started!");
        settings.SetActive(false);
        loaded = false;
        if (saving == null)
        {
            Debug.LogError("Saving component not found!");
            return;
        }

        try
        {
            saving.Load();
            loaded = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            loaded = false;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            Back();      

        inp.ActivateInputField();

        if (Input.GetKeyDown(KeyCode.Return) && inp.IsActive())
        {
            if (!inp.text.StartsWith("*settings"))
            {
                settings.SetActive(false);
            }

            if (ignore)
            {
                ignore = false;
                output.text = "Welcome to <color=yellow>InfernOS</color>! Type *help to get started.";
                return;
            }



            if (progression.CheckPasswords(inp.text))
            {
                return;
            }


            switch (inp.text.Trim())
            {
                case string i when i.Contains("*help"): Help(); break;
                case string i when i.Contains("*ask"): Ask(inp.text.Replace("*ask", "").Replace(" ", "").ToLower().Trim()); break;
                case string i when i.Contains("*history"): History(inp.text); break;
                case string i when i.Contains("*time"): Time(); break;
                case string i when i.Contains("*settings"): Settings(); break;
                case string i when i.Contains("*back"): Back(); break;
                case string i when i.Contains("*exit"): Exit(); break;
                default: output.text = "Unrecognized input. Type *help for list of commands"; break;
            }
        }
    }
    void Help()
    {
        output.text = 
            "*help - shows list of commands<br>" +
            "*exit - exit program and save all memory<br>" +
            "*settings - change program settings<br>" +
            "*time - shows current time<br>" +
            "*history - shows previous valid entries<br>" +
            "*back - goes to the previous valid entry in history<br>" +
            "*ask - takes query and searches <color=yellow>entries</color> for response<br>" +
            "<color=#006400ff>(e.g. *ask entries or *ask Where am I?)</color><br>";
    }


    void Ask(string input, bool skipFoundSound = false)
    {
        SlowType slowType = new SlowType();
        Debug.Log(input);

        //done here to stop slowtype if trying to get to other entry
        slowType.StopType(this);

        input = Regex.Replace(input, @"[^a-zA-Z0-9\s\+]", "");

        edgeCases.HandleEdgeCases(input.Trim());

        if (edgeCases.nullOrAsk)
        {
            edgeCases.nullOrAsk = false;
            return;
        }

        output.text = string.Empty;

        bool found = false;
        foreach (var entry in database.entries)
        {
            string[] names = entry.name.Split(',');
            foreach (var name in names)
            {
                if (input.Equals(name.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    currentEntry = entry;

                    if (!skipFoundSound)
                        audioManager.HandleAudio(entry);

                    if (inp.text.Contains("*ask")) 
                        AddHistory();

                    EntryTracker.MarkAsFound(entry.name);
                    string processedOutput = entry.ProcessOutput(this);
                    string underlinedEntry = Highlight.UnderlineEntries(processedOutput);
                    string highlightedEntry = Highlight.HighlightEntries(underlinedEntry, database);

                    if (entry.progressionLevel > progression.progressionLevel)
                    {
                        if (entry.entryType == Entry.type.Corrupted)
                        {
                            string corruptedEntry = Highlight.CorruptText(highlightedEntry);
                            output.text = corruptedEntry;
                            return;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(entry.altOutput) || entry.minProgressionLevel > progression.progressionLevel)
                            {
                                if (progression.progressionLevel < entry.minProgressionLevel)
                                {
                                    output.text = $"<color=red><ENTRY REDACTED></color><br><color=#FF000088>Protection level: {entry.minProgressionLevel}</color><br><br><color=green>Please type a <color=yellow>password</color> to increase security clearence.</color>";
                                }
                                else
                                {
                                    output.text = $"<color=red><ENTRY REDACTED></color><br><color=#FF000088>Protection level: {entry.progressionLevel}</color><br><br><color=green>Please type a <color=yellow>password</color> to increase security clearence.</color>";
                                }
                                return;
                            }
                            else
                            {
                                string processedAltOutput = entry.ProcessAltOutput(this);
                                underlinedEntry = Highlight.UnderlineEntries(processedAltOutput);
                                highlightedEntry = Highlight.HighlightEntries(underlinedEntry, database);
                                output.text = highlightedEntry;
                                return;
                            }
                        }
                    } else if (entry.progressionLevel <= progression.progressionLevel && entry.entryType == Entry.type.Corrupted)
                    {
                        highlightedEntry = Highlight.RemoveTags(highlightedEntry);
                    }

                    switch (entry.entryType)
                    {
                        case Entry.type.Normal: output.text = highlightedEntry; break;
                        case Entry.type.Corrupted: output.text = highlightedEntry; break;
                        case Entry.type.SlowType: slowType.TypeLine(highlightedEntry, entry.slowTypeSpeed, output, this, database, true); break;
                    }
                    return;
                }
            }
        }
        if (!found)
            output.text = "Couldn't find entry";
    }

    void History(string inp)
    {
        if (inp.Contains("clear"))
        {
            Debug.Log("cleared!");
            history.entries.Clear();
            Debug.Log(history.entries.Count());
        }

        if (history.entries.Count() > 0)
        {
            foreach (string i in history.entries)
            {
                output.text = string.Join("\n", history.entries);
            }

            output.text += string.Concat(Enumerable.Repeat("\n", 19 - history.entries.Count())) + "<color=#006400ff>Use *history clear to empty history";
        }
        else
        {
            output.text = "<color=#006400ff>History Empty.";
        }

        currentHistoryIndex = history.entries.Count;
       
    }
    void AddHistory()
    {
        if (!string.IsNullOrWhiteSpace(inp.text) && !inp.text.Contains("*history"))
        {
            if (history.entries.Count >= 10)
            {
                history.entries.RemoveAt(0);
            }

            if (history.entries.Count == 0 || history.entries.Last() != inp.text)
            {
                history.entries.Add(inp.text);
            }

            // Update currentHistoryIndex to point to the latest (active) entry.
            currentHistoryIndex = history.entries.Count - 1;
        }
    }

    void Time()
    {
        if (progression.timeUpdated)
        {
            SlowType slowType = new SlowType();
            slowType.TypeLine("<color=yellow>LINK</color> Uptime Hours: 124808942189471289471289472189412412904612904612904621704621790461249021648902167426175621795130683918562917521653782605386753217538125732189056321590632178905632197506321905631279056312790563278916537890216537921806537921856307165397021653970126539071650928947218941241290461290461290462170462179046124902164890216742617512480894218947128947128947218941241290461290461290462170462179046124902164890216742617562179513068391856291752165378260538675321753812573218905632159063217890563219750632190563127905631279056327891653789021653792180653792185630716539702165397012653907165092894721894124129046129046129046217046217904612490216489021674261751248089421894712894712894721894124129046129046129046217046217904612490216489021674261756217951306839185629175216537826053867532175381257321890563215906321789056321975063219056312790563127905632789165378902165379218065379218563071653970216539701265390716509289472189412412904612904612904621704621790461249021648902167426175", 0.00001f, output, this, database);
        }
        else
        {
            output.text = System.DateTime.Now.ToString() + " in World Time Units.";
        }

    }

    void Settings()
    {
        settings.SetActive(true);
        output.text = "\r\n\r\nSound\r\n-------------------------------------------------------------------------\r\n\r\nAmbient: \r\n\r\n\r\nFX:";
    }
    public void Back()
    {
        if (history.entries == null || history.entries.Count == 0)
        {
            Debug.LogWarning("History is empty. Cannot go back.");
            output.text = "<color=#006400ff>History Empty. Cannot go back.</color>";
            return;
        }

        // Determine the index of the previous entry relative to currentHistoryIndex.
        int targetIndex = currentHistoryIndex - 1;
        string currentEntryName = currentEntry != null
            ? currentEntry.name.Replace(" ", "").ToLower()
            : string.Empty;

        // Skip entries that match the current entry.
        while (targetIndex >= 0)
        {
            string candidate = history.entries[targetIndex]
                .Replace("*ask ", "")
                .Replace(" ", "")
                .Trim()
                .ToLower();
            if (candidate == currentEntryName)
            {
                targetIndex--;
            }
            else
            {
                break;
            }
        }

        if (targetIndex < 0)
        {
            skipHistoryAdd = true;
            audioManager.PlaySound(audioManager.errorSound2);
            Ask(history.entries[0].Replace("*ask ", "").Replace(" ", "").Trim().ToLower(), true);
            skipHistoryAdd = false;
            currentHistoryIndex = 0;
            output.text += "\n\n<color=#006400ff>Reached the beginning of history.</color>";
            return;
        }

        // Load the found previous entry.
        string targetEntry = history.entries[targetIndex]
            .Replace("*ask ", "")
            .Replace(" ", "")
            .Trim()
            .ToLower();
        skipHistoryAdd = true;
        Ask(targetEntry);
        skipHistoryAdd = false;
        currentHistoryIndex = targetIndex;
    }


    public void Exit()
    {
        try
        {
            output.text = "Saving";
            saving.Save();
            output.text = "Saved!";
            output.text = "Qutting program. Goodbye!";
            Application.Quit();
        }
        catch (Exception e)
        {
            //lord in the heavens above and to all who read this i apologize with all of my soul. I now understand why Hesam Akbari gave me a 5/10 on code handling.
            Debug.LogException(e);
            output.text = "<color=red>Could not properly save program. Press enter to return to the main console.";
            inp.text = "";
            ignore = true;
            return;
        }
    }
}