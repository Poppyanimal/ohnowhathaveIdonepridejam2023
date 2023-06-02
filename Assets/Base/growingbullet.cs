using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class growingbullet : MonoBehaviour
{
    public float growthSpeed = 1f;
    
    void Start() { this.gameObject.transform.localScale = new Vector3(0f,0f,this.gameObject.transform.localScale.z); StartCoroutine(doInfiniteGrowth()); }

    IEnumerator doInfiniteGrowth()
    {
        float startTime = Time.time;
        Transform thisTransform = this.gameObject.transform;

        yield return new WaitUntil(delegate()
        {
            float timeDif = Time.time - startTime;
            thisTransform.localScale = new Vector3(growthSpeed*timeDif, growthSpeed*timeDif, thisTransform.localScale.z);
            return false;
        });
    }

}
