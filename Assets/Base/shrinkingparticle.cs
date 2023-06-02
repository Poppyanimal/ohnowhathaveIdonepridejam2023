using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shrinkingparticle : MonoBehaviour
{
    [SerializeField]
    float minMaxScale, maxMaxScale, minGrowTime, maxGrowTime, minDecayTime, maxDecayTime, minSpeed, maxSpeed;



    float mScale = .5f;
    float gTime = .1f;
    float dTime = 2f;

    Vector2 startingV = new Vector2(1f, .5f);

    void Start()
    {
        randomizeStats();
        gameObject.GetComponent<Rigidbody2D>().velocity = startingV;
        gameObject.transform.localScale = new Vector3(0f,0f,1f);
        StartCoroutine(doGrowDecayCycle());
    }

    void randomizeStats()
    {
        mScale = Random.Range(minMaxScale, maxMaxScale);
        gTime = Random.Range(minGrowTime, maxGrowTime);
        dTime = Random.Range(minDecayTime, maxDecayTime);
        Vector2 vel = Vector2.up * Random.Range(minSpeed, maxSpeed);
        startingV = rotateVector2(vel, Random.Range(0f, 2f*Mathf.PI));
    }

    IEnumerator doGrowDecayCycle()
    {
        float startT = Time.time;
        yield return new WaitUntil(delegate() //do grow
        {
            bool doneGrowing = false;
            float timeDif = Time.time - startT;
            if(timeDif >= gTime)
            {
                gameObject.transform.localScale = new Vector3(mScale, mScale, 1f);
                doneGrowing = true;
            }
            else
            {
                float ratio = timeDif / gTime;
                gameObject.transform.localScale = new Vector3(mScale * ratio, mScale * ratio, 1f);
            }
            return doneGrowing;
        });

        startT = Time.time;
        yield return new WaitUntil(delegate() //do shrink
        {
            bool doneShrinking = false;
            float timeDif = Time.time - startT;
            if(timeDif >= dTime)
            {
                gameObject.transform.localScale = new Vector3(0f, 0f, 1f);
                doneShrinking = true;
            }
            else
            {
                float ratio = timeDif / dTime;
                gameObject.transform.localScale = new Vector3(mScale - mScale * ratio, mScale - mScale * ratio, 1f);
            }
            return doneShrinking;
        });

        Object.Destroy(this.gameObject);
    }

    Vector2 rotateVector2(Vector2 i, float rotation)
    {
        return new Vector2(    
            i.x * Mathf.Cos(rotation) - i.y * Mathf.Sin(rotation),
            i.x * Mathf.Sin(rotation) + i.y * Mathf.Cos(rotation)
        );
    }
}
