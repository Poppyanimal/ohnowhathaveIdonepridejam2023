using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levelMusicHandler : MonoBehaviour
{
    public static levelMusicHandler Singleton;
    public musiclooppoint stageMusic;


    void Start()
    {
        Singleton = this;
    }

    public void stopStageMusic()
    {
        if(stageMusic != null)
            stageMusic.stopTracks();
    }

    public void playStageMusic()
    {
        if(stageMusic != null)
            stageMusic.playTracks();
    }
}
