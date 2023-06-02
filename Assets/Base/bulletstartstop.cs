using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletstartstop : MonoBehaviour
{
    [SerializeField]
    float timeTillStop, durationOfStop;
    Rigidbody2D thisBody;

    void Start() { thisBody = this.gameObject.GetComponent<Rigidbody2D>(); StartCoroutine(doStartStop()); }

    IEnumerator doStartStop()
    {
        Vector2 ogSpeed = thisBody.velocity;
        yield return new WaitForSeconds(timeTillStop);
        thisBody.velocity = Vector2.zero;
        yield return new WaitForSeconds(durationOfStop);
        thisBody.velocity = ogSpeed;
    }
}
