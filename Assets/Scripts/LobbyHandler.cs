using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class LobbyHandler : NetworkBehaviour
{
    //TODO: handle the host being able to change difficulty and who is what player
    public Image playeroneReadyImage, playertwoReadyImage, fadeToBlackOverlay;
    public TMP_Text amPlayerOneText, amPlayerTwoText, countDownText, difficultyText, readyButtonText;
    public GameObject difficultyButton, swapCharactersButton;
    bool playeroneReady, playertwoReady = false;
    Color notReadyCol = new Color(0.6981132f, 0.1045764f, 0.003292995f); Color readyCol = new Color(0.0994485f, 0.6980392f, 0.003921568f);

    bool playerOneIsYuki = true;
    bool difficultyisStandard = true;

    public LobbyPlayerSplash playerOneSplash, playerTwoSplash;
    public characterRibbon playerOneRibbonTop, playerOneRibbon, playerTwoRibbon, playerTwoRibbonTop;

    public Material lobbyDifficultyColorMat;
    Coroutine lobbyDiffEffectCoro;
    public float difficultyEffectChangeSpeed = 1f;
    float targetEasyHue = Mathf.PI * 2f;
    public float targetStandardHue;
    bool readyForDebug = false;
    public sfxRotator menuClickSFX, countdownSFX;


    public string nextScene;

    void Start()
    {
        lobbyDifficultyColorMat.SetFloat("_HueShift", targetStandardHue);
        readyForDebug = true;
        if(NetworkManager.Singleton.IsHost)
        {
            amPlayerOneText.gameObject.SetActive(true);
            difficultyButton.gameObject.SetActive(true);
            swapCharactersButton.gameObject.SetActive(true);
        }
        else
        {
            amPlayerTwoText.gameObject.SetActive(true);
            difficultyButton.gameObject.SetActive(false);
            swapCharactersButton.gameObject.SetActive(false);
            checkVersionServerRpc(GlobalVars.getGameVersion());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void checkVersionServerRpc(int vers)
    {
        if(GlobalVars.getGameVersion() != vers)
        {
            GlobalVars.connectionClosedDueToVersionMismatch = true;
            closeConnectionDueToVersionMismatchClientRpc();
        }
    }

    [ClientRpc]
    void closeConnectionDueToVersionMismatchClientRpc()
    {
        if(!IsHost)
        {
            GlobalVars.connectionClosedDueToVersionMismatch = true;
            NetworkManager.Singleton.Shutdown();
        }
    }

    //
    //
    //

    [ContextMenu("DEBUG Difficulty Toggle")]
    public void toggleDifficultyDEBUG()
    {
        if(!readyForDebug)
            return;
        difficultyisStandard = !difficultyisStandard;
        difficultyText.text = difficultyisStandard ? "Difficulty:\nStandard" : "Difficulty:\nApproachable";
        if(lobbyDiffEffectCoro != null)
            StopCoroutine(lobbyDiffEffectCoro);
        lobbyDiffEffectCoro = StartCoroutine(changeDifficultyEffect(difficultyisStandard));
    }

    IEnumerator changeDifficultyEffect(bool isSTD)
    {
        //if going to standard, decrease till at target or less than target
        //if going to approachable, increase till at target or more than target
        Debug.Log("starting diff change effect coro");
        float startValue = lobbyDifficultyColorMat.GetFloat("_HueShift");
        float startTime = Time.time;
        if(isSTD)
        {
            yield return new WaitUntil(delegate()
            {
                float timeDif = Time.time - startTime;
                float newValue = startValue - difficultyEffectChangeSpeed * timeDif;

                if(newValue <= targetStandardHue)
                {
                    lobbyDifficultyColorMat.SetFloat("_HueShift", targetStandardHue);
                    return true;
                }
                else
                {
                    lobbyDifficultyColorMat.SetFloat("_HueShift", newValue);
                    return false;
                }

            });
        }
        else
        {
            yield return new WaitUntil(delegate()
            {
                float timeDif = Time.time - startTime;
                float newValue = startValue + difficultyEffectChangeSpeed * timeDif;

                if(newValue >= targetEasyHue)
                {
                    lobbyDifficultyColorMat.SetFloat("_HueShift", targetEasyHue);
                    return true;
                }
                else
                {
                    lobbyDifficultyColorMat.SetFloat("_HueShift", newValue);
                    return false;
                }

            });
        }
    }

    public void toggleDifficulty()
    {
        if(IsHost)
        {
            menuClickSFX.playSFX();
            difficultyisStandard = !difficultyisStandard;
            updateDifficultyClientRpc(difficultyisStandard);
        }
    }

    public void swapPlayers()
    {
        if(IsHost)
        {
            menuClickSFX.playSFX();
            playerOneIsYuki = !playerOneIsYuki;
            updatePlayerCharacterClientRpc(playerOneIsYuki);
        }
    }

    [ClientRpc]
    void updateDifficultyClientRpc(bool isSTD)
    {
        difficultyText.text = isSTD ? "Difficulty:\nStandard" : "Difficulty:\nApproachable";
        if(lobbyDiffEffectCoro != null)
            StopCoroutine(lobbyDiffEffectCoro);
        lobbyDiffEffectCoro = StartCoroutine(changeDifficultyEffect(isSTD));
    }

    [ClientRpc]
    void updatePlayerCharacterClientRpc(bool poyuki)
    {
        if(poyuki)
        {
            playerOneSplash.switchToYuki();
            playerOneRibbon.selectYuki();
            playerOneRibbonTop.selectYuki();

            playerTwoSplash.switchToMai();
            playerTwoRibbon.selectMai();
            playerTwoRibbonTop.selectMai();
        }
        else
        {
            playerOneSplash.switchToMai();
            playerOneRibbon.selectMai();
            playerOneRibbonTop.selectMai();

            playerTwoSplash.switchToYuki();
            playerTwoRibbon.selectYuki();
            playerTwoRibbonTop.selectYuki();
        }
    }


    public void readyUp()
    {
        menuClickSFX.playSFX();
        readyButtonText.text = "Readied";
        if(NetworkManager.Singleton.IsHost)
        {
            difficultyButton.gameObject.SetActive(false);
            swapCharactersButton.gameObject.SetActive(false);
            playeroneReady = true;
            playeroneReadyImage.color = readyCol;
            playerOneReadyClientRpc();
            if(playeroneReady && playertwoReady)
                startCountdownClientRpc(playerOneIsYuki, difficultyisStandard);
        }
        else
        {
            playertwoReady = true;
            playertwoReadyImage.color = readyCol;
            playerTwoReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void playerTwoReadyServerRpc()
    {
        playertwoReady = true;
        playertwoReadyImage.color = readyCol;

        if(playeroneReady && playertwoReady)
            startCountdownClientRpc(playerOneIsYuki, difficultyisStandard);

    }

    [ClientRpc]
    void playerOneReadyClientRpc()
    {
        playeroneReady = true;
        playeroneReadyImage.color = readyCol;
    }

    void startCountdownToGameStart(bool isPlayerOneYuki, bool isDifficultyStandard)
    {
        GlobalVars.isDifficultyStandard = isDifficultyStandard;
        GlobalVars.isPlayingYuki = (isPlayerOneYuki == NetworkManager.Singleton.IsHost);

        playerOneRibbon.markReady();
        playerOneRibbonTop.markReady();
        playerTwoRibbon.markReady();
        playerTwoRibbonTop.markReady();

        StartCoroutine(doCountdownVisual());
    }

    IEnumerator doCountdownVisual()
    {
        StartCoroutine(doFadeToBlack());
        countDownText.gameObject.SetActive(true);

        float timePerFade = .7f;
        float waitTime = .3f;

        Vector3 scaleIncreasePerFade = Vector3.one * .5f;

        Color col = countDownText.color;

        for(int n = 3; n >= 1; n--)
        {
            countDownText.text = n.ToString();
            countDownText.gameObject.transform.localScale = Vector3.one;
            col.a = 1f;
            countDownText.color = col;

            float startTime = Time.time;

            //TODO: can play sfx each number, its okay for it to be a little late, but not early
            if(countdownSFX != null)
                countdownSFX.playSFX();

            yield return new WaitUntil(delegate()
            {
                if(Time.time - startTime >= timePerFade)
                {   
                    countDownText.gameObject.transform.localScale = Vector3.one + scaleIncreasePerFade;
                    col.a = 0f;
                    countDownText.color = col;
                    return true;
                }
                else
                {
                    float timeRatio = (Time.time - startTime) / timePerFade;
                    countDownText.gameObject.transform.localScale = Vector3.one + timeRatio * scaleIncreasePerFade;
                    col.a = 1f - timeRatio;
                    countDownText.color = col;
                    return false;
                }
            });

            yield return new WaitForSeconds(waitTime);
        }

        if(NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(nextScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    IEnumerator doFadeToBlack()
    {
        float timeToFade = 2.9f;
        float endOpacity = 1f;
        float startTime = Time.time;

        Color col = fadeToBlackOverlay.color;
        col.a = 0;
        fadeToBlackOverlay.color = col;
        fadeToBlackOverlay.gameObject.SetActive(true);

        yield return new WaitUntil(delegate()
        {
            float curTime = Time.time;
            if(curTime - startTime >= timeToFade)
            {
                col.a = endOpacity;
                fadeToBlackOverlay.color = col;
                return true;
            }
            else
            {
                col.a = ((curTime - startTime)/timeToFade) * endOpacity;
                fadeToBlackOverlay.color = col;
                return false;
            }
        });
    }


    [ClientRpc]
    void startCountdownClientRpc(bool isPlayerOneYuki, bool isDifficultyStandard) { startCountdownToGameStart(isPlayerOneYuki, isDifficultyStandard); }
}
