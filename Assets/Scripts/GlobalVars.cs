using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GlobalVars
{
    public const string gameName = "Touhou Adventure of Two (Pride Jam 2023)";
    public const ushort majorVersion = 0;
    public const ushort minorVersion = 10;
    public const bool isDevBuild = true; //for release set to false
    public const bool forceRemoveDebugInvul = false; //for release set to true
    public const string mainMenuName = "SampleScene";

    public static int getGameVersion()
    {
        return (isDevBuild ? 0 : 1) + ((minorVersion + majorVersion * 1000) << 1);
    }

    public static string getGameVersionString()
    {
        return majorVersion + "." + minorVersion + (isDevBuild ? " DEV" : "");
    }


    //
    // Player Data
    //

    public static int highScoreEasy = 0; //local highscore
    public static int highScoreStandard = 0; //local highscore

    public static bool useController = true;
    public static float screenDim = .5f;
    public static float inputDeadzone = .3f;
    static bool attemptedLoadYet = false;

    public static void loadPlayerDataFromFile()
    {
        attemptedLoadYet = true;

        try
        {
            Debug.Log("load save file called");
            string saveLocation = Application.persistentDataPath + "/save.dat";
            FileStream file;

            if(!File.Exists(saveLocation))
                return;
            else
            {
                file = File.OpenRead(saveLocation);
                BinaryFormatter bf = new BinaryFormatter();
                savedData data;
                try
                {
                    data = (savedData) bf.Deserialize(file);
                }
                catch
                {
                    data = null;
                    Debug.LogError("Data could not be completely read. Exception caught");
                }
                file.Close();

                //parsing saved data
                highScoreEasy = data.highScoreEasy;
                highScoreStandard = data.highScoreStandard;
                //
            }
        }
        catch
        {
            Debug.LogError("Reading of save file completely failed!");
        }

    }
    public static void savePlayerDataToFile()
    {
        Debug.Log("write save file called");
        string saveLocation = Application.persistentDataPath + "/save.dat";
        FileStream file;

        try
        {
            if(!File.Exists(saveLocation))
                File.Create(saveLocation);


            file = File.OpenWrite(saveLocation);

            //entering data to save
            savedData data = new(highScoreEasy, highScoreStandard);
            //

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, data);
            file.Close();
        }
        catch
        {
            Debug.Log("Unable to write save data!");
        }
    }

    public static int getHighscoreEasy()
    {
        if(!attemptedLoadYet)
            loadPlayerDataFromFile();

        return highScoreEasy;
    }
    
    public static int getHighscoreNormal()
    {
        if(!attemptedLoadYet)
            loadPlayerDataFromFile();

        return highScoreStandard;
    }

    public static void setNewEasyHighscore(int s)
    {
        if(s >= highScoreEasy)
        {
            highScoreEasy = s;
            savePlayerDataToFile();
        }
    }

    public static void setNewStandardHighscore(int s)
    {
        if(s >= highScoreStandard)
        {
            highScoreStandard = s;
            savePlayerDataToFile();
        }
    }

    public static string getStringForScore(int score)
    {
        string str = score.ToString();
        str = str.PadLeft(9, '0');
        str = str.PadRight(10, '0');
        return str;
    }


    //
    // Data between scenes
    //

    public static bool mainMenuNetRegDone = false;
    public static bool connectionClosedUnexpectedly = false;
    public static bool connectionClosedDueToVersionMismatch = false;
    public static int endingScore = 0;


    //
    // this data is filled in when the lobby is doing it's three second count down to gameplay
    //

    public static bool isPlayingYuki = true; //usually player one
    public static bool isDifficultyStandard = true; //otherwise, playing on easy


}
[System.Serializable]
public class savedData
{
    public int highScoreEasy;
    public int highScoreStandard;

    public savedData(int hs_Easy, int hs_Std)
    {
        highScoreEasy = hs_Easy;
        highScoreStandard = hs_Std;
    }
}
