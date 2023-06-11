using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stageSFXHandler : MonoBehaviour
{
    public static stageSFXHandler Singleton;
    public bool stageFinished = false;
    public sfxRotator enemyDeath, enemyBullets, yukiBullets, maibullets, playerBomb, playerHit, bossDeath, graze, bells;

    void Start()
    {
        Singleton = this;
    }

}
