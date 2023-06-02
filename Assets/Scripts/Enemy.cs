using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for generic enemies, bosses will likely use a different class or just extend this
public class Enemy : MonoBehaviour
{
    public ComplexPattern standardPattern, easyPattern;
    public loopType loopPattern;
    public int timesToLoop;
    [HideInInspector]
    public int spawnIndexId; //set when it is spawned, keeps track of it in stage handler


    //TODO: pathing to different points in space, custom time delay after each stop is made, support for curved paths to a point (semicircles, sine wave movement maybe)

    //TODO: have this activate after a set location is reached
    public float timeBeforeShooting;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(countdownThenStartShooting());
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

            if(loopPattern != loopType.loop_forever)
            {
                if(loopPattern is loopType.no_looping || (loopPattern is loopType.loop_x_times && loops >= timesToLoop))
                    break;
            }
            
            loops++;
        }
    }

    public enum loopType { no_looping, loop_x_times, loop_forever }
}
