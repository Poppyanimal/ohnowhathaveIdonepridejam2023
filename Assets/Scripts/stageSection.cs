using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/stageSection", order = 1)]
public class stageSection : ScriptableObject
{
    public string label;
    public string descr;
    public List<enemyGrouping> enemyGroups;

    
    [Serializable]
    public struct enemyGrouping
    {
        public List<enemySpawn> enemySpawns;
        public float delayTillNextGroup;
    }
}
[System.Serializable]
public class enemySpawn
{
    public GameObject enemyPrefab;
    public Vector2 spawnLocation;
    public List<Enemy.moveInfo> movements;
    public bool despawnAtMovementEnd = true;
    public bool overrideTimeBeforeShooting = false;
    public float timeBeforeShooting = 2f;
    public bool rotateAllPatterns = false;
    public float patternRotationAmount = 0f;
    public bool rotatePatternsBetweenCycles = false;
    public float rotateAmountPerCycle = 0f;
}