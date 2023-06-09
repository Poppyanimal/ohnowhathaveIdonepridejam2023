using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boss : MonoBehaviour
{
    public List<majorPhase> majorPhases;
    public bossHandler.bossType type;

    Rigidbody2D thisBody;
    Coroutine phaseLogicCoro;
    Coroutine movementCoro;
    Coroutine phaseTimerCoro;

    public bool overrideStartForDebug = false;
    public int phaseIndexToStartup = 0;

    void Start() { thisBody = gameObject.GetComponent<Rigidbody2D>(); if(overrideStartForDebug) startPhase(phaseIndexToStartup); }

    public void startPhase(int index)
    {
        stopCurrentPhase();
        phaseLogicCoro = StartCoroutine(phaseLogic(index));
        movementCoro = StartCoroutine(movementForPhase(index));
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
        while(true)
        {
            foreach(ComplexPattern c in (GlobalVars.isDifficultyStandard ? majorPhases[index].standardPhases : majorPhases[index].easyPhases))
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
        }
    }

    public void doDeath()
    {
        stopCurrentPhase();
        StartCoroutine(doDeathEffect());
    }

    IEnumerator doDeathEffect()
    {
        //TODO
        //do all the fancy effects

        yield return new WaitForSeconds(1f);

        if(bossHandler.Singleton.IsOwner)
        {
            if(type is bossHandler.bossType.midboss)
                bossHandler.Singleton.midBossDefeated.Value = true;
            else if(type is bossHandler.bossType.finalboss)
                bossHandler.Singleton.finalBossDefeated.Value = true;
        }
    }


}

[System.Serializable]
public class majorPhase
{
    public List<ComplexPattern> easyPhases, standardPhases;

    public int hpThisPhase = 100;
    public bool endPhaseEarlyIfTimeExpires = false;
    public float timerLengthSeconds = 60f;

    public List<bossMovement> moveData;
}
[System.Serializable]
public class bossMovement
{
    public Vector2 targetPosition;
    public float timeToArrive = 1f;
    //TODO
}