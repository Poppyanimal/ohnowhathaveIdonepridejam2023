using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class scoreDrift : MonoBehaviour
{
    public bool debugBypass = false;
    public float maxAngle = 30;
    public float fadeTime = 1;
    public float minVel = 2f;
    public float maxVel = 3f;
    TMP_Text text;
    Rigidbody2D thisBody;
    
    void Start()
    {
        if(debugBypass)
            doStartup();
    }
    public void doStartup()
    {
        text = gameObject.GetComponent<TMP_Text>();
        thisBody = gameObject.GetComponent<Rigidbody2D>();
        thisBody.rotation = Random.Range(-maxAngle, maxAngle);
        Vector2 vel = Vector2.up * Random.Range(minVel, maxVel);
        vel = KiroLib.rotateVector2(-thisBody.rotation, vel);
        thisBody.velocity = vel;
        StartCoroutine(fadeEffect());
    }

    IEnumerator fadeEffect()
    {
        Color startingColor = text.color;
        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / fadeTime;
            if(timeRatio >= 1)
            {   
                Color endCol = startingColor;
                endCol.a = 0f;
                text.color = endCol;
                return true;
            }
            else
            {
                Color curCol = startingColor;
                curCol.a = 1f - timeRatio;
                text.color = curCol;
                return false;
            }
        });

        Destroy(this.gameObject);
    }

    public void setText(string t) { gameObject.GetComponent<TMP_Text>().text = t; }
}
