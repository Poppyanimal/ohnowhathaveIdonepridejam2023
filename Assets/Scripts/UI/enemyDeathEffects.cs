using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyDeathEffects : MonoBehaviour
{
    public static enemyDeathEffects Singleton;
    public GameObject deathSparklePrefab;
    public GameObject explosionEffectPrefab;
    public int minDeathSparkles = 3;
    public int maxDeathSparkles = 5;
    public float minSpeed = 3f;
    public float maxSpeed = 6f;
    public float timeTillDecay = 1f;
    
    void Start()
    {
        Singleton = this;
    }

    public void doDeathAt(Vector3 position)
    {
        int sparkAmount = Mathf.RoundToInt(Random.Range(minDeathSparkles, maxDeathSparkles));

        for(int i = 0; i < sparkAmount; i++)
        {
            float rotation = Random.Range(0f, 360f);
            GameObject spark = Instantiate(deathSparklePrefab, position - Vector3.forward * .00001f, Quaternion.Euler(0f,0f,rotation));
            spark.GetComponent<Rigidbody2D>().velocity = KiroLib.rotateVector2(rotation, Vector2.up * Random.Range(minSpeed, maxSpeed));
            spark.GetComponent<LobbyParticle>().timeTillDecay = timeTillDecay;
            Instantiate(explosionEffectPrefab, position - Vector3.forward * .00002f, Quaternion.identity);
        }
    }
}
