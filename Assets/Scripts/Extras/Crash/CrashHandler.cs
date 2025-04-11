using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrashHandler : MonoBehaviour
{
    public RawImage crashImage;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeIn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator FadeIn()
    {
        float duration = 0.2f;
        float targetAlpha = 0.9f;
        float currentAlpha = 0f;

        while (currentAlpha < targetAlpha)
        {
            currentAlpha += Time.deltaTime / duration;
            crashImage.color = new Color(
                crashImage.color.r,
                crashImage.color.g,
                crashImage.color.b,
                Mathf.Clamp(currentAlpha, 0f, 0.9f)
            );
            yield return null;
        }
    }
}
