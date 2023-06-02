using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVars
{
    public const string gameName = "Touhou Adventure of Two (Pride Jam 2023)";
    public const ushort majorVersion = 0;
    public const ushort minorVersion = 1;
    public const bool isDevBuild = true;

    public static int getGameVersion()
    {
        return (isDevBuild ? 0 : 1) + ((minorVersion + majorVersion * 1000) << 1);
    }



    //
    // Player Data
    //

    public static int highScore = 0; //local highscore
    public static float screenDim = .5f;

    public static void loadPlayerDataFromFile()
    {
        //TODO
    }
    public static void savePlayerDataToFile()
    {
        //TODO
    }



    //
    // this data is filled in when the lobby is doing it's three second count down to gameplay
    //

    public static bool isPlayingYuki = true; //usually player one
    public static bool isDifficultyStandard = true; //otherwise, playing on easy


}
