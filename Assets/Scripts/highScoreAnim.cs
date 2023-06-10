using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class highScoreAnim : MonoBehaviour
{
    public float cycleTime = 3f;
    public float startingS, startingV = .5f;
    TMP_Text thisText;

    void Start()
    {
        thisText = gameObject.GetComponent<TMP_Text>();
        StartCoroutine(cycleEffect());
    }

    IEnumerator cycleEffect()
    {
        while(true)
        {
            float startTime = Time.time;
            yield return new WaitUntil(delegate()
            {
                float timeRatio = (Time.time - startTime) / cycleTime;
                if(timeRatio >= 1)
                {
                    thisText.color = Color.HSVToRGB(1f, startingS, startingV);
                    return true;
                }
                else
                {
                    thisText.color = Color.HSVToRGB(timeRatio, startingS, startingV);
                    return false;
                }
            });
        }
    }

}
