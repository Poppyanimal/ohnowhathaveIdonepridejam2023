using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class StageHandler : NetworkBehaviour
{
    public StageHandler Singleton;
    public SpriteRenderer backgroundDimming;
    public Image leftPanel, rightPanel;
    public Rigidbody2D YukiBody, MaiBody;

    int currentFlag = 0;

    public List<stageFlag> stageFlags;

    //the boss fight




    void Awake() { Color col = backgroundDimming.color; col.a = GlobalVars.screenDim; backgroundDimming.color = col; Singleton = this; }
    void Start() { StartCoroutine(doFadeInTransition()); }


    IEnumerator doLogicForSequence(int flagIndex)
    {
        yield return new WaitForSeconds(.1f);
        //TODO
    }


    //TODO:
    // -when stage loads, transition from black fade in
    // -check both players are ready, once so send signal for enemy stage logic in 3 seconds
    // -generate table of enemies with the following info: yuki damage (shared every X damage), mai damage (shared every X damage),
    //    isAlive (updated through RPC to all players), isActive (local only, used to handle some syncing logic like should do death efect, etc), currentObject (local, once spawned)
    // -when an enemy is spawned, it tracks itself by it's index in this table, allowing it to udte it's entry as needed

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


    // for the black bar fade in effect
    IEnumerator doFadeInTransition()
    {
        yield return new WaitForSeconds(.15f);

        float endDistance = 800;
        float timeToFinish = .4f;
        Vector3 leftStartPos = leftPanel.transform.position;
        Vector3 rightStartPos = rightPanel.transform.position;

        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float curTime = Time.time;
            if(curTime - startTime >= timeToFinish)
            {
                leftPanel.transform.position = leftStartPos + Vector3.left * endDistance;
                rightPanel.transform.position = rightStartPos + Vector3.right * endDistance;
                return true;
            }
            else
            {
                float timeRatio = (curTime - startTime) / timeToFinish;
                leftPanel.transform.position = leftStartPos + Vector3.left * endDistance * timeRatio;
                rightPanel.transform.position = rightStartPos + Vector3.right * endDistance * timeRatio;
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
