using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPattern : MonoBehaviour
{
    public Rigidbody2D bullet;
    public PatternData patternDat;
    public bool doDelayBeforeShots = false;
    public float delayBeforeShots;
    public List<AudioSource> patternShootSFXs;
    bool currentlyRunning = false;
    int curAudioSFXIndex = 0;

    Coroutine activityDecayCoro;
    Coroutine curPatternCoro;

    void doPattern()
    {
        currentlyRunning = true;
        if(patternDat.autoSettings.doAutoPattern)
        {
            //Debug.Log("Auto pattern called");
            float angleModifier = 0f;
            if(patternDat.trackTarget != PatternData.playerToTarget.noTracking)
            {
                bool hasTargetPosition = false;
                Vector2 targetPosition = Vector2.zero;

                switch(patternDat.trackTarget)
                {
                    case PatternData.playerToTarget.Closest:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getCloserPlayerTo(gameObject.transform.position);
                        break;
                    case PatternData.playerToTarget.Yuki:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getYukiPosition();
                        break;
                    case PatternData.playerToTarget.Mai:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getMaiPosition();
                        break;
                    case PatternData.playerToTarget.SpecificRigidbody:
                        if(patternDat.customRigidbodyTarget != null)
                        {
                            hasTargetPosition = true;
                            targetPosition = patternDat.customRigidbodyTarget.position;
                        }
                        break;
                }

                if(hasTargetPosition)
                    angleModifier -= KiroLib.angleToTarget(targetPosition, gameObject.transform.position);

            }

            Vector2 startingVelocity = patternDat.autoSettings.startingVelocity;

            if(isCrazyMathDerivative)
            {
                angleModifier *= crazyMathAngleMultiplication();
                angleModifier += crazyMathAngleAddition();
                startingVelocity *= crazyMathVelocityMultiplication();
                startingVelocity += crazyMathVelocityAddition();
            }

            float angleBetween = 360f/patternDat.autoSettings.bulletQuantity;
            for(int i = 0; i < patternDat.autoSettings.bulletQuantity; i++)
            {
                createBullet(angleBetween*i + angleModifier, startingVelocity, Vector2.zero);
            }
            StartCoroutine(activityDecay(0.5f));
        }
        else
        {
            float timeFromBulletDelays = patternDat.patternSettings.timeBetweenBullets * patternDat.patternSettings.sPatternList.Count;
            activityDecayCoro = StartCoroutine(activityDecay(patternDat.patternSettings.activityDurationOffset + timeFromBulletDelays));
            curPatternCoro = StartCoroutine(doSimplePatternLogic(patternDat, 0));
        }
    }

    void createBullet(float angle, Vector2 vel, Rigidbody2D bulletToUse, float angularVelocity, Vector2 positionOffset)
    {
        Quaternion rotation = new Quaternion();
        rotation.eulerAngles = new Vector3(0,0,-angle);
        Vector2 spawnedPosition = KiroLib.rotateVector2(angle, positionOffset);
        Rigidbody2D newBullet = Instantiate(bulletToUse, transform.position, rotation);
        newBullet.velocity = KiroLib.rotateVector2(angle, vel);
        newBullet.angularVelocity = angularVelocity;
    }

    void createBullet(float angle, Vector2 vel, Vector2 positionOffset)
    {
        createBullet(angle, vel, bullet, 0f, positionOffset);
    }

    void createBullet(float angle, Vector2 vel, float angularVelocity, Vector2 positionOffset)
    {
        createBullet(angle, vel, bullet, angularVelocity, positionOffset);
    }

    IEnumerator activityDecay(float timeToWait)
    {
        if(timeToWait < 0) timeToWait = 0;
        yield return new WaitForSeconds(timeToWait);
        currentlyRunning = false;
    }

    IEnumerator doSimplePatternLogic(PatternData settings, int curPatID)
    {
        if(curPatID < settings.patternSettings.sPatternList.Count)
        {
            if(doDelayBeforeShots)
                yield return new WaitForSeconds(delayBeforeShots);

            simplePatternInfo curPat = settings.patternSettings.sPatternList[curPatID];

            float angleModifier = curPat.angleOffset;
            SinPatternAngleInfo sAngInfo = curPat.SinInfo.SinAngleInfo;

            
            if(patternDat.trackTarget != PatternData.playerToTarget.noTracking && curPat.TrackingOverrideChoice == simplePatternInfo.TrackingOverride.Ignore)
            {
                bool hasTargetPosition = false;
                Vector2 targetPosition = Vector2.zero;

                switch(patternDat.trackTarget)
                {
                    case PatternData.playerToTarget.Closest:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getCloserPlayerTo(gameObject.transform.position);
                        break;
                    case PatternData.playerToTarget.Yuki:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getYukiPosition();
                        break;
                    case PatternData.playerToTarget.Mai:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getMaiPosition();
                        break;
                    case PatternData.playerToTarget.SpecificRigidbody:
                        if(patternDat.customRigidbodyTarget != null)
                        {
                            hasTargetPosition = true;
                            targetPosition = patternDat.customRigidbodyTarget.position;
                        }
                        break;
                }
                
                if(hasTargetPosition)
                    angleModifier -= KiroLib.angleToTarget(targetPosition, gameObject.transform.position);
            }
            else if(curPat.TrackingOverrideChoice != simplePatternInfo.TrackingOverride.Ignore)
            {
                bool hasTargetPosition = false;
                Vector2 targetPosition = Vector2.zero;

                switch(curPat.TrackingOverrideChoice)
                {
                    case simplePatternInfo.TrackingOverride.track_Closest:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getCloserPlayerTo(gameObject.transform.position);
                        break;
                    case simplePatternInfo.TrackingOverride.track_Yuki:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getYukiPosition();
                        break;
                    case simplePatternInfo.TrackingOverride.track_Mai:
                        hasTargetPosition = true;
                        targetPosition = StageHandler.Singleton.getMaiPosition();
                        break;
                    case simplePatternInfo.TrackingOverride.track_Custom_Body:
                        if(patternDat.customRigidbodyTarget != null)
                        {
                            hasTargetPosition = true;
                            targetPosition = patternDat.customRigidbodyTarget.position;
                        }
                        break;
                }
                
                if(hasTargetPosition)
                    angleModifier -= KiroLib.angleToTarget(targetPosition, gameObject.transform.position);
            }

            if(sAngInfo.useSinAngleOffset)
            {
                angleModifier += sAngInfo.amplitude*Mathf.Sin(curPat.SinInfo.getTimeSinceInit()/sAngInfo.period + sAngInfo.timeOffset);
            }

            Vector2 startingVelocity = curPat.startingVelocity;
            SinPatternVelocityInfo velInfo = curPat.SinInfo.SinVelocityInfo;

            if(velInfo.useSinVelocity == SinPatternVelocityInfo.SinVelocityUsage.x || velInfo.useSinVelocity == SinPatternVelocityInfo.SinVelocityUsage.both)
            {
                startingVelocity += new Vector2(velInfo.amplitude.x*Mathf.Sin(curPat.SinInfo.getTimeSinceInit()/velInfo.period.x + velInfo.timeOffset.x), 0);
            }
            if(velInfo.useSinVelocity == SinPatternVelocityInfo.SinVelocityUsage.y || velInfo.useSinVelocity == SinPatternVelocityInfo.SinVelocityUsage.both)
            {
                startingVelocity += new Vector2(0, velInfo.amplitude.y*Mathf.Sin(curPat.SinInfo.getTimeSinceInit()/velInfo.period.y + velInfo.timeOffset.y));
            }

            //Debug.Log("simple pattern logic called, do drift: " + settings.patternSettings.sPatternList[curPatID].driftInfo.doAngleDrift + " from " + settings.patternSettings.sPatternList[curPatID]);
            patternDrift driftInfo = settings.patternSettings.sPatternList[curPatID].driftInfo;
            if(driftInfo.doAngleDrift)
            {
                angleModifier += driftInfo.getCurrentDrift();
                driftInfo.drift();
                //Debug.Log(driftInfo.getCurrentDrift() + " drift from " + driftInfo);
            }

            float angularVelocityModifier = curPat.angularVelocity;

            if(isCrazyMathDerivative)
            {
                angleModifier *= crazyMathAngleMultiplication();
                angleModifier += crazyMathAngleAddition();
                startingVelocity *= crazyMathVelocityMultiplication();
                startingVelocity += crazyMathVelocityAddition();
                angularVelocityModifier *= crazyMathAngularVelocityMultiplication();
                angularVelocityModifier += crazyMathAngularVelocityAddition();
            }

            for(int i = 0; i < curPat.bulletQuantity; i++)
            {
                Vector2 posOffset = curPat.posOffsetInfo.doSpawnOffset ? curPat.posOffsetInfo.offset : Vector2.zero;

                if(curPat.bulletOverride == null)
                    createBullet(angleModifier + curPat.angleBetweenBullets * i, startingVelocity, angularVelocityModifier, posOffset);
                else
                    createBullet(angleModifier + curPat.angleBetweenBullets * i, startingVelocity, curPat.bulletOverride, angularVelocityModifier, posOffset);
            }

            if(patternShootSFXs.Count > 0)
            {
                patternShootSFXs[curAudioSFXIndex].Play();
                curAudioSFXIndex++;
                if(curAudioSFXIndex >= patternShootSFXs.Count)
                    curAudioSFXIndex = 0;
            }

            if(settings.patternSettings.timeBetweenBullets > 0)
                yield return new WaitForSeconds(settings.patternSettings.timeBetweenBullets);
            curPatternCoro = StartCoroutine(doSimplePatternLogic(settings, curPatID + 1));
        }
    }

    public bool isRunning()
    {
        return currentlyRunning;
    }

    public void tryPattern()
    {
        if(!currentlyRunning)
            doPattern();
    }

    public void forceStop()
    {
        if(activityDecayCoro != null)
            StopCoroutine(activityDecayCoro);
        if(curPatternCoro != null)
            StopCoroutine(curPatternCoro);
        currentlyRunning = false;
    }




    protected bool isCrazyMathDerivative = false;
    protected virtual float crazyMathAngleAddition()
    {
        return 0f;
    }

    protected virtual float crazyMathAngleMultiplication()
    {
        return 1f;
    }

    protected virtual Vector2 crazyMathVelocityAddition()
    {
        return Vector2.zero;
    }

    protected virtual Vector2 crazyMathVelocityMultiplication()
    {
        return new Vector2(1, 1);
    }

    protected virtual float crazyMathAngularVelocityAddition()
    {
        return 0f;
    }

    protected virtual float crazyMathAngularVelocityMultiplication()
    {
        return 1f;
    }

}
