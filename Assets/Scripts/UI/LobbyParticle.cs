using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyParticle : MonoBehaviour
{
    [HideInInspector]
    public float timeTillDecay = 1f;

    void Start()
    {
        StartCoroutine(shrinkThenDestroy());
    }

    IEnumerator shrinkThenDestroy()
    {
        Vector3 startingScale = gameObject.transform.localScale;

        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / timeTillDecay;
            if(timeRatio >= 1f)
            {
                gameObject.transform.localScale = Vector3.forward;
                return true;
            }
            else
            {
                float scaling = 1f - timeRatio;
                gameObject.transform.localScale = new Vector3(scaling * startingScale.x, scaling * startingScale.y, 1f);
                return false;
            }
        });

        Destroy(this.gameObject);
    }
}
