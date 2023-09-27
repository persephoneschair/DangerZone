using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerBollard : MonoBehaviour
{
    public Animator anim;
    public Material baseMaterial;
    public Material blackMat;
    public Renderer rend;

    private void Start()
    {
        baseMaterial = rend.material;
    }

    public void ToggleBollard()
    {
        anim.SetTrigger("toggle");
    }

    public void SetColor(bool baseCol)
    {
        rend.material = baseCol ? baseMaterial : blackMat;
    }
}
