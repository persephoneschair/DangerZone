using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoundBase : MonoBehaviour
{
    public Question currentQuestion = null;
    public QABox questionBox;
    public QABox[] answerBoxes;

    public int availablePoints = 0;
    public int nerfPoints = 0;

    public GameObject eliminationExplosion;
    public Animator tiebreakerAnim;

    public TextMeshProUGUI[] winnerNameMeshes;
    public Animator winnerNameAnim;
    public Animator uiBackingStrap;

    public virtual void RunQuestion()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.QuestionLoop);
        availablePoints = PlayerManager.Get.players.Count(x => !x.isEliminated);
        ResetPlayerVariables(false);
        currentQuestion = QuestionManager.GetQuestion(GameplayManager.nextQuestionIndex);
        questionBox.SetBoxBorderColor(QABox.BoxColor.White);

        foreach(QABox bx in answerBoxes)
            bx.SetBoxBorderColor(QABox.BoxColor.White);

        GlobalTimeManager.Get.StagedResetClock();

        questionBox.boxMesh.text = currentQuestion.questionText;
        for (int i = 0; i < answerBoxes.Length; i++)
            answerBoxes[i].boxMesh.text = currentQuestion.answers[i].answerText;

        Invoke("QuestionRunning", 5.94f);

        foreach (PlayerObject pl in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, "Get ready for the question...");

        DebugLog.Print($"QUESTION #{GameplayManager.nextQuestionIndex + 1}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
        DebugLog.Print(currentQuestion.questionText, DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
        for (int i = 0; i < currentQuestion.answers.Count; i++)
            DebugLog.Print(currentQuestion.answers[i].answerText, DebugLog.StyleOption.Italic, currentQuestion.answers[i].isCorrect ? DebugLog.ColorOption.Green : DebugLog.ColorOption.Red);
    }

    public virtual void QuestionRunning()
    {
        questionBox.SetBoxBorderColor(QABox.BoxColor.Blue);
        questionBox.ToggleDoors();
        foreach (QABox box in answerBoxes)
        {
            box.SetBoxBorderColor(QABox.BoxColor.Yellow);
            box.ToggleDoors();
        }


        foreach (PlayerObject pl in PlayerManager.Get.players)
        {
            string help = (pl.isEliminated || pl.helpUsed) ? "FALSE" : "TRUE";
            if (QuestionManager.IsFinalRound())
            {
                help = "FALSE";
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.DangerZoneQuestion,
                    $"<u><size=50%>#{GameplayManager.nextQuestionIndex + 1}/{QuestionManager.GetRoundQCount()}</size></u>\n{currentQuestion.questionText}|15|{string.Join("|", currentQuestion.answers.Select(x => x.answerText))}|{help}|0");
            }
            else
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.DangerZoneQuestion,
                        $"<u><size=50%>#{GameplayManager.nextQuestionIndex + 1}/{QuestionManager.GetRoundQCount()}</size></u>\n{currentQuestion.questionText}|15|{string.Join("|", currentQuestion.answers.Select(x => x.answerText))}|{help}|{nerfPoints}");
        }
        DebugLog.Print($"RESPONSES", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
        Invoke("OnQuestionEnded", 16f);
    }

    public virtual void OnQuestionEnded()
    {
        AudioManager.Get.Play(AudioManager.LoopClip.AmbientLoop, true);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.ResetPostQuestion;
        for (int i = 0; i < answerBoxes.Length; i++)
            answerBoxes[i].SetBoxBorderColor(currentQuestion.answers[i].isCorrect ? QABox.BoxColor.Green : QABox.BoxColor.Red);


        foreach(PlayerObject pl in PlayerManager.Get.players)
        {
            string pointsReadout = pl.pointsLastQ == 0 ? "" : $"\n({Extensions.PointOrPoints(pl.pointsLastQ)})";
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.SingleAndMultiResult, $"The correct answer was {currentQuestion.answers.FirstOrDefault(x => x.isCorrect).answerText}{pointsReadout}|{(pl.wasCorrect ? "CORRECT" : "INCORRECT")}");

            if (!pl.isEliminated)
            {
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.UpdateScore, Extensions.PointOrPoints(pl.points + pl.pointsLastQ));
                pl.strap.totalCorrectMesh.text = "+ " + pl.pointsLastQ.ToString();
                pl.cloneStrap.totalCorrectMesh.text = "+ " + pl.pointsLastQ.ToString();
               
                if (pl.multiplier)
                {
                    pl.strap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Multiplied);
                    pl.cloneStrap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Multiplied);
                }

                if (pl.nerfed)
                {
                    pl.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Nerfed);
                    pl.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Nerfed);
                    pl.strap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Nerfed);
                    pl.cloneStrap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Nerfed);
                }
                else if (pl.wasCorrect)
                {
                    pl.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                    pl.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                }
                else
                {
                    pl.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    pl.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    pl.strap.totalCorrectMesh.text = "❌";
                    pl.cloneStrap.totalCorrectMesh.text = "❌";
                }
            }
            else
            {
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.UpdateScore, $"{pl.totalCorrect} CORRECT");
                pl.strap.totalCorrectMesh.text = pl.totalCorrect.ToString();
                pl.cloneStrap.totalCorrectMesh.text = pl.totalCorrect.ToString();

                if (pl.wasCorrect)
                {
                    pl.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.BoneyardCorrect);
                    pl.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.BoneyardCorrect);
                }
                else
                {
                    pl.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.BoneyardIncorrect);
                    pl.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.BoneyardIncorrect);
                }
            }
        }

        DebugLog.Print($"TOP FIVE", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
        var x = PlayerManager.Get.players.Where(x => x.wasCorrect && !x.isEliminated).OrderBy(x => x.submissionTime).ToList();
        int upper = x.Count > 4 ? 5 : x.Count;

        for(int i = 0; i < upper; i++)
            DebugLog.Print($"{Extensions.AddOrdinal(i + 1)}) {x[i].playerName} ({x[i].submissionTime.ToString("0.00")}s, {x[i].pointsLastQ} pts {(x[i].multiplier ? "w/ multiplier" : "")})", DebugLog.StyleOption.Bold, x[i].multiplier ? DebugLog.ColorOption.Yellow : DebugLog.ColorOption.Green);
    }

    public virtual void ResetForNewQuestion()
    {
        questionBox.SetBoxBorderColor(QABox.BoxColor.White);
        questionBox.ToggleDoors();

        foreach (QABox bx in answerBoxes)
        {
            bx.SetBoxBorderColor(QABox.BoxColor.White);
            bx.ToggleDoors();
        }

        foreach (PlayerObject pl in PlayerManager.Get.players)
        {
            if (pl.isEliminated)
            {
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.UpdateScore, $"{pl.totalCorrect.ToString()} CORRECT");
                pl.strap.totalCorrectMesh.text = pl.totalCorrect.ToString();
                pl.cloneStrap.totalCorrectMesh.text = pl.totalCorrect.ToString();
            }
                
            else
            {
                pl.points += pl.pointsLastQ;
                pl.strap.totalCorrectMesh.text = pl.points.ToString();
                pl.cloneStrap.totalCorrectMesh.text = pl.points.ToString();
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.UpdateScore, Extensions.PointOrPoints(pl.points));
                if(pl.helpUsed)
                {
                    pl.strap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Unavailable);
                    pl.cloneStrap.SetButtonDisplay(GlobalLeaderboardStrap.ButtonDisplay.Unavailable);
                }
            }
        }

        LeaderboardManager.Get.ReorderBoard();
        LeaderboardManager.Get.SetMidRoundDangerZone();
    }

    public void InitialiseTieBreaker()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Tiebreaker);
        List<PlayerObject> orderedTiebreakPlayers = null;
        if(QuestionManager.IsFinalRound())
        {
            int topScore = PlayerManager.Get.players.Where(x => !x.isEliminated).OrderByDescending(x => x.points).FirstOrDefault().points;
            orderedTiebreakPlayers = PlayerManager.Get.players.Where(x => !x.isEliminated && x.points == topScore).ToList();
        }
            
        else
            orderedTiebreakPlayers = PlayerManager.Get.players.Where(x => x.isInDangerZone).OrderByDescending(x => x.points).ToList();

        foreach(PlayerObject po in orderedTiebreakPlayers)
        {
            if(po.points == orderedTiebreakPlayers[0].points)
            {
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.TiedInDZ);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.TiedInDZ);
                po.isInTiebreaker = true;
            }
            else
            {
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
            }
        }
        tiebreakerAnim.SetTrigger("toggle");
        uiBackingStrap.SetTrigger("toggle");
    }

    public void RunTieBreakerQuestion()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.QuestionLoop);
        availablePoints = 0;
        ResetPlayerVariables(false);
        currentQuestion = QuestionManager.GetTiebreakerQuestion();
        questionBox.SetBoxBorderColor(QABox.BoxColor.White);

        foreach (QABox bx in answerBoxes)
            bx.SetBoxBorderColor(QABox.BoxColor.White);

        GlobalTimeManager.Get.StagedResetClock();

        questionBox.boxMesh.text = currentQuestion.questionText;

        Invoke("TieBreakerRunning", 5.94f);

        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => x.isInTiebreaker))
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, "Get ready for the question...");
        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => !x.isInTiebreaker))
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, "Tiebreaker running...");

        DebugLog.Print($"TIEBREAKER QUESTION", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
        DebugLog.Print(currentQuestion.questionText, DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
        DebugLog.Print(currentQuestion.answers.FirstOrDefault().answerText, DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
    }

    public void TieBreakerRunning()
    {
        questionBox.SetBoxBorderColor(QABox.BoxColor.Blue);
        questionBox.ToggleDoors();

        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => x.isInTiebreaker))
        {
            pl.tiebreakerSubmission = -1;
            //This should actually be the same I think?
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.NumericalQuestion,
                    $"<u><size=50%>TIEBREAKER</size></u>\n{currentQuestion.questionText}|15");
        }
        DebugLog.Print($"RESPONSES", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
        Invoke("OnTiebreakerEnded", 16f);
    }

    public virtual void OnTiebreakerEnded()
    {
        AudioManager.Get.Play(AudioManager.LoopClip.AmbientLoop, true);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.ResolveTiebreaker;

        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => x.isInTiebreaker))
        {
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, $"TIME UP");

            pl.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.TiedInDZ);
            pl.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.TiedInDZ);

            if(pl.tiebreakerSubmission == -1)
            {
                pl.tiebreakerSubmission = int.MaxValue;
                pl.strap.totalCorrectMesh.text = "❌";
                pl.cloneStrap.totalCorrectMesh.text = "❌";
            }
            else
            {
                pl.strap.totalCorrectMesh.text = pl.tiebreakerSubmission.ToString();
                pl.cloneStrap.totalCorrectMesh.text = pl.tiebreakerSubmission.ToString();
            }
        }
    }

    public void ResolveTiebreaker()
    {
        questionBox.SetBoxBorderColor(QABox.BoxColor.Green);
        Invoke("InvokeQuestionDoors", 5f);
        questionBox.boxMesh.text = currentQuestion.answers.FirstOrDefault().answerText.ToString();
        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => x.isInTiebreaker))
        {
            int response = pl.tiebreakerSubmission;
            int expected = int.TryParse(currentQuestion.answers.FirstOrDefault().answerText, out int ex) ? ex : 0;
            pl.tiebreakerSubmission = Mathf.Abs(expected - response);
        }
        List<PlayerObject> orderedTB = PlayerManager.Get.players.Where(x => x.isInTiebreaker).OrderBy(x => x.tiebreakerSubmission).ThenBy(x => x.submissionTime).ToList();

        foreach (PlayerObject pl in orderedTB)
        {
            if (pl == orderedTB.FirstOrDefault())
                orderedTB.FirstOrDefault().isInTiebreaker = false;
            else
                pl.points--;

            pl.strap.totalCorrectMesh.text = pl.points.ToString();
            pl.cloneStrap.totalCorrectMesh.text = pl.points.ToString();
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, pl == orderedTB.FirstOrDefault() ? "YOU WERE CLOSEST" : "YOU WERE NOT CLOSEST");
        }

        LeaderboardManager.Get.ReorderBoard();
        LeaderboardManager.Get.SetMidRoundDangerZone();

        ResetPlayerVariables(false);
    }

    public void InvokeQuestionDoors()
    {
        questionBox.ToggleDoors();
        questionBox.SetBoxBorderColor(QABox.BoxColor.White);
    }

    public virtual void EndOfRoundLogic()
    {
        StartCoroutine(EliminationAnimation());
    }

    IEnumerator EliminationAnimation()
    {
        Instantiate(eliminationExplosion);
        yield return new WaitForSeconds(5f);
        AudioManager.Get.Play(AudioManager.OneShotClip.Elimination);
        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => x.isEliminated))
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, "END OF ROUND");

        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => x.isInDangerZone))
        {
            pl.isInDangerZone = false;
            pl.isEliminated = true;
            pl.floorAvatar.gameObject.SetActive(false);
            pl.strap.avatarRend.material = LeaderboardManager.Get.grayscaleMat;
            pl.cloneStrap.avatarRend.material = LeaderboardManager.Get.grayscaleMat;
            HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, "YOU HAVE BEEN ELIMINATED");
        }

        var x = PlayerManager.Get.players.Where(x => !x.isEliminated).OrderByDescending(x => x.points).ToList();
        int divisor = 2;
        int boost = x.Count / divisor;

        //Apply boosts if not final round
        if (!QuestionManager.IsFinalRound())
        {
            for (int i = 0; i < x.Count; i++)
            {
                if (i == 0)
                    x[i].starterBoost = boost;
                else
                {
                    if (x[i].points == x[i - 1].points)
                        x[i].starterBoost = boost;
                    else
                    {
                        divisor++;
                        if (divisor == 5)
                            break;

                        boost = x.Count / divisor;
                        x[i].starterBoost = boost;
                    }
                }
            }
        }

        //Apply opening scores if is final round
        else
            foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => !x.isEliminated))
                pl.starterBoost = pl.totalCorrect - pl.dzAppearances;

        ResetPlayerVariables(true);

        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => x.starterBoost != 0))
        {
            po.points = po.starterBoost;
            po.starterBoost = 0;
        }

        foreach(PlayerObject pl in PlayerManager.Get.players)
        {
            if(pl.isEliminated)
            {
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.UpdateScore, $"{pl.totalCorrect.ToString()} CORRECT");
                pl.strap.totalCorrectMesh.text = pl.totalCorrect.ToString();
                pl.cloneStrap.totalCorrectMesh.text = pl.totalCorrect.ToString();
            }
            else
            {
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, "YOU ARE THROUGH TO THE NEXT ROUND");
                HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.UpdateScore, Extensions.PointOrPoints(pl.points));
                pl.strap.totalCorrectMesh.text = pl.points.ToString();
                pl.cloneStrap.totalCorrectMesh.text = pl.points.ToString();
            }
        }

        LeaderboardManager.Get.ReorderBoard();
        LeaderboardManager.Get.SetStartRoundDangerZone();
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.LoadCategory;
    }

    public void GameOverLogic()
    {
        string winnerName = PlayerManager.Get.players.Where(x => !x.isEliminated).OrderByDescending(x => x.points).FirstOrDefault().playerName.ToUpperInvariant();
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.Fanfare);
        foreach (TextMeshProUGUI tx in winnerNameMeshes)
            tx.text = winnerName + "\nIS THE WINNER";
        winnerNameAnim.SetTrigger("winner");
        uiBackingStrap.SetTrigger("winner");

        foreach(PlayerObject po in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"GAME OVER\nYou earned {po.totalCorrect * GameplayPennys.Get.multiplyFactor} Pennys this game");
    }

    public virtual void ResetPlayerVariables(bool roundReset)
    {
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            po.ClearQuestionVariables();
            if (roundReset)
                po.ClearRoundVariables();
        }
    }
}
