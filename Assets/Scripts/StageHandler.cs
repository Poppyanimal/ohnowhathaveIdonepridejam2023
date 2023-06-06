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
    public Material rainbowBulletMaterial;
    public float rainbowBulletCycleTime = 1f; //in seconds
    public Rigidbody2D YukiBody, MaiBody;
    public int syncDamageEveryXDamage = 5;
    public bool bypassNetcodeChecks = false;
    public int flagToBypassTo = 0;

    int currentFlag = 0;
    int currentEnemyIndex = 0;

    public List<stageSection> stageFlags;

    [HideInInspector]
    public List<enemyData> enemyTable = new();
    [HideInInspector]
    public List<GameObject> activeEnemies = new();

    public List<GameObject> yukiBulletPrefabs; //for toggling which layer the bullets are marked as for fake bullets
    public List<GameObject> maiBulletPrefabs;

    //TODO: the boss fight

    bool playerOneWaitingForFlag, playerTwoWaitingForFlag = false;
    int playerOneFlagNumberWaitingOn, playerTwoFlagNumberWaitingOn = 0;

    public int startingHealth = 5;
    int currentHealth;
    public int startingBombs = 2;
    int currentBombsPlayerOne, currentBombsPlayerTwo;





    void Awake() { Color col = backgroundDimming.color; col.a = GlobalVars.screenDim; backgroundDimming.color = col; Singleton = this; }
    void Start() { StartCoroutine(doFadeInTransition()); StartCoroutine(doStageStart()); }


    IEnumerator doStageStart()
    {
        Debug.Log("stage start reached");
        initEnemyTable();
        updatePlayerBulletPrefabs();

        currentHealth = startingHealth;
        currentBombsPlayerOne = startingBombs;
        currentBombsPlayerTwo = startingBombs;

        if(rainbowBulletMaterial != null)
            StartCoroutine(doRainbowBulletAnimation());
        else
            Debug.LogError("Rainbow bullet material is not set!");

        //TODO:
        //title card of makai / 魔界 and subtitle of sub area, fade in into fade out into first enemy grouping
        yield return new WaitForSeconds(fadeTime + 1f);

        //...
        //now done with title card and stuff and any possible dialogue, start enemy groupings

        if(stageFlags.Count > 0)
        {
            if(bypassNetcodeChecks)
            {
                StartCoroutine(doLogicForSequence(flagToBypassTo));
            }
            else
            {
                if(IsHost)
                    playerOneMarkWaitingForFlag(0);
                else
                    playerTwoMarkWaitingForFlag(0);
            }
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
                Debug.Log("Is host, should now progress, starting serverrpc");
                handleProgressionServerRpc(flag);
            }
        }
    }

    [ServerRpc]
    void handleProgressionServerRpc(int flag)
    {
        Debug.Log("server rpc started for flag progression");
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

    [ClientRpc]
    void attemptResyncThenProgressClientRpc(int flag, int curEnemyIndex)
    {
        Debug.Log("attemp resync client rpc started");
        currentEnemyIndex = curEnemyIndex;
        playerOneWaitingForFlag = false;
        playerTwoWaitingForFlag = false;
        StartCoroutine(doLogicForSequence(flag));
    }

    [ClientRpc]
    void unwaitAndProgressToFlagClientRpc(int flag)
    {
        Debug.Log("unwait client rpc started");
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
        currentFlag = flagIndex;
        foreach(stageSection.enemyGrouping group in stageFlags[flagIndex].enemyGroups)
        {
            foreach(enemySpawn spawn in group.enemySpawns)
            {
                spawnEnemy(currentEnemyIndex, spawn);
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
        foreach(stageSection flag in stageFlags)
        {
            foreach(stageSection.enemyGrouping enemyGroup in flag.enemyGroups)
            {
                foreach(enemySpawn enemy in enemyGroup.enemySpawns)
                {
                    enemyTable.Add(new enemyData());
                }
            }
        }
    }



    public int getDamageForEnemy(int index) { return enemyTable[index].damagefromMai + enemyTable[index].damageFromYuki; }

    void spawnEnemy(int index, enemySpawn spawninfo)
    {
        Vector3 pos = (Vector3)spawninfo.spawnLocation + Vector3.back * .1f;
        GameObject enemyObj = Instantiate(spawninfo.enemyPrefab, pos, Quaternion.identity);

        if(index >= enemyTable.Count)
            Debug.LogError("Enemy index is higher than enemy tables length!!! index: "+index+"; table length: "+enemyTable.Count);

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        enemy.spawnIndexId = index;
        enemy.movements = spawninfo.movements;
        enemy.despawnAtMovementEnd = spawninfo.despawnAtMovementEnd;
        enemy.applyAdditionalPatternRot = spawninfo.rotateAllPatterns;
        enemy.additionalPatternRot = spawninfo.patternRotationAmount;
        enemy.rotateAllPatternsBetweenCycles = spawninfo.rotatePatternsBetweenCycles;
        enemy.rotateAmountPerCycle = spawninfo.rotateAmountPerCycle;
        if(spawninfo.overrideTimeBeforeShooting)
            enemy.timeBeforeShooting = spawninfo.timeBeforeShooting;
        enemy.startLogic();

        enemyTable[index].currentObject = enemyObj;
        enemyTable[index].isActive = true;
        activeEnemies.Add(enemyObj);

        if(!enemyTable[index].isAlive)
        {
            enemy.doKillEffect();
            disableEnemy(index);
        }
    }

    public void damageEnemy(int index, int dam = 1)
    {
        int oldDam = GlobalVars.isPlayingYuki ? enemyTable[index].damageFromYuki : enemyTable[index].damagefromMai;

        int newDam = oldDam + dam;
        //Debug.Log("Old damage:"+oldDam+"; newdam: "+newDam+"; isyuki?"+GlobalVars.isPlayingYuki);
        if(GlobalVars.isPlayingYuki)
        {
            enemyTable[index].damageFromYuki = newDam;
        }
        else
        {
            enemyTable[index].damagefromMai = newDam;
        }

        if((int)(oldDam / syncDamageEveryXDamage) != (int)(newDam / syncDamageEveryXDamage))
        {
            attemptSyncDamageServerRpc(index, newDam, GlobalVars.isPlayingYuki);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void attemptSyncDamageServerRpc(int index, int newDamage, bool isYuki)
    {
        int oldDamage = isYuki ? enemyTable[index].damageFromYuki : enemyTable[index].damagefromMai;
        if(newDamage < oldDamage)
            newDamage = oldDamage;
        
        if(isYuki)
            enemyTable[index].damageFromYuki = newDamage;
        else
            enemyTable[index].damagefromMai = newDamage;

        syncDamageClientRpc(index, newDamage, isYuki);
    }

    [ClientRpc]
    public void syncDamageClientRpc(int index, int newDamage, bool isYuki)
    {
        if(isYuki)
            enemyTable[index].damageFromYuki = newDamage;
        else
            enemyTable[index].damagefromMai = newDamage;
    }

    public void killEnemy(int index)
    {
        Debug.Log("kill enemy procced");
        enemyTable[index].isAlive = false;
        if(enemyTable[index].isActive)
        {
            enemyTable[index].currentObject.GetComponent<Enemy>().doKillEffect();
            disableEnemy(index);
        }
        killEnemyServerRpc(index);
    }

    [ServerRpc(RequireOwnership = false)]
    public void killEnemyServerRpc(int index)
    {
        killEnemyClientRpc(index);
    }

    [ClientRpc]
    public void killEnemyClientRpc(int index)
    {
        enemyTable[index].isAlive = false;
        if(enemyTable[index].isActive)
        {
            enemyTable[index].currentObject.GetComponent<Enemy>().doKillEffect();
            disableEnemy(index);
        }
    }

    public void disableEnemy(int index)
    {
        enemyTable[index].isActive = false;
        if(activeEnemies.Contains(enemyTable[index].currentObject))
            activeEnemies.Remove(enemyTable[index].currentObject);
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

    public Rigidbody2D getClosestEnemyTo(Vector2 position)
    {
        Rigidbody2D closestEnemy = null;
        foreach(GameObject activeEnemy in activeEnemies)
        {
            if(((Vector2)activeEnemy.transform.position - position).magnitude <= (closestEnemy != null ? (closestEnemy.position - position).magnitude : 9999f))
                closestEnemy = activeEnemy.GetComponent<Rigidbody2D>();
        }
        return closestEnemy;
    }

    public void playerGotHit()
    {
        int newHealth = currentHealth--;
        if(newHealth <= 0)
        {
            startDeathEffectServerRpc(NetworkManager.Singleton.LocalClientId);
            doDeathEffect();
        }
        else
        {
            takePlayerDamageServerRpc(newHealth);
            startGetHitEffectServerRpc(NetworkManager.Singleton.LocalClientId, GlobalVars.isPlayingYuki);
            doGetHitEffect(GlobalVars.isPlayingYuki);
        }
    }

    public void useBomb()
    {
        int bombs = IsHost ? currentBombsPlayerOne : currentBombsPlayerTwo;
        if(bombs > 0)
        {
            bombs--;
            if(IsHost)
                currentBombsPlayerOne = bombs;
            else
                currentBombsPlayerTwo = bombs;

            updateBombUI();
            useBombServerRpc(GlobalVars.isPlayingYuki, NetworkManager.Singleton.LocalClientId);
            doBombEffect(GlobalVars.isPlayingYuki);
            setPlayerBombsServerRpc(IsHost, bombs);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void takePlayerDamageServerRpc(int newHealth)
    {
        currentHealth = newHealth;
        takePlayerDamageClientRpc(newHealth);
        setPlayerBombsClientRpc(true, startingBombs);
        setPlayerBombsClientRpc(false, startingBombs);
    }
    [ClientRpc]
    public void takePlayerDamageClientRpc(int newHealth)
    {
        currentHealth = newHealth;
    }
    [ServerRpc(RequireOwnership = false)]
    void setPlayerBombsServerRpc(bool isPlayerOne, int bombs)
    {
        setPlayerBombsClientRpc(isPlayerOne, bombs);
    }
    [ClientRpc]
    void setPlayerBombsClientRpc(bool isPlayerOne, int bombs)
    {
        if(isPlayerOne)
            currentBombsPlayerOne = bombs;
        else
            currentBombsPlayerTwo = bombs;
        updateBombUI();
    }

    [ServerRpc(RequireOwnership = false)]
    void useBombServerRpc(bool isYuki, ulong playerId)
    {
        useBombClientRpc(isYuki, playerId);
    }

    [ClientRpc]
    void useBombClientRpc(bool isYuki, ulong playerId)
    {
        if(NetworkManager.Singleton.LocalClientId != playerId)
            doBombEffect(isYuki);
    }

    [ServerRpc(RequireOwnership = false)]
    void startDeathEffectServerRpc(ulong initiatingPlayer)
    {
        startDeathEffectClientRpc(initiatingPlayer);
    }
    [ClientRpc]
    void startDeathEffectClientRpc(ulong initiatingPlayer)
    {
        if(NetworkManager.Singleton.LocalClientId != initiatingPlayer)
            doDeathEffect();
    }

    [ServerRpc(RequireOwnership = false)]
    void startGetHitEffectServerRpc(ulong initiatingPlayer, bool isYuki)
    {
        startGetHitEffectClientRpc(initiatingPlayer, isYuki);
    }
    [ClientRpc]
    void startGetHitEffectClientRpc(ulong initiatingPlayer, bool isYuki)
    {
        if(NetworkManager.Singleton.LocalClientId != initiatingPlayer)
            doGetHitEffect(isYuki);
    }

    void doGetHitEffect(bool isYuki)
    {
        //TODO
        //do hit effect clearing bullets around the one who got hit and giving slight iframes to both characters
    }

    void doDeathEffect()
    {
        //TODO:
        //both players explode
        //slight fade into death screen
        //return to lobby screen
    }

    void doBombEffect(bool isYuki)
    {
        //TODO
        //the actual result of the bomb, starting from that player
    }

    void updateBombUI()
    {
        //TODO
        //just updates the ui with the new bomb values
    }



    
    IEnumerator doFadeInTransition() // for the black bar fade in effect
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

    IEnumerator doRainbowBulletAnimation()
    {
        while(true)
        {
            //Debug.Log("current rainbow float: " + rainbowBulletMaterial.GetFloat("_HueShift"));
            float startTime = Time.time;
            yield return new WaitUntil(delegate()
            {
                float timeRatio = (Time.time - startTime) / rainbowBulletCycleTime;
                //Debug.Log("hue shift cycle started. Time ratio: "+timeRatio);
                if(timeRatio >= 1)
                {
                    rainbowBulletMaterial.SetFloat("_HueShift", 0f);
                    return true;
                }
                else
                {
                    rainbowBulletMaterial.SetFloat("_HueShift", Mathf.PI * (2 * timeRatio));
                    return false;
                }
            });
        }
    }

    void updatePlayerBulletPrefabs()
    {
        int yukiLayer = GlobalVars.isPlayingYuki ? LayerMask.NameToLayer("PlayerBullet") : LayerMask.NameToLayer("PlayerBulletFake");
        int maiLayer = !GlobalVars.isPlayingYuki ? LayerMask.NameToLayer("PlayerBullet") : LayerMask.NameToLayer("PlayerBulletFake");
        for(int i = 0; i < yukiBulletPrefabs.Count; i++)
        {
            yukiBulletPrefabs[i].layer = yukiLayer;
        }
        for(int i = 0; i < maiBulletPrefabs.Count; i++)
        {
            maiBulletPrefabs[i].layer = maiLayer;
        }
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