using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class emergencyStageTransition : MonoBehaviour
{
    public GameObject theTransition;
    void Awake()
    {
        if(!GlobalVars.isDevBuild)
            theTransition.SetActive(true);
    }

}
