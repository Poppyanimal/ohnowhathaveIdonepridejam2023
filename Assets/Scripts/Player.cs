using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public float baseSpeed = 5f;
    public float shootSpeedMult = .7f; //doesnt apply if focused
    public float focusSpeedMult = .3f;
    public GameObject hitboxVisual;


    //Move speed slightly slower while shooting?
    public NetworkVariable<bool> isShooting, isFocusing; //used for visuals of other player
    public character thischar = character.Yuki;
    Rigidbody2D thisBody;


    void Start()
    {
        thisBody = gameObject.GetComponent<Rigidbody2D>();

        if(GlobalVars.isPlayingYuki == thischar is character.Yuki && hitboxVisual != null) //make hitbox visible if playing character
        {
            hitboxVisual.SetActive(true);
        }

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
        Vector2 movement = Vector2.zero;

        //Debug.Log(Input.GetAxisRaw("Horizontal") + "; " +Input.GetAxisRaw("Vertical"));

        if(Mathf.Abs(Input.GetAxisRaw("Horizontal")) >= GlobalVars.inputDeadzone)
            movement.x = Input.GetAxisRaw("Horizontal");
        else if(GlobalVars.useController && Mathf.Abs(Input.GetAxisRaw("HorizontalJoy")) >= GlobalVars.inputDeadzone)
            movement.x = Input.GetAxisRaw("HorizontalJoy");

        if(Mathf.Abs(Input.GetAxisRaw("Vertical")) >= GlobalVars.inputDeadzone)
            movement.y = Input.GetAxisRaw("Vertical");
        else if(GlobalVars.useController && Mathf.Abs(Input.GetAxisRaw("VerticalJoy")) >= GlobalVars.inputDeadzone)
            movement.y = Input.GetAxisRaw("VerticalJoy");

        movement = movement.normalized;

        bool currentlyFocusing = Input.GetButton("Focus") || (GlobalVars.useController && Input.GetButton("FocusJoy"));
        bool currentlyShooting = Input.GetButton("Shoot") || (GlobalVars.useController && Input.GetButton("ShootJoy"));

        thisBody.velocity = movement * baseSpeed * (currentlyFocusing ? focusSpeedMult : currentlyShooting ? shootSpeedMult : 1f);

        //TODO
        //shooting
        //focus
        //also got to do the graze mechanic eventually
        //bombs
    }


    public enum character { Yuki, Mai }
}
