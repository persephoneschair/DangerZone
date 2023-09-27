using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class DZAvatarLerpControl : MonoBehaviour
{
    public bool isActive;
    private Renderer avatarRend;

    float elapsedTime;
    Vector3 originalPos;
    Vector3 originalRot;
    Vector3 startPos;
    Vector3 endPos;
    Vector3 startRot;
    Vector3 endRot;
    float duration;
    bool isMoving;

    float[] widthLowerBounds = new float[3] { -0.25f, -1f, -1.1f };
    float[] widthUpperBounds = new float[3] { 0.5f, 0.75f, 1f };

    public void Initialise(PlayerObject pl)
    {
        avatarRend = this.GetComponentsInChildren<Renderer>()[1];
        avatarRend.material.mainTexture = pl.profileImage;
        this.gameObject.name = pl.playerName;
        this.transform.localPosition = new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 0.02f + (0.001f * PlayerManager.Get.players.Count), -5);
        this.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(-180f, 180f), 0);
        originalPos = this.gameObject.transform.localPosition;
        originalRot = this.gameObject.transform.localEulerAngles;
    }

    [Button]
    public void Activate()
    {
        GetNewPosition(true, false);
    }

    public void GetNewPosition(bool launch, bool end)
    {
        float newX = UnityEngine.Random.Range(-5f, 5f);
        float newZ = Mathf.Abs(newX) > 3 ? UnityEngine.Random.Range(widthLowerBounds[0], widthUpperBounds[0]) :
            Mathf.Abs(newX) > 1 ? UnityEngine.Random.Range(widthLowerBounds[1], widthUpperBounds[1]) :
            UnityEngine.Random.Range(widthLowerBounds[2], widthUpperBounds[2]);

        startPos = this.transform.localPosition;
        startRot = this.transform.localEulerAngles;

        if (end)
        {
            endPos = originalPos;
            endRot = originalRot;
        }
        else
        {
            endPos = new Vector3(newX, originalPos.y, newZ);
            endRot = new Vector3(0, UnityEngine.Random.Range(-180, 180), 0);
        }

        if(launch || end)
            duration = UnityEngine.Random.Range(1.0f, 2.5f);
        else
            duration = UnityEngine.Random.Range(5.0f, 6.0f);

        elapsedTime = 0;
        isMoving = true;
        if(!end)
            StartCoroutine(RePick());
        PerformLerp();
    }

    private void Update()
    {
        if (isMoving)
            PerformLerp();
    }

    private void PerformLerp()
    {
        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / duration;

        this.gameObject.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, percentageComplete));
        this.gameObject.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, Mathf.SmoothStep(0, 1, percentageComplete));
    }

    IEnumerator RePick()
    {
        yield return new WaitForSeconds(duration - 0.1f);
        isMoving = false;
        if (isActive)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
            GetNewPosition(false, false);
        }
            
        else
            GetNewPosition(false, true);
    }
}
