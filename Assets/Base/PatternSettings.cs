using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class autoPatternInfo
{
    public bool doAutoPattern = false;
    public int bulletQuantity = 8;
    public Vector2 startingVelocity = Vector2.up;
}

[System.Serializable]
public class SinPatternInfo
{
    public SinPatternAngleInfo SinAngleInfo = new SinPatternAngleInfo();
    public SinPatternVelocityInfo SinVelocityInfo = new SinPatternVelocityInfo();
    float initilizationTime = 0;
    public void initTime()
    {
        if (initilizationTime == 0) initilizationTime = Time.fixedTime;
    }

    public float getTimeSinceInit()
    {
        if(initilizationTime == 0) initTime();
        return Time.fixedTime - initilizationTime;
    }

}

[System.Serializable]
public class SinPatternAngleInfo
{
    public bool useSinAngleOffset = false;
    public float timeOffset = 0f;
    public float amplitude = 15f;
    public float period = 1f;
}

[System.Serializable]
public class SinPatternVelocityInfo
{
    public enum SinVelocityUsage {disabled, x, y, both};
    public SinVelocityUsage useSinVelocity = SinVelocityUsage.disabled;
    public Vector2 timeOffset = Vector2.zero;
    public Vector2 amplitude = new Vector2(2, 2);
    public Vector2 period = new Vector2(1, 1);
}


[System.Serializable]
public class patternDrift
{
    public bool doAngleDrift = false;
    public float angleDrift = 5f;
    int currentCycle = 0;

    public float getCurrentDrift()
    {
        return angleDrift*currentCycle;
    }
    public void drift()
    {
        currentCycle++;
    }
}

[System.Serializable]
public class simplePatternInfo
{
    public Rigidbody2D bulletOverride = null;
    public enum TrackingOverride {Ignore, No_Tracking, Yes_Tracking};
    public TrackingOverride TrackingOverrideChoice = TrackingOverride.Ignore;
    public int bulletQuantity = 12;
    public float angleBetweenBullets = 30f;
    public float angleOffset = 0f;
    public Vector2 startingVelocity = Vector2.up;
    public float angularVelocity = 0f;
    public SinPatternInfo SinInfo = new SinPatternInfo();
    public patternDrift driftInfo = new patternDrift();
}

[System.Serializable]
public class PatternInfo
{
    public float timeBetweenBullets = 0f;
    public float activityDurationOffset = 0.1f;
    public List<simplePatternInfo> sPatternList = new List<simplePatternInfo>()
    {
        new simplePatternInfo()
    };
}

[System.Serializable]
public class PatternData
{
    public bool trackTarget = false;
    public Rigidbody2D targetToTrack = null;

    public autoPatternInfo autoSettings = new autoPatternInfo();
    public PatternInfo patternSettings = new PatternInfo();
}