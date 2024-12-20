using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    // set these in the inspector
    [Tooltip("Reference to Image component on child panel")]
    public Image fadeImage;

    [Tooltip("Color to use during scene transition")]
    public Color fadeColor = Color.black;

    [Range(0.5f, 100.0f), Tooltip("Rate of fade in / out: higher is faster")]
    public float stepRate = 2;

    float step;

    WaitForSeconds waitForSeconds;

    WaitForSeconds waitOneSeconds = new WaitForSeconds(1f);

    void OnValidate()
    {
        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>();
    }

    void Start()
    {
        // Convert user-friendly setting value to working value
        step = stepRate;
        waitForSeconds = new WaitForSeconds(1 / step);
    }

    public WaitForSeconds GetWaitSeconds()
    {
        return waitForSeconds;
    }

    public IEnumerator FadeIn()
    {
        float alpha = fadeImage.color.a;
        while (alpha < 1)
        {
            yield return null;
            alpha += step * Time.deltaTime;
            fadeColor.a = alpha;
            fadeImage.color = fadeColor;
        }
    }

    public IEnumerator FadeOut()
    {
        float alpha = fadeImage.color.a;

        yield return waitOneSeconds;

        while (alpha > 0)
        {
            yield return null;
            alpha -= step * Time.deltaTime;
            fadeColor.a = alpha;
            fadeImage.color = fadeColor;
        }
    }
}
