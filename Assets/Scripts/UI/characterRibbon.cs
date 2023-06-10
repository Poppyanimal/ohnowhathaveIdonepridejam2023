using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterRibbon : MonoBehaviour
{
    public GameObject ribbonObj;
    public GameObject yukiPart, maiPart, readyPart;
    public float distanceToMove = 160f;
    public float timePerLoop = 4f;
    public bool movingLeft = false;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(doMovementLoop());
    }

    IEnumerator doMovementLoop()
    {
        Vector3 startingPos = ribbonObj.transform.localPosition;
        while(true)
        {
            float startTime = Time.time;
            yield return new WaitUntil(delegate()
            {
                float timeRatio = (Time.time - startTime) / timePerLoop;
                if(timeRatio >= 1f)
                {
                    ribbonObj.transform.localPosition = startingPos;
                    return true;
                }
                else
                {
                    ribbonObj.transform.localPosition = startingPos + Vector3.right * timeRatio * distanceToMove * (movingLeft ? -1 : 1);
                    return false;
                }
            });
        }
    }

    public void selectYuki()
    {
        yukiPart.SetActive(true);
        maiPart.SetActive(false);
    }

    public void selectMai()
    {
        maiPart.SetActive(true);
        yukiPart.SetActive(false);
    }

    public void markReady()
    {
        readyPart.SetActive(true);
        maiPart.SetActive(false);
        yukiPart.SetActive(false);
    }
}
