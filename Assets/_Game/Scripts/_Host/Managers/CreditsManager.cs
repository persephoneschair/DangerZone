using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityStandardAssets.Effects;

public class CreditsManager : SingletonMonoBehaviour<CreditsManager>
{
    public GameObject studioExplosions;
    public Transform explosionTarget;
    public Transform fireworksTarget;
    public GameObject explosion;
    public GameObject fireworks;
    [TextArea(4, 8)] public string[] creditsOptions;
    public TextMeshProUGUI creditsMesh;
    public Animator creditsAnim;

    public GameObject endCard;

    public string[] authors;

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    [Button]
    public void RollCredits()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(CreditsSequence());
    }

    IEnumerator CreditsSequence()
    {
        creditsAnim.speed = 2f;
        Instantiate(explosion, explosionTarget);
        studioExplosions.SetActive(true);
        AudioManager.Get.Play(AudioManager.OneShotClip.MainTheme);
        yield return new WaitForSeconds(4f);
        for (int i = 0; i < creditsOptions.Length; i++)
        {
            Instantiate(explosion, explosionTarget);
            creditsMesh.text = creditsOptions[i].Replace("[AUTHOR1]", authors[0]).Replace("[AUTHOR2]", authors[1]).Replace("[AUTHOR3]", authors[2]).Replace("[AUTHOR4]", authors[3]).Trim();

            if (i == creditsOptions.Length - 3)
                Instantiate(fireworks, fireworksTarget);

            if (i == creditsOptions.Length - 1)
                creditsAnim.SetBool("end", true);

            creditsAnim.SetTrigger("toggle");
            yield return new WaitForSeconds(4f);
        }
        studioExplosions.SetActive(false);
        endCard.SetActive(true);
    }
}
