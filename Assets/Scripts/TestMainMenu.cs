using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Open.Nat;

public class TestMainMenu : MonoBehaviour
{
    public TMP_Text versionText;
    public UnityTransport tp;
    public TMP_InputField ipInput, portInput;
    
    void Start()
    {
        versionText.text = "Version: " + GlobalVars.majorVersion + "." + GlobalVars.minorVersion + (GlobalVars.isDevBuild ? " DEV" : "");
    }

    public void doHost()
    {
        tryUPNP(tp.ConnectionData.Port);
        //TODO
    }

    public void doConnect()
    {
        //TODO
    }

    public void updateIP()
    {
        tp.ConnectionData.Address = ipInput.text;
        //TODO validate ip input and respond if not a valid port
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
            //TODO: update feedback text to say is not a valid port
        }
        //TODO
    }

    public void tryUPNP(ushort port)
    {
        //TODO
    }

}
