using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerSplash : MonoBehaviour
{
    public GameObject yukiSplash, yukiSplashShadow, maiSplash, maiSplashShadow;
    public float timeToArrive = .5f;
    public Vector3 startingPos;
    Vector3 endPosYuki, endPosYukiShado, endPosMai, endPosMaiShado;
    bool gameRunning = false;
    Coroutine movementCoro;


    void Start()
    {
        endPosYuki = yukiSplash.transform.localPosition;
        endPosYukiShado = yukiSplashShadow.transform.localPosition;
        endPosMai = maiSplash.transform.localPosition;
        endPosMaiShado = maiSplashShadow.transform.localPosition;
        gameRunning = true;
    }

    
    [ContextMenu("Switch to Yuki")]
    public void switchToYuki()
    {
        if(!gameRunning)
            return;

        if(movementCoro != null)
            StopCoroutine(movementCoro);

        movementCoro = StartCoroutine(moveInSplash(true));
    }

    [ContextMenu("Switch to Mai")]
    public void switchToMai()
    {
        if(!gameRunning)
            return;
        
        if(movementCoro != null)
            StopCoroutine(movementCoro);

        movementCoro = StartCoroutine(moveInSplash(false));
    }

    IEnumerator moveInSplash(bool isYuki)
    {
        yukiSplash.transform.localPosition = startingPos;
        yukiSplashShadow.transform.localPosition = startingPos;
        maiSplash.transform.localPosition = startingPos;
        maiSplashShadow.transform.localPosition = startingPos;

        yukiSplash.SetActive(isYuki);
        yukiSplashShadow.SetActive(isYuki);
        maiSplash.SetActive(!isYuki);
        maiSplashShadow.SetActive(!isYuki);

        Vector3 yukiPosDif = endPosYuki - startingPos;
        Vector3 yukiShadPosDif = endPosYukiShado - startingPos;
        Vector3 maiPosDif = endPosMai - startingPos;
        Vector3 maiShadPosDif = endPosMaiShado - startingPos;

        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / timeToArrive;
            if(timeRatio >= 1f)
            {
                yukiSplash.transform.localPosition = endPosYuki;
                yukiSplashShadow.transform.localPosition = endPosYukiShado;
                maiSplash.transform.localPosition = endPosMai;
                maiSplashShadow.transform.localPosition = endPosMaiShado;
                return true;
            }
            else
            {
                yukiSplash.transform.localPosition = startingPos + yukiPosDif * timeRatio;
                yukiSplashShadow.transform.localPosition = startingPos + yukiShadPosDif * timeRatio;
                maiSplash.transform.localPosition = startingPos + maiPosDif * timeRatio;
                maiSplashShadow.transform.localPosition = startingPos + maiShadPosDif * timeRatio;
                return false;
            }
        });
    }

}
