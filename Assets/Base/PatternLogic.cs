using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternLogic : MonoBehaviour
{
    public List<majorPhase> majorPhases;

    public Rigidbody2D player;
    public Rigidbody2D boss;
    public bosshealthhandler bossHealth;


    protected int currentMajorPhase = 0;
    protected int currentPhase = 0;
    bool emergencyBrake = false;
    Coroutine moveToTargetCoro;
    //bool currentlyInTransit = false;
    bool isBooting = true;

    void Start() { if(bossHealth == null) Debug.Log("boss health is null!"); }

    void FixedUpdate()
    {
        if(!emergencyBrake)
            doMajorPhaseLogic();
    }

    protected virtual void doMajorPhaseLogic()
    {
        if(isBooting)
        {
            isBooting = false;
            if(majorPhases[currentMajorPhase].moveToData.Count > 0)
                moveToTargetCoro = StartCoroutine(doMovementCycle(majorPhases[currentMajorPhase].moveToData, 0, majorPhases[currentMajorPhase].moveSpeed, majorPhases[currentMajorPhase].loopMoveData));
        }

        if(bossHealth != null && bossHealth.getCurrentHealthPercent() <= majorPhases[currentMajorPhase].healthPercentToDropTactic && majorPhases[currentMajorPhase].healthPercentToDropTactic != 0)
        {
            forceStopMajorPhase(currentMajorPhase);

            currentMajorPhase++;
            if(currentMajorPhase >= majorPhases.Count)
            {
                currentMajorPhase--;
                Debug.LogError("Phase Transition Occured, but there was no phase to move to!!!");
            }
            currentPhase = 0;
            if(majorPhases[currentMajorPhase].moveToData.Count > 0)
                moveToTargetCoro = StartCoroutine(doMovementCycle(majorPhases[currentMajorPhase].moveToData, 0, majorPhases[currentMajorPhase].moveSpeed, majorPhases[currentMajorPhase].loopMoveData));
        }
        else
        {
            doMinorPhaseLogic();
        }
    }


    protected virtual void doMinorPhaseLogic()
    {
        if(!currentlyWaitingForDelay)
        {
            if(majorPhases[currentMajorPhase].phases[currentPhase].isFinished())
            {
                majorPhases[currentMajorPhase].phases[currentPhase].reset();
                currentPhase++;
                if(currentPhase >= majorPhases[currentMajorPhase].phases.Count)
                    currentPhase = 0;
                currentlyWaitingForDelay = true;
                StartCoroutine(waitForDelay());
            }
            else
                majorPhases[currentMajorPhase].phases[currentPhase].shootAllPatterns();
        }
    }

    public void tellBossSheDied() //meant for boss to call from health script
    {
        emergencyBrake = true;
        forceStopMajorPhase(currentMajorPhase);
    }

    public void tellBossThatPlayerDied() { tellBossSheDied(); } //meant for player to call

    void forceStopMajorPhase(int mPhase)
    {
        foreach(ComplexPattern cPatterns in majorPhases[mPhase].phases)
            foreach(cPatternLoopInfo patInfo in cPatterns.simultanousPatterns)
                patInfo.bPattern.forceStop();
        forceStopMovement();
    }

    void forceStopMovement()
    {
        if(moveToTargetCoro != null)
            StopCoroutine(moveToTargetCoro);
        //currentlyInTransit = false;
    }

    IEnumerator doMovementCycle(List<movementData> moveData, int curMoveIndex, float mPhaseSpeed, bool loop)
    {
        float speed = moveData[curMoveIndex].useSpeedOverride ? moveData[curMoveIndex].speedOverride : mPhaseSpeed;
        //currentlyInTransit = true;
        Vector2 targetLocation;

        if(moveData[curMoveIndex].followPlayerInstead)
            targetLocation = player.position;
        else
            targetLocation = moveData[curMoveIndex].targetLocation;

        Vector2 ogLocation = boss.position;
        Vector2 distChange = targetLocation - ogLocation;
        float timeToReach = distChange.magnitude / speed;

        float startingTime = Time.time;

        yield return new WaitUntil(delegate()
        {
            float timeDif = Time.time - startingTime;
            if(timeDif >= timeToReach)
            {
                boss.position = targetLocation;
                boss.velocity = Vector2.zero;
                return true;
            }
            else
            {
                boss.velocity = distChange.normalized * speed;
                return false;
            }
        });

        /*
        yield return new WaitUntil(delegate()
        {
            float timeDif = Time.time - startingTime;
            float ratio = timeDif/timeToReach;
            if(timeDif >= timeToReach)
            {
                boss.position = targetLocation;
                return true;
            }
            boss.position = ogLocation + (ratio * distChange);
            return false;
        });
        */



        curMoveIndex++;
        if(curMoveIndex >= moveData.Count)
        {
            if(loop)
            {
                curMoveIndex = 0;
                moveToTargetCoro = StartCoroutine(doMovementCycle(moveData, curMoveIndex, mPhaseSpeed, loop));
            }
            else
            {
                //currentlyInTransit = false;
            }
        }
        else
        {
            moveToTargetCoro = StartCoroutine(doMovementCycle(moveData, curMoveIndex, mPhaseSpeed, loop));
        }
    }

    protected bool currentlyWaitingForDelay = false;
    protected IEnumerator waitForDelay()
    {
        yield return new WaitForSeconds(majorPhases[currentMajorPhase].phases[currentPhase].timeDelayAfterFinished);
        currentlyWaitingForDelay = false;
    }

    [System.Serializable]
    public struct majorPhase
    {
        public List<ComplexPattern> phases;
        public float healthPercentToDropTactic;
        public List<movementData> moveToData;
        public bool loopMoveData;
        public float moveSpeed;
    }

    [System.Serializable]
    public struct movementData
    {
        public Vector2 targetLocation;
        public bool followPlayerInstead; //lagging behind, grabbing the player's location at the start of the coroutine
        public bool useSpeedOverride;
        public float speedOverride;
    }

}

[System.Serializable]
public class ComplexPattern
{
    public float timeDelayAfterFinished = 0.5f; //time after all patterns are finished before the next phase can occur

    public List<cPatternLoopInfo> simultanousPatterns = new List<cPatternLoopInfo>()
    {
        new cPatternLoopInfo()
    };

    public void reset()
    {
        //Debug.Log("reset called");
        for(int i = 0; i < simultanousPatterns.Count; i++)
        {
            simultanousPatterns[i].resetTimesShot();
        }
    }

    public void shootAllPatterns()
    {
        for(int i = 0; i < simultanousPatterns.Count; i++)
        {
            if(simultanousPatterns[i].timesShot() < simultanousPatterns[i].timesToShoot && !simultanousPatterns[i].bPattern.isRunning())
            {
                simultanousPatterns[i].bPattern.tryPattern();
                simultanousPatterns[i].incTimesShot();
            }
        }
    }

    public bool isFinished()
    {
        for(int i = 0; i < simultanousPatterns.Count; i++)
        {
            if(simultanousPatterns[i].timesShot() < simultanousPatterns[i].timesToShoot || simultanousPatterns[i].bPattern.isRunning())
                return false;
        }
        return true;
    }
}

[System.Serializable]
public class cPatternLoopInfo
{
    public int timesToShoot = 10;
    public BulletPattern bPattern;

    int timesAlreadyShot = 0;
    public void resetTimesShot() { timesAlreadyShot = 0; }
    public void incTimesShot() { timesAlreadyShot++; }
    public int timesShot() { return timesAlreadyShot; }
}