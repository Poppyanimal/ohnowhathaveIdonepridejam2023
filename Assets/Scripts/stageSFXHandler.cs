using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stageSFXHandler : MonoBehaviour
{
    public static stageSFXHandler Singleton;
    public sfxRotator enemyDeath, enemyBullets, playerBullets, playerBomb, playerDeath, bossDeath;

    void Start()
    {
        Singleton = this;
    }

}
