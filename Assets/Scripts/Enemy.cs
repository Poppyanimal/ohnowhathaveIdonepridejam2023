using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for generic enemies, bosses will likely use a different class or just extend this
public class Enemy : MonoBehaviour
{
    public int maxHealth = 10;
    public ComplexPattern standardPattern, easyPattern;
    public loopType shotLoopPattern;
    public int timesToLoop;
    public List<moveInfo> movements;
    public bool despawnAtMovementEnd;
    Rigidbody2D thisBody;
    [HideInInspector]
    public int spawnIndexId; //set when it is spawned, keeps track of it in stage handler


    //TODO: pathing to different points in space, custom time delay after each stop is made, support for curved paths to a point (semicircles, sine wave movement maybe)

    //TODO: have this activate after a set location is reached
    public float timeBeforeShooting;
    public void startLogic()
    {
        thisBody = gameObject.GetComponent<Rigidbody2D>();
        StartCoroutine(countdownThenStartShooting());

        if(movements.Count > 0)
            StartCoroutine(doMovement());
    }


    bool shouldDie() { return StageHandler.Singleton.getDamageForEnemy(spawnIndexId) >= maxHealth; }

    IEnumerator doMovement(int curIndex = 0)
    {
        Vector2 startPos = thisBody.position;
        movementType xMType = movements[curIndex].xMovement.type;
        movementType yMType = movements[curIndex].yMovement.type;
        float xVel = thisBody.velocity.x;
        float yVel = thisBody.velocity.y;

        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / movements[curIndex].timeToTake;
            float timeLeft = movements[curIndex].timeToTake - (Time.time - startTime);

            if(timeRatio >= 1)
            {
                float newXPos = startPos.x + (xMType is movementType.toPointDamp ? movements[curIndex].xMovement.unitsMoved : 0);
                float newYPos = startPos.y + (yMType is movementType.toPointDamp ? movements[curIndex].yMovement.unitsMoved : 0);
                thisBody.position = new Vector2(newXPos, newYPos);
                return true;
            }
            else
            {
                float newXPos = thisBody.position.x;
                float newYPos = thisBody.position.y;

                if(xMType is movementType.toPointDamp)
                    newXPos = Mathf.SmoothDamp(newXPos, startPos.x + movements[curIndex].xMovement.unitsMoved, ref xVel, timeLeft);
                else if(xMType is movementType.toPointSin)
                    newXPos = startPos.x + movements[curIndex].xMovement.amplitude * Mathf.Sin((Mathf.PI * timeRatio) * movements[curIndex].xMovement.numOfPeaks);
                else if(xMType is movementType.toPointCos)
                    newXPos = startPos.x + movements[curIndex].xMovement.amplitude * Mathf.Cos((2 * Mathf.PI * timeRatio) * movements[curIndex].xMovement.numOfPeaks);

                
                if(yMType is movementType.toPointDamp)
                    newYPos = Mathf.SmoothDamp(newYPos, startPos.y + movements[curIndex].yMovement.unitsMoved, ref yVel, timeLeft);
                else if(yMType is movementType.toPointSin)
                    newYPos = startPos.y + movements[curIndex].yMovement.amplitude * Mathf.Sin((Mathf.PI * timeRatio) * movements[curIndex].yMovement.numOfPeaks);
                else if(yMType is movementType.toPointCos)
                    newYPos = startPos.y + movements[curIndex].yMovement.amplitude * Mathf.Cos((2 * Mathf.PI * timeRatio) * movements[curIndex].yMovement.numOfPeaks);
                    
                thisBody.position = new Vector2(newXPos, newYPos);

                return false;
            }
        });

        //thisBody.velocity = new Vector2(xVel,yVel);

        yield return new WaitForSeconds(movements[curIndex].timeToWaitAfter);

        if(curIndex + 1 >= movements.Count)
            movementIsDone();
        else
            StartCoroutine(doMovement(curIndex + 1));
    }

    void movementIsDone()
    {
        thisBody.velocity = Vector2.zero;
        Debug.Log("Enemy Movement Finished");
        if(despawnAtMovementEnd)
        {
            StageHandler.Singleton.disableEnemy(spawnIndexId);
            Destroy(this.gameObject);
        }
    }


    IEnumerator countdownThenStartShooting()
    {
        yield return new WaitForSeconds(timeBeforeShooting);

        int loops = 0;

        ComplexPattern pat = GlobalVars.isDifficultyStandard ? standardPattern : easyPattern;
        while(true)
        {
            yield return new WaitUntil(delegate()
            {
                pat.shootAllPatterns();
                return pat.isFinished();
            });

            yield return new WaitForSeconds(pat.timeDelayAfterFinished);
            pat.reset();

            if(shotLoopPattern != loopType.loop_forever)
            {
                if(shotLoopPattern is loopType.no_looping || (shotLoopPattern is loopType.loop_x_times && loops >= timesToLoop))
                    break;
            }

            loops++;
        }
    }

    public enum loopType { no_looping, loop_x_times, loop_forever }

    [System.Serializable]
    public struct movement
    {
        public movementType type;
        public float unitsMoved; //ignored for sine/cosine for now
        public float amplitude;
        public int numOfPeaks; //for sine a full peak is every pi, cosine a full peak is every 2pi
    }

    [System.Serializable]
    public struct moveInfo
    {
        public float timeToTake;
        public float timeToWaitAfter;
        public movement xMovement;
        public movement yMovement;
    }

    public enum movementType { nothing, toPointDamp, toPointSin, toPointCos }
}
