using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;
using System.Linq;
using Control;

public class GameplayManager : SingletonMonoBehaviour<GameplayManager>
{
    [Header("Rounds")]
    public RoundBase mainRound;

    [Header("Question Data")]
    public static int nextQuestionIndex = 0;

    public enum GameplayStage
    {
        RunTitles,
        OpenLobby,
        LockLobby,
        LoadCategory,
        HideCategory,

        RunQuestion,
        ResetPostQuestion,
        EndOfRoundLogic,
        LaunchTiebreaker,
        ResolveTiebreaker,

        RollCredits,
        DoNothing
    };
    public GameplayStage currentStage = GameplayStage.DoNothing;

    public enum Round { None };
    public Round currentRound = Round.None;
    public int roundsPlayed = 0;

    [Button]
    public void ProgressGameplay()
    {
        switch (currentStage)
        {
            case GameplayStage.RunTitles:
                TitlesManager.Get.RunTitleSequence();
                //If in recovery mode, we need to call Restore Players to restore specific player data (client end should be handled by the reload host call)
                //Also need to call Restore gameplay state to bring us back to where we need to be (skipping titles along the way)
                //Reveal instructions would probably be a sensible place to go to, though check that doesn't iterate any game state data itself
                break;

            case GameplayStage.OpenLobby:
                LobbyManager.Get.OnOpenLobby();
                currentStage++;
                break;

            case GameplayStage.LockLobby:
                LobbyManager.Get.OnLockLobby();
                break;

            case GameplayStage.LoadCategory:
                nextQuestionIndex = 0;
                mainRound.nerfPoints = PlayerManager.Get.players.Count(x => !x.isEliminated) / 2;
                InstructionsManager.Get.OnShowInstructions();
                currentStage++;
                break;

            case GameplayStage.HideCategory:
                InstructionsManager.Get.OnHideInstructions();
                currentStage++;
                break;

            case GameplayStage.RunQuestion:
                currentStage = GameplayStage.DoNothing;
                mainRound.RunQuestion();
                break;

            case GameplayStage.ResetPostQuestion:
                nextQuestionIndex++;
                mainRound.ResetForNewQuestion();
                currentStage = nextQuestionIndex == QuestionManager.GetRoundQCount() ? GameplayStage.EndOfRoundLogic : GameplayStage.RunQuestion;
                break;

            case GameplayStage.EndOfRoundLogic:
                //Regular round
                if (!QuestionManager.IsFinalRound())
                {
                    //Uneven DangerZone - go to a tiebreaker
                    if (PlayerManager.Get.players.Count(x => x.isInDangerZone) != DangerZoneManager.Get.GetRoundStartDZSize())
                    {
                        mainRound.InitialiseTieBreaker();
                        currentStage = GameplayStage.LaunchTiebreaker;
                    }
                    else
                    {
                        roundsPlayed++;
                        mainRound.EndOfRoundLogic();
                        currentStage = GameplayStage.DoNothing;
                    }
                }

                //Final round
                else
                {
                    int topScore = PlayerManager.Get.players.Where(x => !x.isEliminated).OrderByDescending(x => x.points).FirstOrDefault().points;

                    //Tie at the top - go to a tiebreaker
                    if(PlayerManager.Get.players.Where(x => !x.isEliminated).Count(x => x.points == topScore) != 1)
                    {
                        mainRound.InitialiseTieBreaker();
                        currentStage = GameplayStage.LaunchTiebreaker;
                    }
                    else
                    {
                        mainRound.GameOverLogic();
                        currentStage = GameplayStage.RollCredits;
                    }
                }
                break;

            case GameplayStage.LaunchTiebreaker:
                mainRound.RunTieBreakerQuestion();
                currentStage = GameplayStage.DoNothing;
                break;

            case GameplayStage.ResolveTiebreaker:
                mainRound.ResolveTiebreaker();
                currentStage = GameplayStage.EndOfRoundLogic;
                break;

            case GameplayStage.RollCredits:
                AudioManager.Get.StopLoop();
                CreditsManager.Get.RollCredits();
                GameplayPennys.Get.UpdatePennysAndMedals();
                currentStage = GameplayStage.DoNothing;
                break;

            case GameplayStage.DoNothing:
                break;
        }
    }
}
