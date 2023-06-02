using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musiclooppoint : MonoBehaviour
{
    [SerializeField]
    AudioSource sourceOne, sourceTwo;
    [SerializeField]
    AudioClip clipToUse;
    [SerializeField]
    double trackLengthM, trackLengthS;
    double fullTrackLength;
    //float delayAdjust = 0;
    [SerializeField]
    bool playOnStart = false;

    Coroutine playLoop;

    void Awake()
    {
        sourceOne.playOnAwake = false;
        sourceTwo.playOnAwake = false;
        sourceOne.loop = false;
        sourceTwo.loop = false;
        sourceOne.clip = clipToUse;
        sourceTwo.clip = clipToUse;
        fullTrackLength = trackLengthM * 60 + trackLengthS;
    }

    void Start()
    {
        if(playOnStart)
            playTracks();
    }

    public void playTracks()
    {
        playLoop = StartCoroutine(doPlayLoop());
    }

    public void stopTracks()
    {
        StopCoroutine(playLoop);
        sourceOne.Stop();
        sourceTwo.Stop();
    }

    IEnumerator doPlayLoop()
    {
        sourceOne.Play();
        double targetTime = AudioSettings.dspTime + fullTrackLength;
        bool startTrackOne = false;


        yield return new WaitUntil(delegate()
        {
            //Debug.Log("Is Source One Playing? " + sourceOne.isPlaying + " Is Source Two Playing? " + sourceTwo.isPlaying +" current dsp time left "+ (targetTime - AudioSettings.dspTime));
            if(targetTime - AudioSettings.dspTime <= 1.0)
            {
                //prep track
                if(startTrackOne)
                    sourceOne.PlayScheduled(targetTime);
                else
                    sourceTwo.PlayScheduled(targetTime);
                targetTime = targetTime + fullTrackLength;
                startTrackOne = !startTrackOne;
            }
            //else if not prepping track loop until need to

            return false;
        });
    }

}