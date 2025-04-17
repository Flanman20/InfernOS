using System.Collections;
using TMPro;
using UnityEngine;
public class SlowType
{
    public void StopType(MonoBehaviour monoBehaviour)
    {
        monoBehaviour.StopAllCoroutines();
    }
    public void TypeLine(string message, float speed, TMP_Text output, MonoBehaviour monoBehaviour, Database database, bool Randomize = false)
    {
        monoBehaviour.StopAllCoroutines();
        monoBehaviour.StartCoroutine(LinePrint(message, speed, output, database, Randomize));
    }
    public static IEnumerator LinePrint(string message, float speed, TMP_Text output, Database database, bool Randomize)
    {
        bool insideParenthesis = false;
        string typedText = string.Empty; // Holds the visible text as it is typed.

        for (int i = 0; i < message.Length; i++)
        {
            char currentChar = message[i]; 
            if (currentChar == '<')
            {
                insideParenthesis = true;
            }
            else if (currentChar == '>')
            {
                insideParenthesis = false;
            }
            typedText += message[i];

            // Check if the typed text contains any entries and apply highlighting.
            string highlightedText = Highlight.HighlightEntries(typedText, database);

            output.text = highlightedText;
            if (!insideParenthesis)
            {
                
                if (Randomize)
                {
                    float newSpeed = 0;
                    newSpeed= speed*(1 + Random.Range(-0.05f, 0.05f));
                    yield return new WaitForSeconds(newSpeed);
                }
                else {
                    yield return new WaitForSeconds(speed);
                }
                
            }
        }
    }

}