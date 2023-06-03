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
    //TODO: change stageFlag into a scriptable object to make testing sequences in a test environment possible
    //ei, a test singleplayer space can exist to load a since stageFlag and run through it

    [HideInInspector]
    public List<enemyData> enemyTable = new();

    //the boss fight

    bool playerOneWaitingForFlag, playerTwoWaitingForFlag = false;
    int playerOneFlagNumberWaitingOn, playerTwoFlagNumberWaitingOn = 0;





    void Awake() { Color col = backgroundDimming.color; col.a = GlobalVars.screenDim; backgroundDimming.color = col; Singleton = this; }
    void Start() { StartCoroutine(doFadeInTransition()); StartCoroutine(doStageStart()); }


    IEnumerator doStageStart()
    {
        Debug.Log("stage start reached");
        initEnemyTable();
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
        else
        {
            Debug.LogError("Expected stage flags to be set!");
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
        else
        {
            Debug.LogError("Only the host should be calling to set player one's ready state!");
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
        else
        {
            Debug.LogError("Only the connecting client should be calling to set player two's ready state!");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void playerTwoMarkWaitingForFlagServerRpc(int flag)
    {
        Debug.Log("Connecting client requests flag waiting");
        playerTwoFlagNumberWaitingOn = flag;
        playerTwoWaitingForFlag = true;
        markPlayerAsWaitingForFlagClientRpc(flag, 2);
    }
    [ClientRpc]
    void markPlayerAsWaitingForFlagClientRpc(int flag, int player = 1)
    {
        Debug.Log("General Player flag marking called");
        if(player == 1)
        {
            Debug.Log("Player one is now waiting for flag");
            playerOneFlagNumberWaitingOn = flag;
            playerOneWaitingForFlag = true;
        }
        else if(player == 2)
        {
            Debug.Log("Player two is now waiting for flag");
            playerTwoFlagNumberWaitingOn = flag;
            playerTwoWaitingForFlag = true;
        }
        else
        {
            Debug.LogError("unexpected player number: "+player+" was selected for flag waiting update");
        }

        if(playerOneWaitingForFlag && playerTwoWaitingForFlag)
        {
            Debug.Log("Both players are now waiting to progress");
            if(IsHost)
            {
                Debug.Log("Is host, should now progress");
                if(playerOneWaitingForFlag != playerTwoWaitingForFlag)
                {
                    Debug.LogError("Players are waiting for different flags, a desync may have occured. Attempting weak resync");
                    attemptResyncThenProgressClientRpc(flag, currentEnemyIndex);
                }
                else
                {
                    unwaitAndProgressToFlagClientRpc(flag); 
                }
            }
        }
    }

    [ClientRpc]
    void attemptResyncThenProgressClientRpc(int flag, int curEnemyIndex)
    {
        currentEnemyIndex = curEnemyIndex;
        playerOneWaitingForFlag = false;
        playerTwoWaitingForFlag = false;
        StartCoroutine(doLogicForSequence(flag));
    }

    [ClientRpc]
    void unwaitAndProgressToFlagClientRpc(int flag)
    {
        playerOneWaitingForFlag = false;
        playerTwoWaitingForFlag = false;
        StartCoroutine(doLogicForSequence(flag));
    }


    //
    // Progression of the flag
    //

    IEnumerator doLogicForSequence(int flagIndex)
    {
        Debug.Log("new flag starting");
        foreach(enemyGrouping group in stageFlags[flagIndex].enemyGroups)
        {
            foreach(enemySpawn spawn in group.enemySpawns)
            {
                spawnEnemy(currentEnemyIndex, spawn.spawnLocation, spawn.enemyPrefab);
                currentEnemyIndex++;
            }
            yield return new WaitForSeconds(group.delayTillNextGroup);
        }

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
        Debug.Log("Initilizing Enemy Table");
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

    void spawnEnemy(int index, Vector2 position, GameObject enemyPrefab)
    {
        Vector3 pos = (Vector3)position + Vector3.back * .1f;
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        if(index >= enemyTable.Count)
            Debug.LogError("Enemy index is higher than enemy tables length!!! index: "+index+"; table length: "+enemyTable.Count);
        
        enemy.GetComponent<Enemy>().spawnIndexId = index;
        enemyTable[index].currentObject = enemy;
        enemyTable[index].isActive = true;
        if(!enemyTable[index].isAlive)
        {
            disableEnemy(index);
            //TODO
            //instantly play kill visual effect on enemy / remove them
        }
    }

    public void damageEnemy(int index, int dam)
    {
        //sometimes needs netcode sync (every x damage)
        //TODO
    }

    public void killEnemy(int index)
    {
        //needs netcode sync
        //TODO
    }

    public void disableEnemy(int index)
    {
        enemyTable[index].isActive = false;
        enemyTable[index].currentObject = null;
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
    public GameObject currentObject = null; //local only

    public int getDamageTaken()
    {
        return damagefromMai + damageFromYuki;
    }
}