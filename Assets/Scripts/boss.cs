using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boss : MonoBehaviour
{
    public List<majorPhase> majorPhases;
    public bossHandler.bossType type;

    Rigidbody2D thisBody;
    Coroutine phaseLogicCoro = null;
    Coroutine movementCoro = null;
    Coroutine phaseTimerCoro = null; //TODO
    Coroutine checkForHitsCoro = null;

    Collider2D hitbox;

    public bool overrideStartForDebug = false;
    public int phaseIndexToStartup = 0;
    public bool debugKillOnLoad = false;

    void Start()
    {
        if(debugKillOnLoad)
            doDeath();
        else if(overrideStartForDebug)
            startPhase(phaseIndexToStartup);
    }

    public void startPhase(int index)
    {
        Debug.Log("starting phase of index: "+index);
        if(thisBody == null)
            thisBody = gameObject.GetComponent<Rigidbody2D>();
        if(hitbox == null)
            hitbox = gameObject.GetComponent<Collider2D>();
        stopCurrentPhase();
        phaseLogicCoro = StartCoroutine(phaseLogic(index));
        movementCoro = StartCoroutine(movementForPhase(index));

        if(checkForHitsCoro == null)
            checkForHitsCoro = StartCoroutine(checkForHits());
    }

    void stopCurrentPhase()
    {
        if(phaseLogicCoro != null)
            StopCoroutine(phaseLogicCoro);
        if(movementCoro != null)
            StopCoroutine(movementCoro);
        if(phaseTimerCoro != null)
            StopCoroutine(phaseTimerCoro);
    }

    IEnumerator phaseLogic(int index)
    {
        yield return new WaitForSeconds(majorPhases[index].delayBeforePhaseStarts);

        List<ComplexPattern> phaseList = (GlobalVars.isDifficultyStandard ? majorPhases[index].standardPhases : majorPhases[index].easyPhases);
        while(true)
        {
            foreach(ComplexPattern c in phaseList)
            {
                c.reset();
                yield return new WaitUntil(delegate()
                {
                    c.shootAllPatterns();
                    return c.isFinished();
                });
            }
        }
    }

    IEnumerator movementForPhase(int index)
    {
        if(majorPhases[index].moveData.Count > 0)
        {
            while(true)
            {
                foreach(bossMovement move in majorPhases[index].moveData)
                {
                    Vector2 ogPos = thisBody.position;
                    Vector2 posDif = move.targetPosition - ogPos;

                    float startTime = Time.time;
                    yield return new WaitUntil(delegate()
                    {
                        float timeRatio = (Time.time - startTime) / move.timeToArrive;
                        if(timeRatio >= 1)
                        {
                            thisBody.position = move.targetPosition;
                            return true;
                        }
                        else
                        {
                            thisBody.position = ogPos + timeRatio * posDif;
                            return false;
                        }
                    });
                }
                if(!majorPhases[index].loopMoveData)
                    break;
            }
        }
    }

    public void doDeath()
    {
        Debug.Log("doDeath() called");
        stopCurrentPhase();
        StopCoroutine(checkForHitsCoro);

        if(enemyDeathEffects.Singleton != null)
            enemyDeathEffects.Singleton.doBossDeathAt(gameObject.transform.position);

        StartCoroutine(doDeathEffect());
    }

    IEnumerator doDeathEffect()
    {
        //TODO
        //do all the fancy death effects

        float startTime = Time.time;
        float timeToTake = 1f;
        Vector3 startingScale = gameObject.transform.localScale;

        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / timeToTake;
            if(timeRatio >= 1f)
            {
                gameObject.transform.localScale = Vector3.forward * startingScale.z;
                return true;
            }
            else
            {
                gameObject.transform.localScale = new Vector3(1f,1f,0f) * (1f - timeRatio) + Vector3.forward * startingScale.z;
                return false;
            }
        });

        Destroy(this.gameObject);

        if(bossHandler.Singleton.IsOwner)
        {
            if(type is bossHandler.bossType.midboss)
                bossHandler.Singleton.midBossDefeated.Value = true;
            else if(type is bossHandler.bossType.finalboss)
                bossHandler.Singleton.finalBossDefeated.Value = true;
        }
    }

    IEnumerator checkForHits()
    {
        yield return new WaitUntil(delegate()
        {
            Collider2D[] results = new Collider2D[16];
            int hits = hitbox.OverlapCollider(KiroLib.getFakePBulletFilter(), results);

            for(int i = 0; i < hits; i++)
            {
                try
                {
                    results[i].gameObject.GetComponent<bulletDestroyHandler>().destroy();
                }
                catch
                {
                    Destroy(results[i].gameObject);
                }
            }

            results = new Collider2D[16];
            hits = hitbox.OverlapCollider(KiroLib.getPBulletFilter(), results);

            for(int i = 0; i < hits; i++)
            {
                try
                {
                    bossHandler.Singleton.damageBoss();
                } 
                catch(System.Exception e) { Debug.LogError("Could not damage boss, threw the following Exception: "+e); }

                try
                {
                    results[i].gameObject.GetComponent<bulletDestroyHandler>().destroy();
                }
                catch
                {
                    Destroy(results[i].gameObject);
                }
            }

            return false;
        });
    }


}

[System.Serializable]
public class majorPhase
{
    public List<ComplexPattern> easyPhases, standardPhases;

    public int hpThisPhase = 100;
    public bool endPhaseEarlyIfTimeExpires = false;
    public float timerLengthSeconds = 60f;
    public float delayBeforePhaseStarts = 1f;

    public List<bossMovement> moveData;
    public bool loopMoveData = false;
}
[System.Serializable]
public class bossMovement
{
    public Vector2 targetPosition;
    public float timeToArrive = 1f;
    //TODO
}