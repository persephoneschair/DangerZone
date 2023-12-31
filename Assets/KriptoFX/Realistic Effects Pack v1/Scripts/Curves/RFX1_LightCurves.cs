using UnityEngine;
using System.Collections;

public class RFX1_LightCurves : MonoBehaviour
{
    public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public bool IsLoop;

    [HideInInspector] public bool canUpdate;
    private float startTime;
    private Light lightSource;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        lightSource.intensity = LightCurve.Evaluate(0) * GraphIntensityMultiplier;
    }

    private void OnEnable()
    {
        startTime = Time.time;
        canUpdate = true;
        if(lightSource!=null) lightSource.intensity = LightCurve.Evaluate(0) * GraphIntensityMultiplier;

        StartCoroutine(End());
    }

    IEnumerator End()
    {
        yield return new WaitForSeconds(5f);
        canUpdate = false;
    }

    private void Update()
    {
        var time = Time.time - startTime;
        if (canUpdate) {
            var eval = LightCurve.Evaluate(time / GraphTimeMultiplier) * GraphIntensityMultiplier;
            lightSource.intensity = eval;
        }
        if (time >= GraphTimeMultiplier) {
            if (IsLoop) startTime = Time.time;
            else canUpdate = false;
        }
    }
}