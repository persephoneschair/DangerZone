using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QABox : MonoBehaviour
{
    public enum BoxColor { White, Yellow, Blue, Green, Red };

    public Animator doorAnim;
    public TextMeshPro boxMesh;
    public Renderer borderRend;
    public Material[] borderMats;


    public void SetBoxBorderColor(BoxColor color)
    {
        borderRend.material = borderMats[(int)color];
    }

    [Button]
    public void ToggleDoors()
    {
        doorAnim.SetTrigger("toggle");
    }
}
