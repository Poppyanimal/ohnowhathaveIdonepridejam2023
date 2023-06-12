using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class lobbybutton : MonoBehaviour
{
    public Image selectedWhite;
    public Image selectedBlack;
    public TMP_Text text;
    Coroutine growShrinkCoro;

    bool hovered = false;

    float maxGrowth = 1.2f;
    float timeToShrinkGrow = .1f;
    float timeToDoSelectEffect = .1f;
    Coroutine selectEffectCoro;

    public void doHoverVisual()
    {  
        hovered = true;
        selectedWhite.gameObject.SetActive(true);
        if(growShrinkCoro != null)
            StopCoroutine(growShrinkCoro);
        growShrinkCoro = StartCoroutine(shrinkGrow(true));
    }

    public void doUnhoverVisual()
    {
        hovered = false;
        selectedWhite.gameObject.SetActive(false);
        if(growShrinkCoro != null)
            StopCoroutine(growShrinkCoro);
        growShrinkCoro = StartCoroutine(shrinkGrow(false));
    }

    public void doSelectEffect()
    {
        if(selectEffectCoro != null)
            StopCoroutine(selectEffectCoro);

        selectEffectCoro = StartCoroutine(selectEffect());
    }

    public IEnumerator selectEffect()
    {
        selectedWhite.color = Color.black;
        selectedBlack.color = Color.white;
        text.color = Color.black;

        yield return new WaitForSeconds(timeToDoSelectEffect);

        selectedWhite.color = Color.white;
        selectedBlack.color = Color.black;
        text.color = Color.white;
    }

    public IEnumerator shrinkGrow(bool isGrowing)
    {
        Vector3 startingScale = gameObject.transform.localScale;
        Vector3 targetScale = Vector3.forward * gameObject.transform.localScale.z + new Vector3(1f, 1f, 0f) * (isGrowing ? maxGrowth : 1f);

        Vector3 scaleDif = targetScale - startingScale;

        float timeStart = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - timeStart) / timeToShrinkGrow;
            if(timeRatio >= 1f)
            {
                gameObject.transform.localScale = targetScale;
                return true;
            }
            else
            {
                gameObject.transform.localScale = startingScale + scaleDif * timeRatio;
                return false;
            }
        });
    }
    

}
