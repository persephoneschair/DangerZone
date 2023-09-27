using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DangerZoneManager : SingletonMonoBehaviour<DangerZoneManager>
{
    public GameObject floorAvatarObj;
    public Transform avatarInstanceTransform;
    public Animator floorExplosionAnim;

    public int GetRoundStartDZSize()
    {
        int totalActivePlayers = PlayerManager.Get.players.Count(x => !x.isEliminated);
        int eliminationTotal = totalActivePlayers - Operator.Get.playerFinalCount;
        int divisor = (QuestionManager.currentPack.rounds.Count - 1) - GameplayManager.Get.roundsPlayed;
        if (divisor == 0)
            return 0;
        int modulus = eliminationTotal % divisor;
        int baseReturnValue = eliminationTotal / divisor;

        return modulus == 0 ? baseReturnValue : modulus < GameplayManager.Get.roundsPlayed ? baseReturnValue + 1 : baseReturnValue;
    }


    public DZAvatarLerpControl GenerateNewFloorAvatar(PlayerObject pl)
    {
        GameObject gO = Instantiate(floorAvatarObj, avatarInstanceTransform);
        DZAvatarLerpControl dz = gO.GetComponent<DZAvatarLerpControl>();
        dz.Initialise(pl);
        return dz;
    }
}
