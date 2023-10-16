using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using NaughtyAttributes;
using System.Linq;

public class Operator : SingletonMonoBehaviour<Operator>
{
    [Header("Game Settings")]
    [Tooltip("Supresses Twitch chat messages and will store Pennys and medals in a separate test file")]
    public bool testMode;
    [Tooltip("Skips opening titles")]
    public bool skipOpeningTitles;
    [Tooltip("Players must join the room with valid Twitch username as their name; this will skip the process of validation")]
    public bool fastValidation;
    [Tooltip("Start the game in recovery mode to restore any saved data from a previous game crash")]
    public bool recoveryMode;
    [Tooltip("Does not show total Qs per round")]
    public bool hideRoundCount;
    [Tooltip("Limits the number of accounts that may connect to the room (set to 0 for infinite)")]
    [Range(0, 100)] public int playerLimit;

    [Tooltip("Determines the number of players who will be left in the final round")]
    [Range(2, 8)] public int playerFinalCount = 6;

    [Header("Quesion Data")]
    [Tooltip("Load a JSON of the standard question model")]
    public TextAsset questionPack;
    [Tooltip("Load a JSON of the old question model; the string Converted Old Pack will be populated with the pack in the new format")]
    public TextAsset questionPackOld;
    public TextAsset tiebreakerQuestions;
    [TextArea (5,10)] public string convertedOldPack;

    public override void Awake()
    {
        base.Awake();
        if (recoveryMode)
            skipOpeningTitles = true;
    }

    private void Start()
    {
        if(recoveryMode)
            skipOpeningTitles = true;

        HostManager.Get.host.ReloadHost = recoveryMode;
        if (recoveryMode)
            SaveManager.RestoreData();

        if (questionPackOld != null)
            QuestionManager.DecompilePack(questionPackOld, true);
        else if (questionPack != null)
            QuestionManager.DecompilePack(questionPack, false);
        else
            DebugLog.Print("NO QUESTION PACK LOADED; PLEASE ASSIGN ONE AND RESTART THE BUILD", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);

        DataStorage.CreateDataPath();
        GameplayEvent.Log("Game initiated");
        //HotseatPlayerEvent.Log(PlayerObject, "");
        //AudiencePlayerEvent.Log(PlayerObject, "");
        EventLogger.PrintLog();
    }

    [Button]
    public void ProgressGameplay()
    {
        if (QuestionManager.currentPack != null)
            GameplayManager.Get.ProgressGameplay();
    }

    [Button]
    public void Save()
    {
        SaveManager.BackUpData();
    }
}
