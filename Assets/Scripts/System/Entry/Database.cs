using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QueryDatabase", menuName = "InfernOS/Query Database")]
public class Database : ScriptableObject
{
    [SerializeField]
    public List<Entry> entries = new List<Entry>();

    public Entry FindEntry(string name)
    {
        foreach (Entry entry in entries)
        {
            string[] names = entry.name.Split(',');
            foreach (var i in names)
            { 
                if (entry.name.Contains(i))
                {
                    return entry;
                }
            }
        }
        return null;
    }
}

