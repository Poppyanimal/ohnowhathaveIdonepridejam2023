using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using System.Threading;
using Open.Nat;
using System;

public class TestMainMenu : MonoBehaviour
{
    public TMP_Text versionText, debugText;
    public UnityTransport tp;
    public TMP_InputField ipInput, portInput;

    public string levelToTransitionTo;

    Coroutine debugTextCoro;
    
    void Start()
    {
        if(!GlobalVars.mainMenuNetRegDone)
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = BitConverter.GetBytes(GlobalVars.getGameVersion());
            NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCheck;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectCallback;
            GlobalVars.mainMenuNetRegDone = true;
        }

        versionText.text = "Version: " + GlobalVars.majorVersion + "." + GlobalVars.minorVersion + (GlobalVars.isDevBuild ? " DEV" : "");

        //Debug.Log("Unexpected Closure? "+GlobalVars.connectionClosedUnexpectedly);

        if(GlobalVars.connectionClosedUnexpectedly)
        {
            GlobalVars.connectionClosedUnexpectedly = false;
            debugText.color = Color.red;
            debugText.text = "Connection closed unexpectedly!";
        }
    }

    
    void ConnectionApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var clientId = request.ClientNetworkId;
        var ConnectionData = request.Payload;

        if(NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            response.Approved = false;
            response.Pending = false;
            return;
        }

        try
        {
            int vers = BitConverter.ToInt32(ConnectionData, 0);
            response.Approved = (vers == GlobalVars.getGameVersion());
        }
        catch
        {
            Debug.LogError("Invalid Payload from client, rejecting client");
            //response.Reason = "invalid client payload"; //I thought this was supported???
            response.Approved = false;
        }

        response.Pending = false;
        
    }

    void OnClientDisconnectCallback(ulong obj)
    {
        //TODO
        //return to main menu, if server, shutdown server and wait for that to finish
    }

    void OnClientConnectCallback(ulong obj)
    {
        if(NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(levelToTransitionTo, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

    }

    void OnServerStartedCallback(ulong obj)
    {
        //TODO

    }




    //
    //
    //

    public void doHost()
    {
        if(!NetworkManager.Singleton.ShutdownInProgress)
        {
            startDebugCoro("Restarting Network Manager");
            NetworkManager.Singleton.Shutdown();
            StartCoroutine(waitTillShutdown(modeToStart.host));
        }
    }

    public void doConnect()
    {
        if(!NetworkManager.Singleton.ShutdownInProgress)
        {
            startDebugCoro("Restarting Network Manager");
            NetworkManager.Singleton.Shutdown();
            StartCoroutine(waitTillShutdown(modeToStart.client));
        }
    }

    IEnumerator waitTillShutdown(modeToStart selectedMode)
    {
        yield return new WaitUntil(delegate()
        {
            return !NetworkManager.Singleton.ShutdownInProgress;
        });

        if(selectedMode is modeToStart.host)
        {
            //TODO: check if inputfields are valid before continuing
            //TODO: check if state is appropriate to try this
            doUPNP(tp.ConnectionData.Port);
            //TODO

            startDebugCoro("Starting Server");
            NetworkManager.Singleton.StartHost();
            startDebugCoro("Listening For Client");

        }
        else if(selectedMode is modeToStart.client)
        {
            //TODO: check if inputfields are valid before continuing
            //TODO: check if state is appropriate to try this

            startDebugCoro("Trying To Connect To Server");
            NetworkManager.Singleton.StartClient();

        }

    }

    enum modeToStart { none, host, client }

    public void updateIP()
    {
        //TODO validate ip input and respond if not a valid port
        tp.ConnectionData.Address = ipInput.text;
    }

    public void updatePort()
    {
        try
        {
            tp.ConnectionData.Port = ushort.Parse(portInput.text);
        }
        catch
        {
            Debug.LogError("port \""+portInput.text+"\" is not a valid ushort!");
            stopDebugCoro();
            debugText.color = Color.red;
            debugText.text = "Invalid Port";
        }
    }



    //
    // Coro for text debug text updating
    //

    void stopDebugCoro() { if(debugTextCoro != null) StopCoroutine(debugTextCoro); }
    void startDebugCoro(string input) { stopDebugCoro(); debugTextCoro = StartCoroutine(debugCoroLoop(input)); }
    IEnumerator debugCoroLoop(string input, bool isError = false)
    {
        float cycleTime = 1f;
        debugText.text = input;

        if(isError)
            debugText.color = Color.red;
        else
            debugText.color = Color.white;

        while(true)
        {
            for(int count = 0; count <= 3; count++)
            {
                string output = input;
                for(int i = 0; i < count; i++)
                    output+= "."; 

                debugText.text = output;

                yield return new WaitForSeconds(cycleTime/4f);
            }
        }
    }




    //
    //Universal Plug and Play Stuff
    //
    public async void doUPNP(ushort port)
    {
        Protocol protoToUse = Protocol.Udp;

        var discoverer = new NatDiscoverer();
        var cts = new CancellationTokenSource(10000);
        var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

        await device.CreatePortMapAsync(new Mapping(protoToUse, port, port, "temporary UPnP mapping from unity game: "+GlobalVars.gameName));
    }

}
