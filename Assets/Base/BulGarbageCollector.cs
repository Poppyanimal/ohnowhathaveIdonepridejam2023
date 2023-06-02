using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulGarbageCollector : MonoBehaviour
{
    //attach to a block to remove all bullets that overlap it's hitbox, a large body is recommended

    Collider2D thisCol;
    ContactFilter2D bulletsToClearFil;
    void Start()
    {
        bulletsToClearFil = KiroLib.getAllBulletsToClearFilter();
        thisCol = gameObject.GetComponent<CompositeCollider2D>();
        if(thisCol == null)
            thisCol = gameObject.GetComponent<Collider2D>();
        else
            Debug.Log("Using Composite Collider!");
    }

    void Update()
    {
        Collider2D[] bulletsToDestroy = new Collider2D[16];
        int results = thisCol.OverlapCollider(bulletsToClearFil, bulletsToDestroy);
        for(int i = 0; i < results; i++)
        {
            Destroy(bulletsToDestroy[i].gameObject);
        }
    }
}
