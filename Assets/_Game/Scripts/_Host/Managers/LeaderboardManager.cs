using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardManager : SingletonMonoBehaviour<LeaderboardManager>
{
    public GlobalLeaderboardStrap[] straps;
    public GlobalLeaderboardStrap[] cloneStraps;
    public float reorderDuration = 2f;

    public List<GlobalLeaderboardStrap> originalStraps = new List<GlobalLeaderboardStrap>();

    public Material grayscaleMat;

    private void Start()
    {
        for(int i = 0; i < straps.Count(); i++)
        {
            straps[i].SetUpStrap(i + 1);
        }
        for (int i = 0; i < cloneStraps.Count(); i++)
            cloneStraps[i].SetUpStrap(i + 1);

        originalStraps = straps.ToList();
    }

    public void PlayerHasJoined(PlayerObject po)
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.LockIn);
        straps.FirstOrDefault(x => x.containedPlayer == null).PopulateStrap(po, false);
        cloneStraps.FirstOrDefault(x => x.containedPlayer == null).PopulateStrap(po, true);
        ReorderBoard();
        if (LobbyManager.Get.lateEntry/* && GameplayManager.Get.roundsPlayed == 0*/)
            SetStartRoundDangerZone();
    }

    [Button]
    public void ReorderBoard()
    {
        straps = straps.OrderByDescending(x => x.containedPlayer != null).ThenBy(x => x.containedPlayer?.isEliminated).ThenByDescending(x => x.containedPlayer?.points).ThenByDescending(x => x.containedPlayer?.totalCorrect).ThenBy(x => x.containedPlayer?.playerName).ToArray();
        cloneStraps = cloneStraps.OrderByDescending(x => x.containedPlayer != null).ThenBy(x => x.containedPlayer?.isEliminated).ThenByDescending(x => x.containedPlayer?.points).ThenByDescending(x => x.containedPlayer?.totalCorrect).ThenBy(x => x.containedPlayer?.playerName).ToArray();
        //Extensions.ShuffleParallel(straps, cloneStraps);
        for (int i = 0; i < straps.Length; i++)
        {
            straps[i].MoveStrap(originalStraps[i].startPos, i);
            cloneStraps[i].MoveStrap(originalStraps[i].startPos, i);
            straps[i].ordinalNumberMesh.text = (i + 1).ToString()/*Extensions.AddOrdinal(i + 1)*/;
            cloneStraps[i].ordinalNumberMesh.text = (i + 1).ToString()/*Extensions.AddOrdinal(i + 1)*/;
        }
    }

    public void SetStartRoundDangerZone()
    {
        foreach (GlobalLeaderboardStrap st in straps)
            st.ToggleDZLine();

        foreach (GlobalLeaderboardStrap st in cloneStraps)
            st.ToggleDZLine();

        int dzCount = DangerZoneManager.Get.GetRoundStartDZSize();

        List<PlayerObject> orderedPlayers = PlayerManager.Get.players.OrderBy(x => x.isEliminated).ThenByDescending(x => x.points).ThenByDescending(x => x.totalCorrect).ThenBy(x => x.playerName).ToList();

        //No DZ line because final round
        if (QuestionManager.IsFinalRound())
        {
            foreach (PlayerObject po in PlayerManager.Get.players)
            {
                if (po.isEliminated)
                {
                    po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.InBoneyard);
                    po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.InBoneyard);
                }
                else
                {
                    po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                    po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                }
            }
        }

        else
        {
            straps[PlayerManager.Get.players.Count(x => !x.isEliminated) - dzCount].ToggleDZLine(true);
            cloneStraps[PlayerManager.Get.players.Count(x => !x.isEliminated) - dzCount].ToggleDZLine(true);

            for (int i = 0; i < PlayerManager.Get.players.Count; i++)
            {
                //"In the DangerZone"
                if (i >= PlayerManager.Get.players.Count(x => !x.isEliminated) - dzCount /*|| (straps[i].containedPlayer != null && straps[i].containedPlayer.isInDangerZone)*/)
                {
                    //repaint but DON'T SET AS IN DZ - this comes post-Q
                    straps[i].SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    cloneStraps[i].SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                }

                //Safe
                else
                {
                    straps[i].SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                    cloneStraps[i].SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                }
            }

            //If we are in late entry mode and the number of players in the DZ is greater than the expected value
            if(LobbyManager.Get.lateEntry && PlayerManager.Get.players.Count(x => x.isInDangerZone) > DangerZoneManager.Get.GetRoundStartDZSize())
            {
                foreach(PlayerObject pl in PlayerManager.Get.players.Where(x => !x.isEliminated).ToList())
                {
                    //Kill all DZ lines of non-eliminated players
                    pl.strap.ToggleDZLine(false);
                    pl.cloneStrap.ToggleDZLine(false);
                    //Color tied in DZ players orange
                    if(pl.isInDangerZone || pl.points == 0)
                    {
                        pl.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.TiedInDZ);
                        pl.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.TiedInDZ);
                    }
                }
                PlayerManager.Get.players.OrderByDescending(x => x.points).ThenBy(x => x.playerName).FirstOrDefault(x => x.isInDangerZone).strap.ToggleDZLine(true);
                PlayerManager.Get.players.OrderByDescending(x => x.points).ThenBy(x => x.playerName).FirstOrDefault(x => x.isInDangerZone).cloneStrap.ToggleDZLine(true);
            }
        }

        SetBoneyard(orderedPlayers);
    }

    public void SetMidRoundDangerZone()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.LockIn);
        int standardDZ = DangerZoneManager.Get.GetRoundStartDZSize();

        //Switch off all DZ lines
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            po.strap.ToggleDZLine();
            po.cloneStrap.ToggleDZLine();
        }

        //Reorder the board
        ReorderBoard();

        //Get list of ordered players
        List<PlayerObject> orderedPlayers = PlayerManager.Get.players.OrderBy(x => x.isEliminated).ThenByDescending(x => x.points).ThenByDescending(x => x.totalCorrect).ThenBy(x => x.playerName).ToList();

        SetBoneyard(orderedPlayers);

        List<PlayerObject> livePlayers = orderedPlayers.Where(x => !x.isEliminated).Reverse().ToList();

        //DO NOT SET THE DANGERZONE IF WE ARE IN THE FINAL
        if (QuestionManager.IsFinalRound())
        {
            foreach(PlayerObject po in PlayerManager.Get.players.Where(x => !x.isEliminated))
            {
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
            }
            return;
        }

        bool uncheckFromDZ = false;
        for (int i = 0; i < livePlayers.Count; i++)
        {
            //Deffo in the DZ
            if (i < standardDZ)
                livePlayers[i].SetAsInDangerZone();

            else
            {
                //Non-standard DZ
                if (!uncheckFromDZ && livePlayers[i].points == livePlayers[i - 1].points)
                    livePlayers[i].SetAsInDangerZone();
                else
                {
                    uncheckFromDZ = true;
                    livePlayers[i].isInDangerZone = false;
                }
            }
        }

        //Repaint orange if there's a tie
        if (livePlayers.Count(x => x.isInDangerZone) != standardDZ)
        {
            foreach (PlayerObject po in livePlayers.Where(x => x.isInDangerZone))
            {
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.TiedInDZ);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.TiedInDZ);
            }
        }

        //Set top of DZ line
        livePlayers.LastOrDefault(x => x.isInDangerZone).strap.ToggleDZLine(true);
        livePlayers.LastOrDefault(x => x.isInDangerZone).cloneStrap.ToggleDZLine(true);

        //Final repaint and move floor avatars
        foreach (PlayerObject po in livePlayers)
        {
            if (!po.isInDangerZone)
            {
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                if (po.floorAvatar.isActive)
                    po.floorAvatar.isActive = false;
            }
            else
            {
                if (!po.floorAvatar.isActive)
                {
                    po.floorAvatar.Activate();
                    po.floorAvatar.isActive = true;
                }
            }
        }
    }

    public void SetBoneyard(List<PlayerObject> orderedPlayers)
    {
        //If anybody has been eliminated, we need a boneyard
        if (orderedPlayers.Any(x => x.isEliminated))
        {

            foreach (PlayerObject po in orderedPlayers.Where(x => x.isEliminated))
            {
                //Set top of boneyard line
                if (orderedPlayers.FirstOrDefault(x => x.isEliminated) == po)
                {
                    orderedPlayers.FirstOrDefault(x => x.isEliminated).strap.ToggleDZLine(true);
                    orderedPlayers.FirstOrDefault(x => x.isEliminated).cloneStrap.ToggleDZLine(true);
                }

                //Set emoji for joint top
                if(orderedPlayers.FirstOrDefault(x => x.isEliminated).totalCorrect == po.totalCorrect)
                {
                    po.strap.ordinalNumberMesh.text = "<size=75%>👑";
                    po.cloneStrap.ordinalNumberMesh.text = "<size=75%>👑";
                }
                else
                {
                    po.strap.ordinalNumberMesh.text = "<size=75%>💀";
                    po.cloneStrap.ordinalNumberMesh.text = "<size=75%>💀";
                }

                //Set colors and clear variables
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.InBoneyard);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.InBoneyard);
                po.isInDangerZone = false;
            }
        }
    }
}
