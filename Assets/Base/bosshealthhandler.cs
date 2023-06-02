using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class bosshealthhandler : MonoBehaviour
{
    
    [SerializeField]
    PatternLogic bossPatternLogic;
    public GameObject getHitParticle;
    public GameObject deathParticle;
    public AudioSource deathSFX;
    //public musiclooppoint stageMusic;
    public List<AudioSource> hurtSFX;
    int curHurtSFXIndex = 0;

    int minHitParticles = 1, maxHitParticles = 2, minDeathParticles = 64, maxDeathParticles = 80;
    int curHealth;
    public int maxHealth = 500;
    Collider2D hurtbox;
    ContactFilter2D playerBulletFilter;


    //public playerPuppet player;

    [SerializeField]
    SpriteRenderer bossSprite;
    [SerializeField]
    GameObject wholeBossObject;
    float fadeTime = 2f;
    float waitTime = 3f;
    Color startingColor;
    Color flashColor;
    int curFlashFrames = 0;
    bool isDead = false;

    [SerializeField]
    string nextScene;

    

    void Start()
    {
        startingColor = bossSprite.color;
        flashColor = Color.gray * startingColor;
        curHealth = maxHealth;
        hurtbox = gameObject.GetComponent<Collider2D>();
        playerBulletFilter = KiroLib.getPBulletFilter();
    }

    void Update()
    {
        if(!isDead)
        {
            checkForSnowballs();
            if(curHealth <= 0)
                commitDie();
            if(curFlashFrames > 0)
            {
                curFlashFrames--;
                if(curFlashFrames == 0)
                    bossSprite.color = startingColor;
            }
        }
    }

    void checkForSnowballs()
    {
        Collider2D[] detectedBullets = new Collider2D[16];
        int results = gameObject.GetComponent<Collider2D>().OverlapCollider(playerBulletFilter, detectedBullets);
        if(results > 0)
        {
            Debug.Log("Boss Health: " + curHealth);
            for(int i = 0; i < results; i++)
            {
               takeDamage(1);
               try
               { detectedBullets[i].gameObject.GetComponent<bulletDestroyHandler>().destroy(); }
               catch
               { Destroy(detectedBullets[i].gameObject); }
            }
        }
    }

    void takeDamage(int damage)
    {
        int numOfParticles = Random.Range(minHitParticles, maxHitParticles);

        for(int i = 0; i < numOfParticles; i++)
        {
            Instantiate(getHitParticle, this.gameObject.transform.position + new Vector3(0f,0f,-.1f), this.gameObject.transform.rotation);
        }

        curFlashFrames = 2;
        bossSprite.color = flashColor;

        curHealth -= damage;

        if(curHealth > 0 && hurtSFX.Count > 0)
        {
            if(hurtSFX[curHurtSFXIndex] != null)
                hurtSFX[curHurtSFXIndex].Play();
            curHurtSFXIndex++;
            if(curHurtSFXIndex >= hurtSFX.Count)
                curHurtSFXIndex = 0;
        }
    }

    void commitDie()
    {
        isDead = true;
        int numOfParticles = Random.Range(minDeathParticles, maxDeathParticles);

        for(int i = 0; i < numOfParticles; i++)
        {
            Instantiate(getHitParticle, this.gameObject.transform.position + new Vector3(0f,0f,-.1f), this.gameObject.transform.rotation);
        }

        Instantiate(deathParticle, this.gameObject.transform.position + new Vector3(0f,0f,-.2f), this.gameObject.transform.rotation);
        deathSFX.Play();

        //stageMusic.stopTracks();
        bossPatternLogic.tellBossSheDied();
        //player.markFightWon();

        Debug.Log("Boss has committed die!");
        StartCoroutine(shrinkIntoDeath());
    }

    IEnumerator shrinkIntoDeath()
    {
        Vector3 startingScale = wholeBossObject.transform.localScale;
        float startingTime = Time.time;

        yield return new WaitUntil(delegate()
        {
            float timeDif = Time.time - startingTime;
            float ratio = timeDif / fadeTime;
            if(timeDif >= fadeTime)
            {
                wholeBossObject.transform.localScale = new Vector3(0f,0f,startingScale.z);
                return true;   
            }
            else
            {
                wholeBossObject.transform.localScale = new Vector3(startingScale.x - startingScale.x * ratio, startingScale.y - startingScale.y * ratio,startingScale.z);
                return false;
            }
        });


        yield return new WaitForSeconds(waitTime);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(nextScene);
    }

    public float getCurrentHealthPercent() { return curHealth / (float)maxHealth; }
}
