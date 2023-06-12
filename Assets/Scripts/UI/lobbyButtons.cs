using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class lobbyButtons : MonoBehaviour
{
    public static lobbyButtons Singleton;
    public LobbyHandler LobbHandler;
    public bool debugIsHost = false;
    bool isHost = false;
    bool isReadied = false;
    buttonTypes selectedButton = buttonTypes.undefined;

    public lobbybutton difficultyButton, characterButton, readyButton;
    public TMP_Text readyText;

    public sfxRotator scrollSFX;

    void Start()
    {

        lobbyButtons.Singleton = this;
        if(NetworkManager.Singleton != null)
            isHost = NetworkManager.Singleton.IsHost;

        if(debugIsHost && GlobalVars.isDevBuild)
            isHost = true;

        if(!isHost)
            hideHostButtons();
    }

    bool pressingUp = false;
    bool pressingDown = false;
    void Update()
    {
        if(Mathf.Abs(Input.GetAxisRaw("Vertical")) > GlobalVars.inputDeadzone)
        {
            if(Input.GetAxisRaw("Vertical") > 0f)
            {
                if(!pressingUp)
                {
                    cycleUpSelectionList();
                    pressingUp = true;
                }
                pressingDown = false;
            }
            else
            {
                if(!pressingDown)
                {
                    cycleDownSelectionList();
                    pressingDown = true;
                }
                pressingUp = false;
            }
        }
        else if(Mathf.Abs(Input.GetAxisRaw("VerticalJoy")) > GlobalVars.inputDeadzone)
        {
            if(Input.GetAxisRaw("VerticalJoy") > 0f)
            {
                if(!pressingUp)
                {
                    cycleUpSelectionList();
                    pressingUp = true;
                }
                pressingDown = false;
            }
            else
            {
                if(!pressingDown)
                {
                    cycleDownSelectionList();
                    pressingDown = true;
                }
                pressingUp = false;
            }
        }
        else
        {
            pressingUp = false;
            pressingDown = false;
        }

        if(Input.GetButtonDown("ConfirmJoy") || Input.GetButtonDown("Confirm") || Input.GetButtonDown("Shoot") || Input.GetButtonDown("ShootJoy"))
            selectButton(selectedButton);
    }

    void cycleDownSelectionList()
    {
        //Debug.Log("called cycle down list");
        if(isReadied)
            return;

        if(!isHost)
        {
            if(selectedButton == buttonTypes.ready)
                return;
            selectedButton = buttonTypes.ready;
        }
        else
        {
            switch(selectedButton)
            {
                case buttonTypes.undefined:
                    selectedButton = buttonTypes.difficulty; break;
                case buttonTypes.difficulty:
                    selectedButton = buttonTypes.character; break;
                case buttonTypes.character:
                    selectedButton = buttonTypes.ready; break;
                case buttonTypes.ready:
                    selectedButton = buttonTypes.difficulty; break;    
            }
        }
        hoverOverButton(selectedButton);
    }

    void cycleUpSelectionList()
    {
        //Debug.Log("called cycle up list");
        if(isReadied)
            return;

        if(!isHost)
        {
            if(selectedButton == buttonTypes.ready)
                return;
            selectedButton = buttonTypes.ready;
        }
        else
        {
            switch(selectedButton)
            {
                case buttonTypes.undefined:
                    selectedButton = buttonTypes.ready; break;
                case buttonTypes.difficulty:
                    selectedButton = buttonTypes.ready; break;
                case buttonTypes.character:
                    selectedButton = buttonTypes.difficulty; break;
                case buttonTypes.ready:
                    selectedButton = buttonTypes.character; break;    
            }
        }
        hoverOverButton(selectedButton);
    }

    //
    //

    public void playHoverOverSound()
    {
        if(scrollSFX != null)
            scrollSFX.playSFX();
    }

    public void playSelectSound()
    {
        //ignore, handled by lobby already
    }


    //
    //
    //
    public void hoverOverButton(buttonTypes buttonType)
    {
        if(!isValidButtonToPress(buttonType))
            return;

        playHoverOverSound();
        unhoverAllButtons();
        selectedButton = buttonType;
        
        switch(buttonType)
        {
            case buttonTypes.difficulty:
                difficultyButton.doHoverVisual();
                break;
            case buttonTypes.character:
                characterButton.doHoverVisual();
                break;
            case buttonTypes.ready:
                readyButton.doHoverVisual();
                break;
        }
    }

    public void selectButton(buttonTypes buttonType)
    {
        //Debug.Log("select button called");
        if(!isValidButtonToPress(buttonType))
            return;

        playSelectSound();

        if(buttonType == buttonTypes.ready)
        {
            //Debug.Log("selected ready");
            isReadied = true;
            unhoverAllButtons();
            readyButton.doSelectEffect();
            if(isHost)
                hideHostButtons();
            if(readyText != null)
                readyText.text = "Readied";
            if(LobbHandler != null)
                LobbHandler.readyUp();
        }
        else if(buttonType == buttonTypes.difficulty)
        {
            //Debug.Log("selected difficulty");
            difficultyButton.doSelectEffect();
            if(LobbHandler != null)
                LobbHandler.toggleDifficulty();
        }
        else if(buttonType == buttonTypes.character)
        {
            //Debug.Log("selected character");
            characterButton.doSelectEffect();
            if(LobbHandler != null)
                LobbHandler.swapPlayers();
        }
    }


    //
    //
    //

    void hideHostButtons()
    {
        difficultyButton.gameObject.SetActive(false);
        characterButton.gameObject.SetActive(false);
    }

    void unhoverAllButtons()
    {
        if(difficultyButton.isActiveAndEnabled)
            difficultyButton.doUnhoverVisual();
        if(characterButton.isActiveAndEnabled)
            characterButton.doUnhoverVisual();
        readyButton.doUnhoverVisual();
    }

    bool isValidButtonToPress(buttonTypes buttonType)
    {
        if(isReadied)
            return false;
        else if(!isHost && buttonType != buttonTypes.ready)
            return false;
        else
            return true;
    }

    //
    //
    //

    public void mouseHoverOverDifficulty() { hoverOverButton(buttonTypes.difficulty); }
    public void mouseHoverOverCharacter() { hoverOverButton(buttonTypes.character); }
    public void mouseHoverOverReady() { hoverOverButton(buttonTypes.ready); }
    public void mouseSelectDifficulty() { hoverOverButton(buttonTypes.difficulty); selectButton(buttonTypes.difficulty); }
    public void mouseSelectCharacter() { hoverOverButton(buttonTypes.character); selectButton(buttonTypes.character); }
    public void mouseSelectReady() { hoverOverButton(buttonTypes.ready); selectButton(buttonTypes.ready); }


    public enum buttonTypes { undefined, difficulty, character, ready }
}
