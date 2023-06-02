using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KiroLib : MonoBehaviour
{
    public static Vector2 rotateVector2(float angle, Vector2 startV)
    {
        float angleR = angle * Mathf.Deg2Rad;
        return new Vector2(
            Mathf.Cos(angleR)*startV.x + Mathf.Sin(angleR)*startV.y,
            Mathf.Sin(angleR)*startV.x + Mathf.Cos(angleR)*startV.y);
    }

    public static float angleToTarget(Vector3 origin, Vector3 target)
    {
        return Mathf.Atan2(target.y - origin.y, target.x - origin.x) * 180 / Mathf.PI + 90;
    }
    
    public static ContactFilter2D getBulletFilter()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = LayerMask.GetMask("Bullet");
        return filter;
    }
    public static ContactFilter2D getPBulletFilter()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = LayerMask.GetMask("PlayerBullet");
        return filter;
    }
}