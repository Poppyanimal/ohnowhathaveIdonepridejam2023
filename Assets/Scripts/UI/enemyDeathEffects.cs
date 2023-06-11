using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyDeathEffects : MonoBehaviour
{
    public static enemyDeathEffects Singleton;
    public GameObject deathSparklePrefab;
    public GameObject explosionEffectPrefab;
    public GameObject bossRegExplosionEffectPrefab;
    public GameObject bossBigExplosionEffectPrefab;
    public int minDeathSparkles = 3;
    public int maxDeathSparkles = 5;
    public float minSpeed = 3f;
    public float maxSpeed = 6f;
    public float timeTillDecay = 1f;

    public int bossMinSparks = 20;
    public int bossMaxSparks = 30;
    public float bossExplosionRange = 1f;
    public int bossExplosionExtra = 8;
    public float timeBetweenBossExtraExplosions = .1f;

    public GameObject playerHurtParticle;
    public int minPlayerHurtPart = 10;
    public int maxPlayerHurtPart = 16;
    public float minSpeedPlayer = 3f;
    public float maxSpeedPlayer = 6f;
    public float timeTillDecayPlayer = 1f;
    
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
        }
        Instantiate(explosionEffectPrefab, position - Vector3.forward * .00002f, Quaternion.identity);
    }

    
    public void doBossDeathAt(Vector3 position)
    {
        Debug.Log("doBossDeathAt() called");
        int sparkAmount = Mathf.RoundToInt(Random.Range(bossMinSparks, bossMaxSparks));

        for(int i = 0; i < sparkAmount; i++)
        {
            float rotation = Random.Range(0f, 360f);
            GameObject spark = Instantiate(deathSparklePrefab, position - Vector3.forward * .00001f, Quaternion.Euler(0f,0f,rotation));
            spark.GetComponent<Rigidbody2D>().velocity = KiroLib.rotateVector2(rotation, Vector2.up * Random.Range(minSpeed, maxSpeed));
            spark.GetComponent<LobbyParticle>().timeTillDecay = timeTillDecay;
        }

        if(stageSFXHandler.Singleton.bossDeath != null)
            stageSFXHandler.Singleton.bossDeath.playSFX();

        Instantiate(bossBigExplosionEffectPrefab, position - Vector3.forward * .00002f, Quaternion.identity);
        StartCoroutine(lastPopsBossDeath(position));
    }

    IEnumerator lastPopsBossDeath(Vector3 position)
    {
        for(int i = 0; i < bossExplosionExtra; i++)
        {
            if(stageSFXHandler.Singleton.bossDeath != null)
                stageSFXHandler.Singleton.bossDeath.playSFX();
            yield return new WaitForSeconds(timeBetweenBossExtraExplosions);
            Vector3 posAdjust = new Vector3(Random.Range(-bossExplosionRange, bossExplosionRange), Random.Range(-bossExplosionRange, bossExplosionRange), 0f);
            Instantiate(bossRegExplosionEffectPrefab, position + posAdjust - Vector3.forward * .00002f, Quaternion.identity);


            int sparkAmount = Mathf.RoundToInt(Random.Range(bossMinSparks, bossMaxSparks));

            for(int j = 0; j < sparkAmount; j++)
            {
                float rotation = Random.Range(0f, 360f);
                GameObject spark = Instantiate(deathSparklePrefab, position - Vector3.forward * .00001f, Quaternion.Euler(0f,0f,rotation));
                spark.GetComponent<Rigidbody2D>().velocity = KiroLib.rotateVector2(rotation, Vector2.up * Random.Range(minSpeed, maxSpeed));
                spark.GetComponent<LobbyParticle>().timeTillDecay = timeTillDecay;
            }
        }
    }

    public void makePlayerTakeHitEffect(bool isYuki)
    {
        stageSFXHandler.Singleton.playerHit.playSFX();
        Player p = isYuki ? StageHandler.Singleton.YukiBody.gameObject.GetComponent<Player>() : StageHandler.Singleton.MaiBody.gameObject.GetComponent<Player>();
        p.triggerRedFlashEffect();

        
        int effectAmount = Mathf.RoundToInt(Random.Range(minPlayerHurtPart, maxPlayerHurtPart));

        for(int i = 0; i < effectAmount; i++)
        {
            float rotation = Random.Range(0f, 360f);
            GameObject eff = Instantiate(playerHurtParticle, p.gameObject.transform.position - Vector3.forward * .01f, Quaternion.Euler(0f,0f,rotation));
            eff.GetComponent<Rigidbody2D>().velocity = KiroLib.rotateVector2(rotation, Vector2.up * Random.Range(minSpeedPlayer, maxSpeedPlayer));
            eff.GetComponent<LobbyParticle>().timeTillDecay = timeTillDecayPlayer;
        }
    }
}
