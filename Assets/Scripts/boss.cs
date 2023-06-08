using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boss : MonoBehaviour
{
    public List<majorPhase> majorPhases;
    public int currentPhase;
    public bossHandler.bossType type;

    Coroutine phaseLogicCoro;
    Coroutine movementCoro;


    void startPhase(int index)
    {
        stopCurrentPhase();
        phaseLogicCoro = StartCoroutine(phaseLogic(index));
        movementCoro = StartCoroutine(movementForPhase(index));
    }

    void stopCurrentPhase() { if(phaseLogicCoro != null) StopCoroutine(phaseLogicCoro); if(movementCoro != null) StopCoroutine(movementCoro); }

    IEnumerator phaseLogic(int index)
    {
        yield return new WaitForSeconds(1f);
    }

    IEnumerator movementForPhase(int index)
    {
        //TODO
        yield return new WaitForSeconds(1f);
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