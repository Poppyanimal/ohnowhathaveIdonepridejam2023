using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explosiongrowshrink : MonoBehaviour
{
    public float maxScale = 1f;
    public float timeToGrow = .1f;
    public float timeToDissipate = .2f;

    void Start()
    {
        StartCoroutine(doThing());
    }

    IEnumerator doThing()
    {
        gameObject.transform.localScale = new Vector3(0f, 0f, gameObject.transform.localScale.z);

        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / timeToGrow;
            if(timeRatio >= 1f)
            {
                gameObject.transform.localScale = new Vector3(1f,1f,0f) * maxScale + Vector3.forward * gameObject.transform.localScale.z;
                return true;
            }
            else
            {
                gameObject.transform.localScale = new Vector3(1f,1f,0f) * maxScale * timeRatio + Vector3.forward * gameObject.transform.localScale.z;
                return false;
            }
        });

        
        startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / timeToGrow;
            if(timeRatio >= 1f)
            {
                gameObject.transform.localScale = Vector3.zero + Vector3.forward * gameObject.transform.localScale.z;
                return true;
            }
            else
            {
                gameObject.transform.localScale = new Vector3(1f,1f,0f) * maxScale * (1f - timeRatio) + Vector3.forward * gameObject.transform.localScale.z;
                return false;
            }
        });


        Destroy(this.gameObject);
    }

}
