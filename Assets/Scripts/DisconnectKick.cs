using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class DisconnectKick : MonoBehaviour
{
    public static DisconnectKick Singleton;

    public static bool bypassDisconnectCheck = false;

    void Start()
    {
        if(Singleton == null)
        {
            Singleton = this;
            NetworkManager.Singleton.OnClientDisconnectCallback += disconnectHappened;
        }
    }

    public static void disconnectHappened(ulong id)
    {
        if(bypassDisconnectCheck)
            return;
        Debug.LogError("Connection Closed Unexpectedly");
        GlobalVars.connectionClosedUnexpectedly = true;
        SceneManager.LoadScene(GlobalVars.mainMenuName, LoadSceneMode.Single);
        if(!NetworkManager.Singleton.ShutdownInProgress)
            NetworkManager.Singleton.Shutdown();
    }
}
