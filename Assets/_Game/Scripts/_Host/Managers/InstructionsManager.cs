using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructionsManager : SingletonMonoBehaviour<InstructionsManager>
{
    public TextMeshProUGUI instructionsMesh;
    public Animator instructionsAnim;
    private readonly string instructions =
        "<gradient=\"DZGrad\"><size=200%><font=DZLogo>ROUND [#]</gradient></font></size>\n" +
        "<size=300%>[ROUNDNAME]</size>\n\n" +
        "" +
        "[Q#] questions this round\n" +
        "Multiplying will <color=yellow>double your points</color> for the question\n" +
        "Nerfing in this round will earn you <color=yellow>[###] points</color>\n\n" +
        "" +
        "<color=yellow><size=75%>YOU MAY USE EITHER HELP ONCE THIS ROUND BUT NOT BOTH</size></color>";

    private readonly string finalInstructions =
        "<gradient=\"DZGrad\"><size=200%><font=DZLogo>FINAL ROUND</gradient></font></size>\n" +
        "<size=300%>[ROUNDNAME]</size>\n\n" +
        "" +
        "[Q#] questions this round\n" +
        "Multipliers and nerfs are inactive\n\n" +
        "" +
        "Live players start with a score equal to their total correct answers minus their total DangerZone appearances";

    [Button]
    public void OnShowInstructions()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.Tiebreaker);
        instructionsAnim.SetTrigger("toggle");
        if (QuestionManager.IsFinalRound())
            instructionsMesh.text = finalInstructions.Replace("[ROUNDNAME]", QuestionManager.currentPack.rounds[GameplayManager.Get.roundsPlayed].category.ToUpperInvariant())
                .Replace("[Q#]", Operator.Get.hideRoundCount ? "Unknown" : Extensions.ForceFirstCharToUpper(Extensions.NumberToWords(QuestionManager.GetRoundQCount())));
        else
            instructionsMesh.text = instructions.Replace("[#]", (GameplayManager.Get.roundsPlayed + 1).ToString())
                .Replace("[ROUNDNAME]", QuestionManager.currentPack.rounds[GameplayManager.Get.roundsPlayed].category.ToUpperInvariant())
                .Replace("[###]", Extensions.NumberToWords(GameplayManager.Get.mainRound.nerfPoints))
                .Replace("[Q#]", Operator.Get.hideRoundCount ? "Unknown" : Extensions.ForceFirstCharToUpper(Extensions.NumberToWords(QuestionManager.GetRoundQCount())));
    }

    [Button]
    public void OnHideInstructions()
    {
        instructionsAnim.SetTrigger("toggle");
    }
}
