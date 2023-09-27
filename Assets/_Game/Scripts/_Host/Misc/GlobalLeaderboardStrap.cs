using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TwitchLib.Client.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLeaderboardStrap : MonoBehaviour
{
    public enum StrapColor
    { 
        TiedInDZ,
        InBoneyard,
        Correct,
        Incorrect,
        LockedIn,
        BoneyardCorrect,
        BoneyardIncorrect,
        Nerfed
    };

    public enum ButtonDisplay
    {
        Available,
        Nerfed,
        Multiplied,
        Unavailable
    }

    public Vector3 startPos;
    public PlayerObject containedPlayer;

    public TextMeshProUGUI ordinalNumberMesh;
    public TextMeshProUGUI playerNameMesh;
    public TextMeshProUGUI totalCorrectMesh;
    public RawImage avatarRend;

    public Image borderRend;
    public Image backgroundRend;

    public Color[] borderCols;
    public Color[] backgroundCols;

    private Vector3 targetPosition;
    private float elapsedTime = 0;

    public Image bonusButton;
    public TextMeshProUGUI bonusText;
    public Color[] buttonColors;
    private string[] buttonText = new string[4] { "AVAILABLE", "NERFED", "<color=black><b><size=175%>x2</size></b></color>", "<color=red>SPENT</color>" };


    public RawImage dzLine;
    public bool isBoneyardStrap;

    public void SetUpStrap(int ordinal)
    {
        startPos = GetComponent<RectTransform>().localPosition;
        targetPosition = startPos;
        ordinalNumberMesh.text = ordinal.ToString();
        playerNameMesh.text = "";
        totalCorrectMesh.text = "";
        gameObject.SetActive(false);
    }

    public void PopulateStrap(PlayerObject pl, bool isClone)
    {
        //Flicker to life?
        gameObject.SetActive(true);
        containedPlayer = pl;
        playerNameMesh.text = pl.playerName;
        avatarRend.texture = pl.profileImage;
        totalCorrectMesh.text = pl.points.ToString();
        if (!isClone)
            pl.strap = this;
        else
            pl.cloneStrap = this;

        if(pl.isEliminated)
        {
            ordinalNumberMesh.text = "💀";
            SetStrapColor(StrapColor.InBoneyard);
            SetButtonDisplay(ButtonDisplay.Unavailable);
        }
        else
        {
            SetStrapColor(StrapColor.Correct);
            SetButtonDisplay(ButtonDisplay.Available);
        }
    }

    public void ToggleDZLine(bool active = false)
    {
        dzLine.enabled = active;
    }

    public void SetStrapColor(StrapColor col)
    {
        backgroundRend.color = backgroundCols[(int)col];
        borderRend.color = borderCols[(int)col];
    }

    public void SetButtonDisplay(ButtonDisplay disp)
    {
        bonusButton.color = buttonColors[(int)disp];
        bonusText.text = buttonText[(int)disp];
    }

    public void MoveStrap(Vector3 targetPos, int i)
    {
        //playerNameMesh.text = (i + 1).ToString();
        targetPosition = targetPos;
        elapsedTime = 0;
    }

    public void Update()
    {
        LerpStraps();
    }

    private void LerpStraps()
    {
        elapsedTime += Time.deltaTime * 1f;
        float percentageComplete = elapsedTime / LeaderboardManager.Get.reorderDuration;
        this.gameObject.transform.localPosition = Vector3.Lerp(this.gameObject.transform.localPosition, targetPosition, Mathf.SmoothStep(0, 1, percentageComplete));
    }
}
