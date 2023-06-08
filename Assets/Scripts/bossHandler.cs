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
    public boss spawnedBossObject = null;

    public boss midBossPrefab;
    public boss finalBossPrefab;

    public int syncEveryXDamage = 5;

    [HideInInspector]
    public List<bossPhaseDamage> midBossDamageTable;
    [HideInInspector]
    public List<bossPhaseDamage> finalBossDamageTable;

    public int currentbossPhase = 0;


    void Start() { Singleton = this; initilizeDamageTables(); }


    public void startMidBoss()
    {
        currentbossPhase = 0;
        //TODO

        if(IsHost)
            midBossDefeated.Value = true;
    }

    public void startFinalBoss()
    {
        currentbossPhase = 0;
        //TODO
        
        if(IsHost)
            finalBossDefeated.Value = true;
    }

    void initilizeDamageTables()
    {
        midBossDamageTable.Clear();
        finalBossDamageTable.Clear();

        foreach(majorPhase mPhase in midBossPrefab.majorPhases)
        {
            midBossDamageTable.Add(new bossPhaseDamage());
        }
        foreach(majorPhase mPhase in finalBossPrefab.majorPhases)
        {
            finalBossDamageTable.Add(new bossPhaseDamage());
        }
    }



    public enum bossType { midboss, finalboss, undefined };


}

public class bossPhaseDamage
{
    int damageFromYuki, damageFromMai = 0;
}