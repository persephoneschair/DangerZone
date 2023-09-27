using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : SingletonMonoBehaviour<LobbyManager>
{
    public bool lateEntry;

    public TextMeshProUGUI welcomeMessageMesh;
    public Animator lobbyCodeAnim;
    private const string welcomeMessage = "Welcome to\n<font=DZLogo><gradient=\"DZGrad\"><size=200%>DANGERZONE</size></font></gradient>\n" +
      "" +
    "Playing on a mobile device? Scan the QR code!\n\n\n\n\n\n\n" +
    "" +
    "Desktop or laptop? Please visit:\n<color=yellow>https://persephoneschair.itch.io/gamenight</color>\n" +
    "<size=300%><color=#F8A3A3>[ABCD]</color>";

    

    private const string permaMessage = "To join the game, please visit <color=yellow>https://persephoneschair.itch.io/gamenight</color> and join with the room code <color=#F8A3A3>[ABCD]</color>";

    public Animator permaCodeAnim;
    public TextMeshProUGUI permaCodeMesh;

    [Button]
    public void OnOpenLobby()
    {
        lobbyCodeAnim.SetTrigger("toggle");
        string spacedCode = string.Join(" ", HostManager.Get.host.RoomCode.ToCharArray());
        welcomeMessageMesh.text = welcomeMessage.Replace("[ABCD]", spacedCode.ToUpperInvariant());
    }

    [Button]
    public void OnLockLobby()
    {
        if(PlayerManager.Get.players.Count < ((QuestionManager.currentPack.rounds.Count - 1) + Operator.Get.playerFinalCount))
        {
            DebugLog.Print("THERE ARE NOT ENOUGH PLAYERS FOR THE CURRENT QUESTION PACK AND GAME SETTINGS", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
            return;
        }
        else
        {
            string spacedCode = string.Join(" ", HostManager.Get.host.RoomCode.ToCharArray());
            lateEntry = true;
            lobbyCodeAnim.SetTrigger("toggle");
            permaCodeMesh.text = permaMessage.Replace("[ABCD]", spacedCode.ToUpperInvariant());
            Invoke("TogglePermaCode", 1f);
            GameplayManager.Get.currentStage++;
            AudioManager.Get.Play(AudioManager.OneShotClip.Elimination);
            LeaderboardManager.Get.SetStartRoundDangerZone();
        }
    }

    public void TogglePermaCode()
    {
        permaCodeAnim.SetTrigger("toggle");
    }
}
