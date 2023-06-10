using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyParticleEmitter : MonoBehaviour
{
    public GameObject particlePrefab;
    public float maxRangeHoriz = 12f;
    public float maxAngle = 15f;
    public float minSpeed = 1f;
    public float maxSpeed = 2f;
    public float TimeBetweenParticles = .5f;
    public float timeTillDecay = 3f;

    void Start()
    {
        StartCoroutine(doParticleEmission());
    }

    IEnumerator doParticleEmission()
    {
        while(true)
        {
            GameObject part = Instantiate(particlePrefab, gameObject.transform.position + Vector3.back * .1f + Vector3.right * Random.Range(-maxRangeHoriz, maxRangeHoriz), Quaternion.identity);

            Vector2 newVel = KiroLib.rotateVector2(Random.Range(-maxAngle, maxAngle), Vector2.up * Random.Range(minSpeed, maxSpeed));

            part.GetComponent<LobbyParticle>().timeTillDecay = timeTillDecay;
            part.GetComponent<Rigidbody2D>().velocity = newVel;

            yield return new WaitForSeconds(TimeBetweenParticles);
        }
    }
}
