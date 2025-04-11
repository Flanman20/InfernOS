using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class History : MonoBehaviour
{
    [SerializeField]
    public List<string> entries = new List<string>();
    private void Update()
    {
        if(entries.Count > 10)
        { 
            entries.RemoveAt(0);
        }
    }
}
