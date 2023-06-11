using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class makaiareatitle : MonoBehaviour
{
    public GameObject daLine;
    public Vector3 lineRetractedPos, lineExtendedPos;
    public TMP_Text makaiTitleJP, makaiSubJP, makaiTitleEng, makaiSubEng;
    List<TMP_Text> allText = new();
    public float delayBeforeStart = 1.2f;
    public float delayBeforeTitle = .2f;
    public float delayBeforeSubtitle = .3f;
    public float fadeTime = .2f;
    public float lineExtensionTime = .3f;
    public float holdTime = 1f;
    public float waitBeforeLineRetreats = .2f;
    public float delayBeforeBells = .35f;

    Coroutine titleFade;
    Coroutine subTitleFade;


    void Start()
    {
        allText.Add(makaiTitleJP);
        allText.Add(makaiSubJP);
        allText.Add(makaiTitleEng);
        allText.Add(makaiSubEng);
        for(int i = 0; i < allText.Count; i++)
        {
            Color c = allText[i].color;
            c.a = 0f;
            allText[i].color = c;
        }

        daLine.transform.localPosition = lineRetractedPos;

        StartCoroutine(doAnims());
    }

    IEnumerator bellsOnDelay()
    {
        yield return new WaitForSeconds(delayBeforeBells);
        stageSFXHandler.Singleton.bells.playSFX();
    }

    IEnumerator doAnims()
    {
        yield return new WaitForSeconds(delayBeforeStart);


        titleFade = StartCoroutine(titleFadeIn());
        subTitleFade = StartCoroutine(subTitleFadeIn());
        StartCoroutine(bellsOnDelay());

        Vector3 posDif = lineExtendedPos - lineRetractedPos;
        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / lineExtensionTime;
            if(timeRatio >= 1f)
            {
                daLine.transform.localPosition = lineExtendedPos;
                return true;
            }
            else
            {
                daLine.transform.localPosition = lineRetractedPos + posDif * timeRatio;
                return false;
            }
        });

        yield return new WaitForSeconds(holdTime);

        if(titleFade != null)
            StopCoroutine(titleFade);
        titleFade = StartCoroutine(titleFadeOut());
        if(subTitleFade != null)
            StopCoroutine(subTitleFade);
        subTitleFade = StartCoroutine(subTitleFadeOut());

        yield return new WaitForSeconds(waitBeforeLineRetreats);

        startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / lineExtensionTime;
            if(timeRatio >= 1f)
            {
                daLine.transform.localPosition = lineRetractedPos;
                return true;
            }
            else
            {
                daLine.transform.localPosition = lineExtendedPos - posDif * timeRatio;
                return false;
            }
        });

    }

    IEnumerator titleFadeIn()
    {
        yield return new WaitForSeconds(delayBeforeTitle);

        Color col = Color.white;
        col.a = 0f;

        float timeStart = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - timeStart) / fadeTime;
            col.a = timeRatio;
            if(timeRatio >= 1f)
            {
                col.a = 1f;
                makaiTitleJP.color = col;
                makaiTitleEng.color = col;
                return true;
            }
            else
            {
                makaiTitleJP.color = col;
                makaiTitleEng.color = col;
                return false;
            }
        });

    }

    IEnumerator titleFadeOut()
    {
        Color col = Color.white;
        col.a = 1f;

        float timeStart = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - timeStart) / fadeTime;
            col.a = 1f - timeRatio;
            if(timeRatio >= 1f)
            {
                col.a = 0f;
                makaiTitleJP.color = col;
                makaiTitleEng.color = col;
                return true;
            }
            else
            {
                makaiTitleJP.color = col;
                makaiTitleEng.color = col;
                return false;
            }
        });
    }

    IEnumerator subTitleFadeIn()
    {
        yield return new WaitForSeconds(delayBeforeTitle + delayBeforeSubtitle);
        
        Color col = Color.white;
        col.a = 0f;

        float timeStart = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - timeStart) / fadeTime;
            col.a = timeRatio;
            if(timeRatio >= 1f)
            {
                col.a = 1f;
                makaiSubJP.color = col;
                makaiSubEng.color = col;
                return true;
            }
            else
            {
                makaiSubJP.color = col;
                makaiSubEng.color = col;
                return false;
            }
        });

    }

    IEnumerator subTitleFadeOut()
    {
        Color col = Color.white;
        col.a = 1f;

        float timeStart = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - timeStart) / fadeTime;
            col.a = 1f - timeRatio;
            if(timeRatio >= 1f)
            {
                col.a = 0f;
                makaiSubJP.color = col;
                makaiSubEng.color = col;
                return true;
            }
            else
            {
                makaiSubJP.color = col;
                makaiSubEng.color = col;
                return false;
            }
        });
    }

}
