using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliminationControl : MonoBehaviour
{

    public GameObject explosion;
    public GameObject lightning;
    private Animator floorLight;

    void Start()
    {
        floorLight = DangerZoneManager.Get.floorExplosionAnim;
        StartCoroutine(TriggerElimination());
    }

    IEnumerator TriggerElimination()
    {
        floorLight.SetTrigger("toggle");
        yield return new WaitForSeconds(5f);
        explosion.SetActive(true);
        lightning.SetActive(false);
        yield return new WaitForSeconds(4f);
        Destroy(this.gameObject);
    }    
}
