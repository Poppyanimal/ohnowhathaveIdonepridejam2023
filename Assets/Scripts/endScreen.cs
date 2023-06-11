using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class endScreen : MonoBehaviour
{
    public Vector3 bgslideStart, bgslideEnd;
    public float bgSlideTime = 1f;
    public float timeAfterSlide = .3f;
    public float timeBetweenEntries = .5f;
    public float timeToTotalScore = 1f;
    public GameObject slideInBackDrop;
    public TMP_Text gameclearedtext, difficultytext, actualdifficultytext, scoretext, actualscoretext, gameversiontext, actualgameversiontext, thankyouforplayingtext;
    [HideInInspector]
    public List<TMP_Text> preScoreText;
    [HideInInspector]
    public List<TMP_Text> postScoreText;
    public GameObject returnToMainMenuButton; 
    public UnityEngine.UI.Image yukisplash, maisplash;
    public float splashFadeIn = 1f;
    public float splashTargetAlpha = .2f;

    bool canSkip = false;
    bool gotHighscore = false;
    Coroutine effectCoro;
    Coroutine splashCoro;
    public string lobbySceneName = "SampleScene";

    public GameObject newHighscoreText;

    public bool doOnStartupDebug = false;
    public int debugScore = 50;

    void Start()
    {
        if(doOnStartupDebug)
        {
            GlobalVars.endingScore = debugScore;
            startEndingScreen();
        }
    }

    void Update()
    {
        if(canSkip && (Input.GetButtonDown("Shoot") || Input.GetButtonDown("ShootJoy")))
        {
            canSkip = false;
            if(effectCoro != null)
                StopCoroutine(effectCoro);
            if(splashCoro != null)
                StopCoroutine(splashCoro);
        }
    }

    public void startEndingScreen()
    {
        DisconnectKick.bypassDisconnectCheck = true;
        stageSFXHandler.Singleton.stageFinished = true;
        fillLists();
        actualdifficultytext.text = GlobalVars.isDifficultyStandard ? "Standard" : "Approachable";
        actualscoretext.text = getScoreTextForInt(0);
        actualgameversiontext.text = GlobalVars.getGameVersionString();
        gotHighscore = updateHighScore();
        effectCoro = StartCoroutine(doEffect());
    }

    public void fillLists()
    {
        preScoreText.Add(gameclearedtext);
        preScoreText.Add(difficultytext);
        preScoreText.Add(actualdifficultytext);
        preScoreText.Add(scoretext);
        
        postScoreText.Add(gameversiontext);
        postScoreText.Add(actualgameversiontext);
    }

    IEnumerator doSplashTransparency()
    {
        Color splashColor = Color.white;
        splashColor.a = 0f;

        yukisplash.color = splashColor;
        maisplash.color = splashColor;
        yukisplash.gameObject.SetActive(true);
        maisplash.gameObject.SetActive(true);

        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / splashFadeIn;
            if(timeRatio >= 1)
            {
                splashColor.a = splashTargetAlpha;
                yukisplash.color = splashColor;
                maisplash.color = splashColor;
                return true;
            }
            else
            {
                splashColor.a = splashTargetAlpha * timeRatio;
                yukisplash.color = splashColor;
                maisplash.color = splashColor;
                return false;
            }
        });
    }

    IEnumerator doEffect()
    {
        slideInBackDrop.transform.localPosition = bgslideStart;
        Vector3 slideDif = bgslideEnd - bgslideStart;
        float startTime = Time.time;
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time - startTime) / bgSlideTime;
            if(timeRatio >= 1)
            {
                slideInBackDrop.transform.localPosition = bgslideEnd;
                return true;
            }
            else
            {
                slideInBackDrop.transform.localPosition = bgslideStart + timeRatio * slideDif;
                return false;
            }
        });


        if(NetworkManager.Singleton != null && !NetworkManager.Singleton.ShutdownInProgress)
            NetworkManager.Singleton.Shutdown();
        canSkip = true;

        yield return new WaitForSeconds(timeAfterSlide);

        splashCoro = StartCoroutine(doSplashTransparency());
        
        foreach(TMP_Text t in preScoreText)
        {
            t.gameObject.SetActive(true);
            playShowSFX();
            yield return new WaitForSeconds(timeBetweenEntries);
        }

        actualscoretext.gameObject.SetActive(true);
        startTime = Time.time;
        playScoreTickSFX();
        yield return new WaitUntil(delegate()
        {
            float timeRatio = (Time.time -startTime) / timeToTotalScore;
            if(timeRatio >= 1)
            {
                actualscoretext.text = getScoreTextForInt(GlobalVars.endingScore);
                return true;
            }
            else
            {
                actualscoretext.text = getScoreTextForInt((int)(GlobalVars.endingScore * timeRatio));
                return false;
            }
        });
        playShowSFX();
        if(gotHighscore)
            newHighscoreText.SetActive(true);
        
        yield return new WaitForSeconds(timeBetweenEntries);

        
        foreach(TMP_Text t in postScoreText)
        {
            t.gameObject.SetActive(true);
            playShowSFX();
            yield return new WaitForSeconds(timeBetweenEntries);
        }

        thankyouforplayingtext.gameObject.SetActive(true);

        canSkip = false;
        snapToEndScreen();
    }

    void snapToEndScreen()
    {
        DisconnectKick.bypassDisconnectCheck = false;

        actualscoretext.text = getScoreTextForInt(GlobalVars.endingScore);

        for(int i = 0; i < preScoreText.Count; i++)
            preScoreText[i].gameObject.SetActive(true);

        actualscoretext.gameObject.SetActive(true);

        for(int i = 0; i < postScoreText.Count; i++)
            postScoreText[i].gameObject.SetActive(true);

        if(gotHighscore)
            newHighscoreText.SetActive(true);


        thankyouforplayingtext.gameObject.SetActive(true);
        returnToMainMenuButton.gameObject.SetActive(true);

        Color splashcolor = Color.white;
        splashcolor.a = splashTargetAlpha;
        yukisplash.color = splashcolor;
        maisplash.color = splashcolor;
        yukisplash.gameObject.SetActive(true);
        maisplash.gameObject.SetActive(true);

        playShowSFX();
    }

    string getScoreTextForInt(int s)
    {
        string score = s.ToString();
        score = score.PadLeft(9, '0');
        score = score.PadRight(10, '0');
        return score;
    }

    bool updateHighScore() //returns if is new highscore
    {
        bool newHighscore = false;
        if(GlobalVars.isDifficultyStandard)
        {
            if(GlobalVars.endingScore > GlobalVars.highScoreStandard)
            {
                try{ GlobalVars.setNewStandardHighscore(GlobalVars.endingScore); } catch{}
                newHighscore = true;
            }
        }
        else
        {
            if(GlobalVars.endingScore > GlobalVars.highScoreEasy)
            {
                try{ GlobalVars.setNewEasyHighscore(GlobalVars.endingScore); } catch{}
                newHighscore = true;
            }
        }
        return newHighscore;
    }

    void playShowSFX()
    {
        //TODO
    }

    void playScoreTickSFX()
    {
        //TODO
    }

    public void pressReturnToMainMenuButton()
    {
        SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }

}
