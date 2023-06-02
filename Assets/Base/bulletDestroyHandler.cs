using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletDestroyHandler : MonoBehaviour
{
    [SerializeField]
    GameObject destroyEffect;
    [SerializeField]
    float fadeTime = .2f;
    public void destroy()
    {
        this.gameObject.layer = LayerMask.NameToLayer("ignoreall");
        try
        {
            this.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        catch {}

        if(destroyEffect != null)
            Instantiate(destroyEffect, this.gameObject.transform.position + new Vector3(0f,0f,-.1f), this.gameObject.transform.rotation);

        StartCoroutine(shrinkAndDestroy());
    }

    IEnumerator shrinkAndDestroy()
    {
        Vector3 sScale = gameObject.transform.localScale;
        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeDif = Time.time - startTime;
            float ratio = timeDif / fadeTime;
            if(timeDif >= fadeTime)
            {
                this.gameObject.transform.localScale = new Vector3(0f, 0f, sScale.z);
                return true;
            }
            else
            {
                this.gameObject.transform.localScale = new Vector3(sScale.x - sScale.x * ratio, sScale.y - sScale.y * ratio, sScale.z);
            }
            return false;
        });
        Object.Destroy(this.gameObject);
    }
}
