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
    int currentEnemyIndex = 0;

    public List<stageFlag> stageFlags;

    [HideInInspector]
    public List<enemyData> enemyTable = new();

    //the boss fight

    bool playerOneWaitingForFlag, playerTwoWaitingForFlag = false;
    int playerOneFlagNumberWaitingOn, playerTwoFlagNumberWaitingOn = 0;





    void Awake() { Color col = backgroundDimming.color; col.a = GlobalVars.screenDim; backgroundDimming.color = col; Singleton = this; }
    void Start() { StartCoroutine(doFadeInTransition()); }


    IEnumerator doStageStart()
    {
        //TODO:
        //title card of makai / 魔界 and subtitle of sub area, fade in into fade out into first enemy grouping
        yield return new WaitForSeconds(fadeTime + 1f);

        //...
        //now done with title card and stuff and any possible dialogue, start enemy groupings

        if(stageFlags.Count > 0)
        {
            if(IsHost)
                playerOneMarkWaitingForFlag(0);
            else
                playerTwoMarkWaitingForFlag(0);
        }
    }

    //
    // Flag Netcode
    //

    void playerOneMarkWaitingForFlag(int flag)
    {
        if(IsHost)
        {
            playerOneFlagNumberWaitingOn = flag;
            playerOneWaitingForFlag = true;
            markPlayerAsWaitingForFlagClientRpc(flag);
        }
    }
    void playerTwoMarkWaitingForFlag(int flag)
    {
        if(!IsHost)
        {
            playerTwoFlagNumberWaitingOn = flag;
            playerTwoWaitingForFlag = true;
            playerTwoMarkWaitingForFlagServerRpc(flag);
        }
    }
    [ServerRpc]
    void playerTwoMarkWaitingForFlagServerRpc(int flag)
    {
        playerTwoFlagNumberWaitingOn = flag;
        playerTwoWaitingForFlag = true;
        markPlayerAsWaitingForFlagClientRpc(flag, 2);
    }
    [ClientRpc]
    void markPlayerAsWaitingForFlagClientRpc(int flag, int player = 1)
    {
        if(player == 1)
        {
            playerOneFlagNumberWaitingOn = flag;
            playerOneWaitingForFlag = true;
        }
        else if(player == 2)
        {
            playerTwoFlagNumberWaitingOn = flag;
            playerTwoWaitingForFlag = true;
        }

        if(IsHost && playerOneWaitingForFlag && playerTwoWaitingForFlag)
        {
            if(playerOneWaitingForFlag != playerTwoWaitingForFlag)
            {
                Debug.LogError("Players are waiting for different flags, a desync may have occured");
            }
            unwaitAndProgressToFlagClientRpc(flag);
        }
    }
    [ClientRpc]
    void unwaitAndProgressToFlagClientRpc(int flag)
    {
        playerOneWaitingForFlag = false;
        playerTwoWaitingForFlag = false;
        doLogicForSequence(flag);
    }


    //
    // Progression of the flag
    //

    IEnumerator doLogicForSequence(int flagIndex)
    {
        yield return new WaitForSeconds(.1f);
        //TODO

        //alllll the logic for this flag

        //
        //




        if(flagIndex + 1 < stageFlags.Count)
        {
            if(IsHost)
            {
                playerOneMarkWaitingForFlag(flagIndex+1);
            }
            else
            {
                playerTwoMarkWaitingForFlag(flagIndex+1);
            }
        }
        else
        {
            //TODO
            //all stage flags are done
            //finish the stage and close it out properly by *safely* disconnecting the two players (aka no kick to menu immediately),
            //a scoreboard of their score and stats (damage taken, hearts obtained, bombs used per player / total), showing the difficult,
            //and saving the highscore locally
        }
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



    public int getDamageForEnemy(int index) { return enemyTable[index].damagefromMai + enemyTable[index].damagefromMai; }

    public void damageEnemy(int index, int dam)
    {
        //TODO
    }

    public void killEnemy(int index)
    {
        //TODO
    }

    public void disableEnemy(int index)
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