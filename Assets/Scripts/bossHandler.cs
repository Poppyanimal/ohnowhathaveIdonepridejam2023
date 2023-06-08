using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossHandler : MonoBehaviour
{
    [HideInInspector]
    public static bossHandler currentBoss;
    public int thisBossID = -1; //use 0 for midboss and 1 for final boss
    public List<majorPhase> majorPhases;
    int currentPhase = -1;
    Coroutine activePhase;

    public void registerAsCurrentBoss() { currentBoss = this; }
    public void unregisterCurrentBoss() { currentBoss = null; }

    void stopPhase()
    {
        if(activePhase != null)
            StopCoroutine(activePhase);
    }






}

public class majorPhase
{
    public List<ComplexPattern> phases;

    public int hpThisPhase = 100;
    public bool endPhaseEarlyIfTimeExpires = false;
    public float timerLengthSeconds = 60f;

    //public List<bossMovement> moveData;
}

public class bossMovement
{
    Vector2 targetPosition;
    float timeToArrive = 1f;
    //TODO
}