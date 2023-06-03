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
    public TMP_Text amPlayerOneText, amPlayerTwoText, countDownText, difficultyText;
    public GameObject difficultyButton;
    bool playeroneReady, playertwoReady = false;
    Color notReadyCol = new Color(0.6981132f, 0.1045764f, 0.003292995f); Color readyCol = new Color(0.0994485f, 0.6980392f, 0.003921568f);

    bool playerOneIsYuki = true;
    bool difficultyisStandard = true;

    public string nextScene;

    void Start()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            amPlayerOneText.gameObject.SetActive(true);
            difficultyButton.gameObject.SetActive(true);
        }
        else
        {
            amPlayerTwoText.gameObject.SetActive(true);
            difficultyButton.gameObject.SetActive(false);
        }
    }

    //
    //
    //

    public void toggleDifficulty()
    {
        if(IsHost)
        {
            difficultyisStandard = !difficultyisStandard;
            updateDifficultyTextClientRpc(difficultyisStandard);
        }
    }

    [ClientRpc]
    void updateDifficultyTextClientRpc(bool isStandard)
    {
        if(isStandard)
            difficultyText.text = "Difficulty:\nStandard";
        else
            difficultyText.text = "Difficulty:\nApproachable";
    }


    public void readyUp()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            difficultyButton.gameObject.SetActive(false);
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
