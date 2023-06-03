using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class DisconnectKick : MonoBehaviour
{
    public static DisconnectKick Singleton;

    public string fallbackScene;

    void Start()
    {
        Singleton = this;
        NetworkManager.Singleton.OnClientDisconnectCallback += disconnectHappened;
    }

    public void disconnectHappened(ulong id)
    {
        Debug.LogError("Connection Closed Unexpectedly");
        GlobalVars.connectionClosedUnexpectedly = true;
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(fallbackScene, LoadSceneMode.Single);
    }
}
