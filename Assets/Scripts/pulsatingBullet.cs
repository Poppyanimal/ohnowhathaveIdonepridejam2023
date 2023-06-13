using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pulsatingBullet : MonoBehaviour
{
    public float maxGrowthPercent;
    public float halfCycleTime;
    

    void Start()
    {
        StartCoroutine(doPulse());
    }

    IEnumerator doPulse()
    {
        float startingScale = gameObject.transform.localScale.x;
        Vector3 vecToScale = new Vector3(1f,1f,0f);

        while(true)
        {
            float startTime = Time.time;
            yield return new WaitUntil(delegate()
            {
                float timeRatio = (Time.time - startTime) / halfCycleTime;
                if(timeRatio >= 1f)
                {
                    gameObject.transform.localScale = vecToScale * startingScale * (1f + maxGrowthPercent) + Vector3.forward;
                    return true;
                }
                else
                {
                    gameObject.transform.localScale = vecToScale * startingScale * (1f + maxGrowthPercent * timeRatio) + Vector3.forward;
                    return false;
                }
            });

            startTime = Time.time;
            yield return new WaitUntil(delegate()
            {
                float timeRatio = (Time.time - startTime) / halfCycleTime;
                if(timeRatio >= 1f)
                {
                    gameObject.transform.localScale = vecToScale * (startingScale) + Vector3.forward;
                    return true;
                }
                else
                {
                    gameObject.transform.localScale = vecToScale * startingScale * (1f + maxGrowthPercent * (1f - timeRatio)) + Vector3.forward;
                    return false;
                }
            });
        }
    }
}
