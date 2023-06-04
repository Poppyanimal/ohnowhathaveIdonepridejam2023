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

    [Serializable]
    public struct enemySpawn
    {
        public GameObject enemyPrefab;
        public Vector2 spawnLocation;
        public List<Enemy.moveInfo> movements;
        public bool despawnAtMovementEnd;
    }
}
