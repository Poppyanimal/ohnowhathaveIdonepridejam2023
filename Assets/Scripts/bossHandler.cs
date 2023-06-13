using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class bossHandler : NetworkBehaviour
{
    public static bossHandler Singleton;
    public NetworkVariable<bool> midBossDefeated;
    public NetworkVariable<bool> finalBossDefeated;

    [HideInInspector]
    public bool isBossActive = false;
    [HideInInspector]
    public GameObject spawnedBossObject = null;

    public GameObject midBossPrefab;
    public GameObject finalBossPrefab;
    public Vector2 midBossSpawnPos, finalBossSpawnPos;

    public int syncEveryXDamage = 5;

    [HideInInspector]
    public List<bossPhaseDamage> midBossDamageTable = new();
    [HideInInspector]
    public List<bossPhaseDamage> finalBossDamageTable = new();

    public int currentbossPhase = 0;



    void Start() { Singleton = this; initilizeDamageTables(); }



    public void startMidBoss()
    {
        currentbossPhase = 0;
        spawnedBossObject = Instantiate(midBossPrefab, (Vector3)midBossSpawnPos + Vector3.forward * 5f, Quaternion.identity);
        boss b = spawnedBossObject.GetComponent<boss>();
        b.type = bossType.midboss;
        isBossActive = true;
        b.startPhase(currentbossPhase);
    }

    public void startFinalBoss()
    {
        currentbossPhase = 0;
        spawnedBossObject = Instantiate(finalBossPrefab, (Vector3)midBossSpawnPos + Vector3.forward * 5f, Quaternion.identity);
        boss b = spawnedBossObject.GetComponent<boss>();
        b.type = bossType.finalboss;
        isBossActive = true;
        b.startPhase(currentbossPhase);
    }



    [ServerRpc(RequireOwnership = false)]
    public void advanceBossPhaseToServerRpc(int index)
    {
        advanceBossPhaseToClientRpc(index);
    }
    [ClientRpc]
    public void advanceBossPhaseToClientRpc(int index)
    {
        if(index <= currentbossPhase || !isBossActive)
            return;

        currentbossPhase = index;

        
        if(index > 0)
            StageHandler.Singleton.doBossBulletClearBombEffect();


        if(spawnedBossObject.GetComponent<boss>().type is bossType.midboss) //assume is midboss
        {

            if(index >= midBossDamageTable.Count) //all phases finished
            {
                isBossActive = false;
                spawnedBossObject.GetComponent<boss>().doDeath();
                if(IsHost)
                    midBossDefeated.Value = true;
            }
            else
            {
                spawnedBossObject.GetComponent<boss>().startPhase(index);
            }
        }
        else //assume is final boss
        {
            if(index >= finalBossDamageTable.Count) //all phases finished
            {
                isBossActive = false;
                spawnedBossObject.GetComponent<boss>().doDeath();
                if(IsHost)
                    finalBossDefeated.Value = true;
            }
            else
            {
                spawnedBossObject.GetComponent<boss>().startPhase(index);
            }
        }
    }

    public void damageBoss(int amount = 1)
    {
        bool isYuki = GlobalVars.isPlayingYuki;
        int oldDam = 0;
        switch(spawnedBossObject.GetComponent<boss>().type is bossType.midboss, isYuki)
        {
            case(true, true):
                oldDam = midBossDamageTable[currentbossPhase].damageFromYuki;
                break;
            case(true, false):
                oldDam = midBossDamageTable[currentbossPhase].damageFromMai;
                break;
            case(false, true):
                oldDam = finalBossDamageTable[currentbossPhase].damageFromYuki;
                break;
            case(false, false):
                oldDam = finalBossDamageTable[currentbossPhase].damageFromMai;
                break;
        }
        int newDam = oldDam + amount;
        int totalDam = newDam + (isYuki ? (spawnedBossObject.GetComponent<boss>().type is bossType.midboss ? midBossDamageTable[currentbossPhase] : finalBossDamageTable[currentbossPhase]).damageFromMai :
            (spawnedBossObject.GetComponent<boss>().type is bossType.midboss ? midBossDamageTable[currentbossPhase] : finalBossDamageTable[currentbossPhase]).damageFromYuki);
        if(totalDam >= spawnedBossObject.GetComponent<boss>().majorPhases[currentbossPhase].hpThisPhase) //if hit will kill phase, try to kill phase
        {
            advanceBossPhaseToServerRpc(currentbossPhase+1);
        }
        else
        {
            switch(spawnedBossObject.GetComponent<boss>().type is bossType.midboss, isYuki)
            {
                case(true, true):
                    midBossDamageTable[currentbossPhase].damageFromYuki = newDam;
                    break;
                case(true, false):
                    midBossDamageTable[currentbossPhase].damageFromMai = newDam;
                    break;
                case(false, true):
                    finalBossDamageTable[currentbossPhase].damageFromYuki = newDam;
                    break;
                case(false, false):
                    finalBossDamageTable[currentbossPhase].damageFromMai = newDam;
                    break;
            }

            if((int)(oldDam / syncEveryXDamage) != (int)(newDam / syncEveryXDamage))
                syncDamageServerRpc(newDam, isYuki, currentbossPhase, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void syncDamageServerRpc(int newDam, bool isYuki, int phaseIndex, ulong initatingClient)
    {
        syncDamageClientRpc(newDam, isYuki, phaseIndex, initatingClient);
    }
    [ClientRpc]
    public void syncDamageClientRpc(int newDam, bool isYuki, int phaseIndex, ulong initatingClient)
    {
        if(initatingClient != NetworkManager.Singleton.LocalClientId)
        {
            boss b = spawnedBossObject.GetComponent<boss>();
            switch(b.type, isYuki)
            {
                case(bossType.midboss, true):
                    midBossDamageTable[phaseIndex].damageFromYuki = newDam;
                    break;
                case(bossType.midboss, false):
                    midBossDamageTable[phaseIndex].damageFromMai = newDam;
                    break;
                case(bossType.finalboss, true):
                    finalBossDamageTable[phaseIndex].damageFromYuki = newDam;
                    break;
                case(bossType.finalboss, false):
                    finalBossDamageTable[phaseIndex].damageFromMai = newDam;
                    break;
            }

            if(phaseIndex == currentbossPhase && getTotalDamageForPhase(b.type is bossType.midboss, phaseIndex) >= b.majorPhases[phaseIndex].hpThisPhase)
            {
                advanceBossPhaseToServerRpc(currentbossPhase + 1);
            }
        }
    }

    int getTotalDamageForPhase(bool isMidBoss, int phaseIndex)
    {
        if(isMidBoss)
        {
            return midBossDamageTable[phaseIndex].damageFromYuki + midBossDamageTable[phaseIndex].damageFromMai;
        }
        else
        {
            return finalBossDamageTable[phaseIndex].damageFromYuki + finalBossDamageTable[phaseIndex].damageFromMai;
        }
    }

    void initilizeDamageTables()
    {
        midBossDamageTable.Clear();
        finalBossDamageTable.Clear();

        foreach(majorPhase mPhase in midBossPrefab.GetComponent<boss>().majorPhases)
        {
            midBossDamageTable.Add(new bossPhaseDamage());
        }
        foreach(majorPhase mPhase in finalBossPrefab.GetComponent<boss>().majorPhases)
        {
            finalBossDamageTable.Add(new bossPhaseDamage());
        }
    }



    public enum bossType { midboss, finalboss, undefined };


}

public class bossPhaseDamage
{
    public int damageFromYuki = 0;
    public int damageFromMai = 0;
}