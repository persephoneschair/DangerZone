using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitlesManager : SingletonMonoBehaviour<TitlesManager>
{
    public Transform explosionTarget;
    public Transform fireworksTarget;
    public GameObject explosion;
    public GameObject smoke;
    public GameObject fireworks;
    [TextArea(2, 3)] public string[] titlesOptions;
    public TextMeshProUGUI titlesMesh;
    public Animator titlesAnim;

    [Button]
    public void RunTitleSequence()
    {
        if (Operator.Get.skipOpeningTitles)
            EndOfTitleSequence();
        else
        {
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DoNothing;
            StartCoroutine(TitleSequence());
        }           
    }

    IEnumerator TitleSequence()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.MainTheme);
        yield return new WaitForSeconds(4f);
        for(int i = 0; i < titlesOptions.Length; i++)
        {
            Instantiate(explosion, explosionTarget);
            titlesMesh.text = titlesOptions[i].Replace("[AUTHOR]", QuestionManager.currentPack.author.ToUpperInvariant());

            if (i == titlesOptions.Length - 2)
                Instantiate(fireworks, fireworksTarget);

            if (i == titlesOptions.Length - 1)
                titlesAnim.SetBool("end", true);

            titlesAnim.SetTrigger("toggle");
            yield return new WaitForSeconds(4f);

            if (i == titlesOptions.Length - 1)
                Instantiate(explosion, explosionTarget);

            yield return new WaitForSeconds(4f);
        }
        EndOfTitleSequence();
    }

    void EndOfTitleSequence()
    {
        Instantiate(explosion, explosionTarget);
        smoke.SetActive(false);
        AudioManager.Get.Play(AudioManager.OneShotClip.Elimination);
        AudioManager.Get.Play(AudioManager.LoopClip.AmbientLoop);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.OpenLobby;
        GameplayManager.Get.ProgressGameplay();
        this.gameObject.SetActive(false);
    }
}
