using Control;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerObject
{
    public string playerClientID;
    public Player playerClientRef;
    public Podium podium;
    public string otp;
    public string playerName;

    public GlobalLeaderboardStrap strap;
    public GlobalLeaderboardStrap cloneStrap;
    public DZAvatarLerpControl floorAvatar;

    public string twitchName;
    public Texture profileImage;

    public bool isInDangerZone;
    public bool isInTiebreaker;
    public bool isEliminated;
    public bool helpUsed;
    public bool nerfed;
    public bool multiplier;

    public int starterBoost;
    public int points;
    public int pointsLastQ;
    public int totalCorrect;
    public string submission;
    public float submissionTime;
    public int tiebreakerSubmission;
    public bool wasCorrect;
    public int dzAppearances;


    public PlayerObject(Player pl, string name)
    {
        playerClientRef = pl;
        otp = OTPGenerator.GenerateOTP();
        playerName = name;
        points = 0;
        //podium = Podiums.GetPodiums.podia.FirstOrDefault(x => x.containedPlayer == null);
        //podium.containedPlayer = this;
    }

    public void ApplyProfilePicture(string name, Texture tx, bool bypassSwitchAccount = false)
    {
        //Player refreshs and rejoins the same game
        if (PlayerManager.Get.players.Count(x => (!string.IsNullOrEmpty(x.twitchName)) && x.twitchName.ToLowerInvariant() == name.ToLowerInvariant()) > 0 && !bypassSwitchAccount)
        {
            PlayerObject oldPlayer = PlayerManager.Get.players.FirstOrDefault(x => x.twitchName.ToLowerInvariant() == name.ToLowerInvariant());
            if (oldPlayer == null)
                return;

            HostManager.Get.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.SecondInstance, "");

            oldPlayer.playerClientID = playerClientID;
            oldPlayer.playerClientRef = playerClientRef;
            oldPlayer.playerName = playerName;

            oldPlayer.strap.playerNameMesh.text = playerName;
            oldPlayer.cloneStrap.playerNameMesh.text = playerName;

            otp = "";
            playerClientRef = null;
            playerName = "";

            if (PlayerManager.Get.pendingPlayers.Contains(this))
                PlayerManager.Get.pendingPlayers.Remove(this);

            HostManager.Get.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.Validated, $"{oldPlayer.playerName}|{oldPlayer.points.ToString()}|{oldPlayer.twitchName}");
            //HostManager.Get.UpdateLeaderboards();
            return;
        }
        otp = "";
        twitchName = name.ToLowerInvariant();
        profileImage = tx;
        floorAvatar = DangerZoneManager.Get.GenerateNewFloorAvatar(this);
        //podium.avatarRend.material.mainTexture = profileImage;

        if(LobbyManager.Get.lateEntry && GameplayManager.Get.roundsPlayed != 0)
        {
            isEliminated = true;
            helpUsed = true;
        }            

        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.Validated, $"{playerName}|{points.ToString()} POINTS|{twitchName}");
        PlayerManager.Get.players.Add(this);
        PlayerManager.Get.pendingPlayers.Remove(this);
        LeaderboardManager.Get.PlayerHasJoined(this);
        SaveManager.BackUpData();

        if (LobbyManager.Get.lateEntry && GameplayManager.Get.roundsPlayed != 0)
        {
            strap.avatarRend.material = LeaderboardManager.Get.grayscaleMat;
            cloneStrap.avatarRend.material = LeaderboardManager.Get.grayscaleMat;
        }
        //HostManager.GetHost.UpdateLeaderboards();
    }

    public void HandlePlayerScoring(string[] submittedAnswers)
    {
        multiplier = (submittedAnswers.Length == 2 && submittedAnswers[1] == "***MULTIPLY***") ? true : false;
        nerfed = submittedAnswers[0] == "***NERF***" ? true : false;
        wasCorrect = submittedAnswers[0] == GameplayManager.Get.mainRound.currentQuestion.answers.FirstOrDefault(x => x.isCorrect).answerText;

        submissionTime = GlobalTimeManager.Get.elapsedTime;

        strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.LockedIn);
        cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.LockedIn);

        AudioManager.Get.Play(AudioManager.OneShotClip.LockIn);

        if (multiplier || nerfed)
        {
            helpUsed = true;
            if (nerfed)
            {
                pointsLastQ = GameplayManager.Get.mainRound.nerfPoints;
                DebugLog.Print($"{playerName} (NERFED, {submissionTime.ToString("0.00")}s)", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
                return;
            }
                
            else
                DebugLog.Print($"{playerName} (MULTIPLIER)", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
        }

        if (wasCorrect)
        {
            totalCorrect++;
            if (!isEliminated)
            {
                pointsLastQ = (multiplier ? 2 : 1) * GameplayManager.Get.mainRound.availablePoints;
                GameplayManager.Get.mainRound.availablePoints--;
                DebugLog.Print($"{playerName} ({submissionTime.ToString("0.00")}s)", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
            }
            else
                DebugLog.Print($"{playerName} ({submissionTime.ToString("0.00")}s)", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        else
        {
            if(!isEliminated)
                DebugLog.Print($"{playerName} ({submissionTime.ToString("0.00")}s)", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
            else
                DebugLog.Print($"{playerName} ({submissionTime.ToString("0.00")}s)", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Red);
        }
    }

    public void HandleTiebreakerSubmission(string[] submittedAnswers)
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.LockIn);
        tiebreakerSubmission = int.TryParse(submittedAnswers[0], out int g) ? g : int.MaxValue;
        submissionTime = GlobalTimeManager.Get.elapsedTime;

        DebugLog.Print($"{playerName}: {tiebreakerSubmission} ({submissionTime.ToString("0.00")}s)", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);

        strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.LockedIn);
        cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.LockedIn);
    }

    public void SetAsInDangerZone()
    {
        isInDangerZone = true;
        strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
        cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
        dzAppearances++;
    }

    public void ClearQuestionVariables()
    {
        nerfed = false;
        multiplier = false;
        pointsLastQ = 0;
        submission = "";
        tiebreakerSubmission = -1;
        submissionTime = 0;
        wasCorrect = false;
    }

    public void ClearRoundVariables()
    {
        points = 0;
        isInTiebreaker = false;

        if (QuestionManager.IsFinalRound() || isEliminated)
        {
            helpUsed = true;
            strap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Unavailable);
            cloneStrap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Unavailable);
        }
        else
        {
            helpUsed = false;
            strap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Available);
            cloneStrap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Available);
        }
    }
}
