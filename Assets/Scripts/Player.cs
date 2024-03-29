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
    public NetworkVariable<bool> isShooting = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isFocusing = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); //used for visuals of other player
    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> facingLeft = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public character thischar = character.Yuki;
    Rigidbody2D thisBody;

    public ComplexPattern regularShot, focusedShot;
    public List<BulletPattern> homingShots;

    public Collider2D hitbox, clearbox, grazebox;
    public SpriteRenderer spriteRendOutput;
    public GameObject iFrameWhiteFlash, iFrameRedFlash;
    public SpriteMask iFrameWhiteSpriteMask, iFrameRedSpriteMask;
    public float iframeTime = 1f;
    public float iframeflashTime = 20f / 60f;
    public float redFlashEffectTime = 20f / 60f;
    Coroutine redFlashCoro;
    bool inIframes = false;
    Coroutine iFrameCoro;
    public bool bypassDamageDebug = false;
    Animator anim;
    bool onBombCooldown = false;
    float bombCooldown = .5f;
    public NetworkVariable<int> score = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    bool isDead = false;


    void Start()
    {
        if(GlobalVars.forceRemoveDebugInvul)
            bypassDamageDebug = false;

        thisBody = gameObject.GetComponent<Rigidbody2D>();
        try
        { anim = gameObject.GetComponent<Animator>(); }
        catch
        { anim = null; }

        isMoving.OnValueChanged += onAnimUpdateMoving;
        facingLeft.OnValueChanged += onAnimUpdateFacingLeft;
        score.OnValueChanged += updateScore;

        if(GlobalVars.isPlayingYuki == thischar is character.Yuki) //make hitbox visible if playing character
        {
            if(hitboxVisual != null)
                hitboxVisual.SetActive(true);

            if(hitbox != null)
                StartCoroutine(hitCheck());
            if(grazebox != null)
                StartCoroutine(doGrazeCheck());
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
        StartCoroutine(shootingLogic());
    }

    void onAnimUpdateMoving(bool prevValue, bool newValue) { if(anim != null) anim.SetBool("isMoving", newValue);  }
    void onAnimUpdateFacingLeft(bool prevValue, bool newValue) { if(anim != null) anim.SetBool("facingLeft", newValue); }


    void Update()
    {
        if(IsOwner && !isDead)
        {
            doInputStuff();
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

        if(Mathf.Abs(movement.x) > 0.001f)
        {
            isMoving.Value = true;
            facingLeft.Value = movement.x < 0;
            if(anim != null)
            {
                anim.SetBool("isMoving", true);
                anim.SetBool("facingLeft", movement.x < 0);
            }
        }
        else
        {
            isMoving.Value = false;
            if(anim != null)
                anim.SetBool("isMoving", false);
        }

        bool currentlyFocusing = Input.GetButton("Focus") || (GlobalVars.useController && Input.GetButton("FocusJoy"));
        bool currentlyShooting = Input.GetButton("Shoot") || (GlobalVars.useController && Input.GetButton("ShootJoy"));

        if((Input.GetButtonDown("Bomb") || Input.GetButtonDown("BombJoy")) && !onBombCooldown)
        {
            StageHandler.Singleton.useBomb();
            StartCoroutine(bombcooldownTimer());
        }

        //TODO: bomb and cooldown on hitting bomb?
        //also maybe bomb should hurt all "active" enemies a specific amount

        thisBody.velocity = movement * baseSpeed * (currentlyFocusing ? focusSpeedMult : currentlyShooting ? shootSpeedMult : 1f);

        if(IsOwner)
        {
            //Debug.Log("perms: shoot: "+isShooting.WritePerm + "; focus:"+isFocusing.WritePerm);
            isShooting.Value = currentlyShooting;
            isFocusing.Value = currentlyFocusing;
        }

        //TODO
        //shooting
        //focus
        //also got to do the graze mechanic eventually
        //bombs
    }

    IEnumerator shootingLogic()
    {
        yield return new WaitUntil(delegate()
        {
            if(isShooting.Value)
            {
                if(thischar is character.Yuki)
                    stageSFXHandler.Singleton.yukiBullets.playSFX();
                else
                    stageSFXHandler.Singleton.maibullets.playSFX();


                if(isFocusing.Value)
                {
                    updateLockonTarget();
                    if(focusedShot.isFinished())
                        focusedShot.reset();
                    focusedShot.shootAllPatterns();
                }
                else
                {
                    if(regularShot.isFinished())
                        regularShot.reset();
                    regularShot.shootAllPatterns();
                }
            }
            return false;
        });
    }

    void updateLockonTarget()
    {
        Rigidbody2D target = StageHandler.Singleton.getClosestEnemyTo(thisBody.position);
        for(int i = 0; i < homingShots.Count; i++)
        {
            homingShots[i].patternDat.trackTarget = PatternData.playerToTarget.SpecificRigidbody;
            homingShots[i].patternDat.customRigidbodyTarget = target;
        }
    }

    IEnumerator hitCheck()
    {
        yield return new WaitUntil(delegate()
        {
            if(!inIframes)
            {
                Collider2D[] list = new Collider2D[4];
                int hits = hitbox.OverlapCollider(KiroLib.getBulletFilter(), list);
                if(hits > 0 && !bypassDamageDebug)
                    StageHandler.Singleton.playerGotHit();
            }
            return false;
        });
    }

    IEnumerator doGrazeCheck()
    {
        yield return new WaitUntil(delegate()
        {
            Collider2D[] possibles = new Collider2D[4];
            int hits = grazebox.OverlapCollider(KiroLib.getBulletFilter(), possibles);
            bool grazedThisFrame = false;
            if(hits > 0)
            {
                for(int i = 0; i < hits; i++)
                {
                    if(!possibles[i].gameObject.tag.Equals("Grazed"))
                    {
                        if(!grazedThisFrame)
                        {
                            grazedThisFrame = true;
                            stageSFXHandler.Singleton.graze.playSFX();
                        }

                        StageHandler.Singleton.gainScore(2); //grazing a bullet is worth 2 points
                        possibles[i].gameObject.tag = "Grazed";
                        StageHandler.Singleton.spawnScoreIndicator(20, (Vector2)possibles[i].gameObject.transform.position);
                        StageHandler.Singleton.tryPlayingGrazeSFX();
                    }
                }
            }
            return false;
        });
    }

    public void updateFlashSpriteMasks()
    {
        iFrameWhiteSpriteMask.sprite = spriteRendOutput.sprite;
        iFrameRedSpriteMask.sprite = spriteRendOutput.sprite;
    }

    IEnumerator iFrameCounter()
    {
        //TODO: iframe visual effect
        inIframes = true;
        float startTime = Time.time;
        float targetTime = iframeTime + startTime;

        yield return new WaitUntil(delegate() //white flash effect
        {
            updateFlashSpriteMasks();
            iFrameWhiteFlash.SetActive((Time.time - startTime) % iframeflashTime >= (iframeTime % iframeflashTime) / 2f);
            if(Time.time >= targetTime)
                return true;
            else
                return false;
        });
        
        iFrameWhiteFlash.SetActive(false);
        inIframes = false;
    }

    public void triggerRedFlashEffect()
    {
        if(redFlashCoro != null)
            StopCoroutine(redFlashCoro);

        redFlashCoro = StartCoroutine(redFlashEffect());
    }

    IEnumerator redFlashEffect()
    {
        updateFlashSpriteMasks();
        iFrameRedFlash.SetActive(true);
        float startTime = Time.time;
        float targetTime = startTime + redFlashEffectTime;
        yield return new WaitUntil(delegate()
        {
            updateFlashSpriteMasks();
            if(Time.time >= targetTime)
                return true;
            else
                return false;
        });
        iFrameRedFlash.SetActive(false);
    }

    IEnumerator bombcooldownTimer()
    {
        onBombCooldown = true;
        yield return new WaitForSeconds(bombCooldown);
        onBombCooldown = false;
    }

    public void giveIframes()
    {
        if(!inIframes)
            StartCoroutine(iFrameCounter());
    }

    public void clearProjectiles() //should be called when a hit is registered and sync
    {
        if(clearbox != null)
        {
            Collider2D[] list = new Collider2D[32];
            int hits = clearbox.OverlapCollider(KiroLib.getBulletFilter(), list);
            if(hits > 0)
            {
                for(int i = 0; i < hits; i++)
                {
                    try
                    {
                        list[i].gameObject.GetComponent<bulletDestroyHandler>().destroy();
                    }
                    catch
                    {
                        Destroy(list[i].gameObject);
                    }
                }
            }
        }
    }

    public void doDeathExplosion()
    {
        isDead = true;
        StopAllCoroutines();
        thisBody.velocity = Vector2.zero;
        iFrameRedFlash.SetActive(false);
        iFrameWhiteFlash.SetActive(false);

        enemyDeathEffects.Singleton.doPlayerDeathEffectAt(gameObject.transform.position);
        if(stageSFXHandler.Singleton != null && stageSFXHandler.Singleton.playerDeath != null)
            stageSFXHandler.Singleton.playerDeath.playSFX();
            
        StartCoroutine(shrinkIntoNothingDeath());
    }

    public float timeToShrinkIntoDeath = .2f;
    IEnumerator shrinkIntoNothingDeath()
    {
        Vector3 oldScale = gameObject.transform.localScale;
        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / timeToShrinkIntoDeath;
            if(timeRatio >= 1f)
            {
                gameObject.transform.localScale = Vector3.forward * oldScale.z;
                return true;
            }
            else
            {
                gameObject.transform.localScale = new Vector3(oldScale.x, oldScale.y, 0f) * (1f - timeRatio) + Vector3.forward * oldScale.z;
                return false;
            }
        });
    }

    public void updateScore(int oldScore, int newScore) { StageHandler.Singleton.updateScore(); }
    public enum character { Yuki, Mai }
}
