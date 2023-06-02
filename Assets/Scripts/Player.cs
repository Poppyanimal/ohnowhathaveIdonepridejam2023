using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public float baseSpeed = 5f;
    public float shootSpeedMult = .9f;
    public float focusSpeedMult = .5f;


    //Move speed slightly slower while shooting?
    public NetworkVariable<bool> isShooting, isFocusing; //used for visuals of other player
    public character thischar = character.Yuki;
    Rigidbody2D thisBody;


    void Start()
    {
        thisBody = gameObject.GetComponent<Rigidbody2D>();

        if(NetworkManager.IsHost) //passes ownership over to other player if host is not playing this character
        {
            if((GlobalVars.isPlayingYuki != thischar is character.Yuki) && IsOwner)
            {
                Debug.Log("passing character of "+thischar+" to the other player");
                ulong otherPlayer = 0;
                foreach(NetworkClient client in NetworkManager.ConnectedClientsList)
                {
                    if(client.ClientId != NetworkManager.Singleton.LocalClientId)
                    {
                        otherPlayer = client.ClientId;
                        break;
                    }
                }

                if(otherPlayer == 0)
                    throw new System.Exception("Could not find any other players!!!");
                
                NetworkObject.ChangeOwnership(otherPlayer);
            }
        }
    }


    void Update()
    {
        if(IsOwner)
        {
            doInputStuff();
        }
        else
        {
            //TODO
            //if shooting, do fake shots, also care about the focusing variable here for visuals and shots
        }
        
    }

    void doInputStuff()
    {
        float axisDeadzone = 0.5f;
        Vector2 movement = Vector2.zero;

        if(Mathf.Abs(Input.GetAxisRaw("Horizontal")) >= axisDeadzone)
        {
            movement.x = Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;
        }
        if(Mathf.Abs(Input.GetAxisRaw("Vertical")) >= axisDeadzone)
        {
            movement.y = Input.GetAxisRaw("Vertical") > 0 ? 1 : -1;
        }

        bool currentlyFocusing = Input.GetButton("Focus");
        bool currentlyShooting = Input.GetButton("Shoot");

        thisBody.velocity = movement * baseSpeed * (currentlyShooting ? shootSpeedMult : 1f) * (currentlyFocusing ? focusSpeedMult : 1f);

        //TODO
        //shooting
        //focus
        //also got to do the graze mechanic eventually
        //bombs
    }


    public enum character { Yuki, Mai }
}
