using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VisualNovelHandler : MonoBehaviour
{
    const float textSpeed = 60f; //characters per second
    public string nextScene = "";

    const float antiAccidentalSkipLag = .1f; //how long to wait in seconds after typing finishes to mark it as finished
    const float minScroll = 0.01f;

    [SerializeField]
    List<scriptAction> theScript = new List<scriptAction>();

    int curActionIndex = -1; //can only ever be at -1 on scene start


    List<boolCoroutinePair> curPriorityMovingActions = new List<boolCoroutinePair>(); //moves before the others activate
    bool priorityMovingDone = true;
    List<boolCoroutinePair> curMovingActions = new List<boolCoroutinePair>();
    List<boolCoroutinePair> curTypingActions = new List<boolCoroutinePair>();

    public GameObject skipSceneScreen;
    bool skipScenePromptActive = false;
    bool skipSceneCancelSelected = true;

    public GameObject yesSkipSelected;
    public GameObject noSkipSelected;

    public Vector2[] yesButtonCorners = new Vector2[2]; //assuming top left then bottom right
    public Vector2[] noButtonCorners = new Vector2[2]; //assuming top left then bottom right

    public AudioSource changeSelectionSound;
    public AudioSource makeSelectionSound;
    public AudioSource continueDialogueSound;
    public AudioSource reverseDialogueSound;

    public Camera curCamera;

    //public vn_animalese animaleseHandler;
    //const bool onlyAnimaleseAfterASemicolon = true;



    void Start() { tryToAdvance(); orderButtonCorners(); }

    void Update()
    {
        if(!skipScenePromptActive)
        {
            if(!priorityMovingDone && isCoroutineListDone(curPriorityMovingActions))
            {
                priorityMovingDone = true;
                doPostPriorityAdvancement();
            }

            if(Input.GetButtonDown("Select") || Input.GetButtonDown("Continue") || Input.GetAxis("Mouse Scrollwheel") > minScroll)
            {
                if(continueDialogueSound != null)
                    continueDialogueSound.Play();
                tryToAdvance();
            }
            else if(Input.GetButtonDown("Rewind Dialogue") || Input.GetAxis("Mouse Scrollwheel") < -minScroll)
            {
                if(reverseDialogueSound != null)
                    reverseDialogueSound.Play();
                rollbackTime();
            }
            if(Input.GetKeyDown("escape"))
            {
                openSkipScenePrompt();
            }
        }
        else
        {
            Vector2 mPos = curCamera.ScreenToWorldPoint(Input.mousePosition);

            if(mPos.x >= yesButtonCorners[0].x && mPos.x <= yesButtonCorners[1].x && mPos.y <= yesButtonCorners[0].y && mPos.y >= yesButtonCorners[1].y) // if mouse hovers left box
            {
                updateSkipSelection(false);

                if(Input.GetKeyDown("mouse 0"))
                    skipScene();
            }
            else if(mPos.x >= noButtonCorners[0].x && mPos.x <= noButtonCorners[1].x && mPos.y <= noButtonCorners[0].y && mPos.y >= noButtonCorners[1].y) // if mouse hovers right box
            {
                updateSkipSelection(true);

                if(Input.GetKeyDown("mouse 0"))
                    closeSkipScenePrompt();
            }
            else if(Input.GetKeyDown("left") || Input.GetKeyDown("right"))
            {
                updateSkipSelection(!skipSceneCancelSelected);
            }
            else if(Input.GetButtonDown("Select") || Input.GetKeyDown("enter"))
            {
                if(skipSceneCancelSelected)
                    closeSkipScenePrompt();
                else
                    skipScene();
            }
            else if(Input.GetKeyDown("escape") || Input.GetButtonDown("Rewind Dialogue"))
            {
                closeSkipScenePrompt();
            }
        }
    }


    void tryToAdvance()
    {
        if(isCoroutineListDone(curPriorityMovingActions) && isCoroutineListDone(curMovingActions) && isCoroutineListDone(curTypingActions))
        {
            curActionIndex++;

            if(curActionIndex >= theScript.Count)
            {
                endScene();
            }
            else
            {
                foreach(scriptAction_show_hide showHide in theScript[curActionIndex].showHides)
                {
                    if(showHide.target.activeSelf == showHide.show)
                    {
                        Debug.LogError("Tried to hide an object during script progression, but it was already hidden. This will break the rollback feature if left unfixed!!!");
                        Debug.LogError("Removing showHide action from script runtime to avoid rollback issues...");
                        theScript[curActionIndex].showHides.Remove(showHide);
                    }
                    showHide.target.SetActive(showHide.show);
                }

                for(int i = 0; i < theScript[curActionIndex].moves.Count; i++)
                {
                    scriptAction_move move = theScript[curActionIndex].moves[i];
                    move.originalLocation = move.target.transform.position;
                    theScript[curActionIndex].moves[i] = move;
                }
                for(int i = 0; i < theScript[curActionIndex].priorityMoves.Count; i++)
                {
                    scriptAction_move move = theScript[curActionIndex].priorityMoves[i];
                    move.originalLocation = move.target.transform.position;
                    theScript[curActionIndex].priorityMoves[i] = move;
                }

                if(theScript[curActionIndex].priorityMoves.Count > 0)
                {
                    priorityMovingDone = false;

                    foreach(scriptAction_move moveAct in theScript[curActionIndex].priorityMoves)
                    {
                        if(moveAct.doSlideIn)
                        {
                            int curPMoveCoroutines = curPriorityMovingActions.Count;
                            boolCoroutinePair newPair;
                            newPair.isDone = false;
                            newPair.action = StartCoroutine(doPMovingAction(moveAct.target, moveAct.timeToSlide, moveAct.newLocation, curPMoveCoroutines));
                            curPriorityMovingActions.Add(newPair);
                        }
                        else
                        {
                            moveAct.target.transform.position = new Vector3(moveAct.newLocation.x, moveAct.newLocation.y, moveAct.target.transform.position.z);
                        }
                    }
                }
                else
                {
                    doPostPriorityAdvancement();
                }
            }
        }
        else
        {
            forcePMovingDone();
            forceMovingDone();
            forceTypingDone();
        }
    }

    void doPostPriorityAdvancement() //done after visibility and priority moves finish, but before all actions finish
    {
        curMovingActions.Clear();
        curTypingActions.Clear();

        foreach(scriptAction_move moveAct in theScript[curActionIndex].moves)
        {
            if(moveAct.doSlideIn)
            {
                int curMoveCoroutines = curMovingActions.Count;
                boolCoroutinePair newPair;
                newPair.isDone = false;
                newPair.action = StartCoroutine(doMovingAction(moveAct.target, moveAct.timeToSlide, moveAct.newLocation, curMoveCoroutines));
                curMovingActions.Add(newPair);
            }
            else
            {
                moveAct.target.transform.position = new Vector3(moveAct.newLocation.x, moveAct.newLocation.y, moveAct.target.transform.position.z);
            }
        }

        foreach(scriptAction_updateText textUpdate in theScript[curActionIndex].textUpdates)
        {
            int curTypingCoroutines = curTypingActions.Count;
            boolCoroutinePair newPair;
            newPair.isDone = false;
            textUpdate.textbox.color = getColorForChar(textUpdate.c);
            newPair.action = StartCoroutine(doTypingAction(textUpdate.textbox, textUpdate.text, textUpdate.c, curTypingCoroutines));
            curTypingActions.Add(newPair);
        }
    }


    void rollbackTime()
    {
        forceTypingDone();
        forceMovingDone();

        foreach(scriptAction_move moveAct in theScript[curActionIndex].priorityMoves)
        {
            moveAct.target.transform.position = new Vector3(moveAct.originalLocation.x, moveAct.originalLocation.y, moveAct.target.transform.position.z);
        }
        foreach(scriptAction_move moveAct in theScript[curActionIndex].moves)
        {
            moveAct.target.transform.position = new Vector3(moveAct.originalLocation.x, moveAct.originalLocation.y, moveAct.target.transform.position.z);
        }
        foreach(scriptAction_show_hide showHide in theScript[curActionIndex].showHides)
        {
            showHide.target.SetActive(!showHide.show);
        }

        if(curActionIndex > 0)
            curActionIndex--;

        foreach(scriptAction_updateText updateText in theScript[curActionIndex].textUpdates)
        {
            updateText.textbox.text = updateText.text;
            updateText.textbox.color = getColorForChar(updateText.c);
        }

        curMovingActions.Clear();
        curTypingActions.Clear();
    }

    void openSkipScenePrompt()
    {
        if(makeSelectionSound != null)
            makeSelectionSound.Play();
        skipSceneScreen.SetActive(true);
        skipScenePromptActive = true;
        updateSkipSelection(true);
    }

    void closeSkipScenePrompt()
    {
        if(makeSelectionSound != null)
            makeSelectionSound.Play();
        skipSceneScreen.SetActive(false);
        skipScenePromptActive = false;
    }

    void updateSkipSelection(bool cancelSelected)
    {
        yesSkipSelected.SetActive(!cancelSelected);
        noSkipSelected.SetActive(cancelSelected);

        if(skipSceneCancelSelected != cancelSelected && changeSelectionSound != null)
            changeSelectionSound.Play();
            
        skipSceneCancelSelected = cancelSelected;
    }

    void skipScene()
    {
        if(makeSelectionSound != null)
            makeSelectionSound.Play();
        endScene();
    }
    void endScene()
    {
        if(nextScene == "")
        {
            Debug.LogError("No next scene set!!!");
        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }



    bool isCoroutineListDone(List<boolCoroutinePair> coList)
    {
        bool done = true;

        foreach(boolCoroutinePair coAct in coList)
            if(coAct.isDone == false)
                done = false;

        if(done)
        {
            foreach(boolCoroutinePair coAct in coList)
                StopCoroutine(coAct.action);
        }

        return done;
    }

    void forcePMovingDone()
    {
        foreach(boolCoroutinePair moveCo in curPriorityMovingActions)
            StopCoroutine(moveCo.action);
        foreach(scriptAction_move movePair in theScript[curActionIndex].priorityMoves)
            movePair.target.transform.position = new Vector3(movePair.newLocation.x, movePair.newLocation.y, movePair.target.transform.position.z);
        priorityMovingDone = true;
        for(int i = 0; i < curPriorityMovingActions.Count; i++)
        {
            boolCoroutinePair curPair = curPriorityMovingActions[i];
            curPair.isDone = true;
            curPriorityMovingActions[i] = curPair;
        }
    }

    void forceMovingDone()
    {
        foreach(boolCoroutinePair moveCo in curMovingActions)
            StopCoroutine(moveCo.action);
        foreach(scriptAction_move movePair in theScript[curActionIndex].moves)
            movePair.target.transform.position = new Vector3(movePair.newLocation.x, movePair.newLocation.y, movePair.target.transform.position.z);
        for(int i = 0; i < curMovingActions.Count; i++)
        {
            boolCoroutinePair curPair = curMovingActions[i];
            curPair.isDone = true;
            curMovingActions[i] = curPair;
        }
    }

    void forceTypingDone()
    {
        foreach(boolCoroutinePair typeCo in curTypingActions)
            StopCoroutine(typeCo.action);
        foreach(scriptAction_updateText textUpdate in theScript[curActionIndex].textUpdates)
            { textUpdate.textbox.text = textUpdate.text; textUpdate.textbox.color = getColorForChar(textUpdate.c); }
        for(int i = 0; i < curTypingActions.Count; i++)
        {
            boolCoroutinePair curPair = curTypingActions[i];
            curPair.isDone = true;
            curTypingActions[i] = curPair;
        }
    }


    void orderButtonCorners()
    {
        float yesTL_X, yesTL_Y, yesBR_X, yesBR_Y;

        if(yesButtonCorners[0].x < yesButtonCorners[1].x)
            { yesTL_X = yesButtonCorners[0].x; yesBR_X = yesButtonCorners[1].x; }
        else  
            { yesTL_X = yesButtonCorners[1].x; yesBR_X = yesButtonCorners[0].x; }

        if(yesButtonCorners[0].y > yesButtonCorners[1].y)
            { yesTL_Y = yesButtonCorners[0].y; yesBR_Y = yesButtonCorners[1].y; }
        else  
            { yesTL_Y = yesButtonCorners[1].y; yesBR_Y = yesButtonCorners[0].y; }

        yesButtonCorners[0] = new Vector2(yesTL_X, yesTL_Y);
        yesButtonCorners[1] = new Vector2(yesBR_X, yesBR_Y);

        float noTL_X, noTL_Y, noBR_X, noBR_Y;
        
        if(noButtonCorners[0].x < noButtonCorners[1].x)
            { noTL_X = noButtonCorners[0].x; noBR_X = noButtonCorners[1].x; }
        else  
            { noTL_X = noButtonCorners[1].x; noBR_X = noButtonCorners[0].x; }

        if(noButtonCorners[0].y > noButtonCorners[1].y)
            { noTL_Y = noButtonCorners[0].y; noBR_Y = noButtonCorners[1].y; }
        else  
            { noTL_Y = noButtonCorners[1].y; noBR_Y = noButtonCorners[0].y; }
        
        noButtonCorners[0] = new Vector2(noTL_X, noTL_Y);
        noButtonCorners[1] = new Vector2(noBR_X, noBR_Y);
    }


    IEnumerator doPMovingAction(GameObject target, float time, Vector2 newPos, int routineArrayIndex)
    {
        Vector3 ogTargetPos = target.transform.position;
        Vector3 posDifference = new Vector3(newPos.x - ogTargetPos.x, newPos.y - ogTargetPos.y, 0f);
        float startTime = Time.time;

        yield return new WaitUntil(delegate()
        {
            float timeDif = Time.time - startTime;
            if(timeDif >= time)
            {
                target.transform.position = new Vector3(newPos.x, newPos.y, target.transform.position.z);
                return true;
            }
            else
            {
                target.transform.position = ogTargetPos + (timeDif/time)*posDifference;
                return false;
            }
        });

        //if(!(routineArrayIndex >= curPriorityMovingActions.Count))
        //{
            boolCoroutinePair pair = curPriorityMovingActions[routineArrayIndex];
            pair.isDone = true; 
            curPriorityMovingActions[routineArrayIndex] = pair;
        //}
    }

    IEnumerator doMovingAction(GameObject target, float time, Vector2 newPos, int routineArrayIndex)
    {
        Vector3 ogTargetPos = target.transform.position;
        Vector3 posDifference = new Vector3(newPos.x - ogTargetPos.x, newPos.y - ogTargetPos.y, 0f);
        float startTime = Time.time;

        yield return new WaitUntil(delegate()
        {
            float timeDif = Time.time - startTime;
            if(timeDif >= time)
            {
                target.transform.position = new Vector3(newPos.x, newPos.y, target.transform.position.z);
                return true;
            }
            else
            {
                target.transform.position = ogTargetPos + (timeDif/time)*posDifference;
                return false;
            }
        });

        //if(!(routineArrayIndex >= curMovingActions.Count))
        //{
            boolCoroutinePair pair = curMovingActions[routineArrayIndex];
            pair.isDone = true; 
            curMovingActions[routineArrayIndex] = pair;
        //}
    }

    IEnumerator doTypingAction(TMP_Text textbox, string text, chara character, int routineArrayIndex)
    {
        bool pastSemicolon = false;
        if(text.Length > 0)
        {
            for(int i = 0; i <= text.Length; i++)
            {
                //if(pastSemicolon)
                    //animaleseHandler.playSoundForCharacter(character, text.Substring(0,1));
                textbox.text = text.Substring(0, i);

                if(!pastSemicolon && i > 0 && text.Substring(i-1, 1).Equals(":"))
                {
                    pastSemicolon = true;
                }

                yield return new WaitForSeconds(1f / textSpeed);
            }
        }
        textbox.text = text;

        yield return new WaitForSeconds(antiAccidentalSkipLag);

        if(!(routineArrayIndex >= curTypingActions.Count))
        {
            boolCoroutinePair pair = curTypingActions[routineArrayIndex];
            pair.isDone = true; 
            curTypingActions[routineArrayIndex] = pair;
        }
    }

    
    [System.Serializable]
    public struct scriptAction
    {
        public List<scriptAction_show_hide> showHides;
        public List<scriptAction_move> priorityMoves;
        public List<scriptAction_move> moves;
        public List<scriptAction_updateText> textUpdates;
    }

    [System.Serializable]
    public struct scriptAction_move
    {
        public GameObject target;
        public Vector2 newLocation;
        public bool doSlideIn;
        public float timeToSlide; //in seconds

        [System.NonSerialized]
        public Vector3 originalLocation; //is updated whenever this struct is initially pulled, allows reversing time
    }

    [System.Serializable]
    public struct scriptAction_show_hide
    {
        public GameObject target;
        public bool show; //should always be different than current state is
    }

    [System.Serializable]
    public struct scriptAction_updateText
    {
        public TMP_Text textbox;
        public string text;
        public chara c;
        public AudioSource speaking_sfx;
    }

    public enum chara
    {
        menus, //Color.white
        green, //Color 69FF8D
        blue //Color 72DFFF
    }

    public Color getColorForChar(chara c)
    {
        Color tempC = Color.white;
        if(c == chara.green)
        {
            tempC.r = 0.4470588f; tempC.g = 0.8745098f; tempC.b = 1f;
        }
        else if(c == chara.blue)
        {  
            tempC.r = 0.4103774f; tempC.g = 1f; tempC.b = 0.5514243f;
        }

        return tempC;
    }


    
    public struct boolCoroutinePair
    {
        public Coroutine action;
        public bool isDone;
    }

}
