using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class mmhighscorelistings : MonoBehaviour
{
    public TMP_Text easyScoreText, normalScoreText;
    
    void Start()
    {
        easyScoreText.text = GlobalVars.getStringForScore(GlobalVars.getHighscoreEasy());
        normalScoreText.text = GlobalVars.getStringForScore(GlobalVars.getHighscoreNormal());
    }

}
