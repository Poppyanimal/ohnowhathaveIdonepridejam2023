using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class StageHandler : NetworkBehaviour
{
    public static StageHandler Singleton;
    public SpriteRenderer backgroundDimming;
    public Image leftPanel, rightPanel;
    public float fadeDistance;
    public float fadeTime;
    public Rigidbody2D YukiBody, MaiBody;

    int currentFlag = 0;

    public List<stageFlag> stageFlags;

    [HideInInspector]
    public List<enemyData> enemyTable = new();

    //the boss fight




    void Awake() { Color col = backgroundDimming.color; col.a = GlobalVars.screenDim; backgroundDimming.color = col; Singleton = this; }
    void Start() { StartCoroutine(doFadeInTransition()); }


    IEnumerator doLogicForSequence(int flagIndex)
    {
        yield return new WaitForSeconds(.1f);
        //TODO
    }

    void initEnemyTable() //used for generic enemies, not bosses
    {
        enemyTable.Clear();
        foreach(stageFlag flag in stageFlags)
        {
            foreach(enemyGrouping enemyGroup in flag.enemyGroups)
            {
                foreach(enemySpawn enemy in enemyGroup.enemySpawns)
                {
                    enemyTable.Add(new enemyData());
                }
            }
        }
    }


    //TODO:
    // -check both players are ready, once so send signal for enemy stage logic in 3 seconds

    public void damageEnemy(int index, int dam)
    {
        //TODO
    }

    public void killEnemy(int index)
    {
        //TODO
    }

    public Vector2 getYukiPosition() { return YukiBody.position; }
    public Vector2 getMaiPosition() { return MaiBody.position; }
    public Vector2 getCloserPlayerTo(Vector2 target)
    {
        float distanceToYuki = (YukiBody.position - target).magnitude;
        float distanceToMai = (MaiBody.position - target).magnitude;
        if(distanceToYuki <= distanceToMai)
            return getYukiPosition();
        else
            return getMaiPosition();
    }


    // for the black bar fade in effect
    IEnumerator doFadeInTransition()
    {
        yield return new WaitForSeconds(.15f);

        Vector3 leftStartPos = leftPanel.transform.position;
        Vector3 rightStartPos = rightPanel.transform.position;

        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float curTime = Time.time;
            if(curTime - startTime >= fadeTime)
            {
                leftPanel.transform.position = leftStartPos + Vector3.left * fadeDistance;
                rightPanel.transform.position = rightStartPos + Vector3.right * fadeDistance;
                return true;
            }
            else
            {
                float timeRatio = (curTime - startTime) / fadeTime;
                leftPanel.transform.position = leftStartPos + Vector3.left * fadeDistance * timeRatio;
                rightPanel.transform.position = rightStartPos + Vector3.right * fadeDistance * timeRatio;
                return false;
            }
        });
    }


    //each client keeps track of which stage flag they are on and when they reach the end of one flag, they wait for the other player to also reach that before continuing the stage
    //(to avoid massive desync)
    [Serializable]
    public struct stageFlag
    {
        public List<enemyGrouping> enemyGroups;
    }

    [Serializable]
    public struct enemyGrouping
    {
        public List<enemySpawn> enemySpawns;
        public float delayTillNextGroup;
    }

    [Serializable]
    public struct enemySpawn
    {
        public GameObject enemyPrefab;
        public Vector2 spawnLocation;
    }

}
public class enemyData
{
    public int damageFromYuki, damagefromMai = 0; //shared info wise every x damage done
    public bool isAlive = true; //updated always with rpc when changed
    public bool isActive = false; //local only
    GameObject currentObject = null; //local only

    public int getDamageTaken()
    {
        return damagefromMai + damageFromYuki;
    }
}