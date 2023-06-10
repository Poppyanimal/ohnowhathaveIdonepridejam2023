using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainMenuBGtrans : MonoBehaviour
{
    public Material mmMat;
    public float redHue = 4.1f;
    public float blueHue = 2.1f;
    public float timeBetweenTransitions = 2f;
    public float timeForTransition = .5f;

    void Start()
    {
        mmMat.SetFloat("_HueShift", redHue);
        StartCoroutine(transitionEffect());
    }

    IEnumerator transitionEffect()
    {
        while(true)
        {
            yield return new WaitForSeconds(timeBetweenTransitions);

            float startTime = Time.time;
            float hueDif = blueHue - redHue;
            //down from red to blue
            yield return new WaitUntil(delegate()
            {
                float timeRatio = (Time.time - startTime) / timeForTransition;
                if(timeRatio >= 1f)
                {
                    mmMat.SetFloat("_HueShift", blueHue);
                    return true;
                }
                else
                {
                    mmMat.SetFloat("_HueShift", redHue + hueDif * timeRatio);
                    return false;
                }
            });

            yield return new WaitForSeconds(timeBetweenTransitions);

            startTime = Time.time;
            hueDif = redHue - blueHue;
            //up from blue to red
            yield return new WaitUntil(delegate()
            {
                float timeRatio = (Time.time - startTime) / timeForTransition;
                if(timeRatio >= 1f)
                {
                    mmMat.SetFloat("_HueShift", redHue);
                    return true;
                }
                else
                {
                    mmMat.SetFloat("_HueShift", blueHue + hueDif * timeRatio);
                    return false;
                }
            });
        }
    }
}
