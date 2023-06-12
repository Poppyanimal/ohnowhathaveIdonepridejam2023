using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sfxRotator : MonoBehaviour
{
    public AudioClip sfx;
    public List<AudioSource> sources;
    int curIndex = 0;

    public bool cooldownBetweenSFX = false;
    public float cooldownTime = .1f;
    bool onCooldown = false;
    public bool allowPlayAtStageEnd = false;

    void Start()
    {
        for(int i = 0; i < sources.Count; i++)
            sources[i].clip = sfx;
    }
    
    public void playSFX()
    {
        if(sfx == null)
            return;

        if(onCooldown || (StageHandler.Singleton != null && stageSFXHandler.Singleton.stageFinished && !allowPlayAtStageEnd))
            return;

        sources[curIndex].Play();
        curIndex++;
        if(curIndex > sources.Count - 1)
            curIndex = 0;

        if(cooldownBetweenSFX)
        {
            onCooldown = true;
            StartCoroutine(cooldown());
        }
    }

    IEnumerator cooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        onCooldown = false;
    }
}
