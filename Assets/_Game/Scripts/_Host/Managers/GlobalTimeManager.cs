using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTimeManager : SingletonMonoBehaviour<GlobalTimeManager>
{
    private bool _questionClockRunning;
    public bool QuestionClockRunning
    {
        get
        {
            return _questionClockRunning;
        }
        set
        {
            if (_questionClockRunning != value)
            {
                if (value)
                    StartCoroutine(ClockAnimation());
                _questionClockRunning = value;
            }
        }
    }

    [ShowOnly] public float elapsedTime;
    public TimerBollard[] bollards;
    public Animator timerAnim;

    private void Update()
    {
        if (QuestionClockRunning)
            QuestionTimer();
        else
            elapsedTime = 0;
    }

    void QuestionTimer()
    {
        elapsedTime += (1f * Time.deltaTime);
    }

    public float GetRawTimestamp()
    {
        return elapsedTime;
    }

    public string GetFormattedTimestamp()
    {
        return elapsedTime.ToString("#0.00");
    }

    [Button]
    public void StartClock()
    {
        QuestionClockRunning = true;
    }

    [Button]
    public void ResetClock()
    {
        StartCoroutine(ResetClockAnim());
    }

    [Button]
    public void StagedResetClock()
    {
        StartCoroutine(StagedResetAnimation());
    }

    IEnumerator ClockAnimation()
    {
        for(int i = 0; i < 9; i++)
        {
            bollards[i].ToggleBollard();
            if (i != 8)
                bollards[bollards.Length - 1 - i].ToggleBollard();

            yield return new WaitForSeconds(1.5f);
        }
        yield return new WaitForSeconds(3.5f);
        QuestionClockRunning = false;
    }

    IEnumerator StagedResetAnimation()
    {
        timerAnim.SetTrigger("toggle");
        for (int i = 8; i >= 0; i -= 2)
        {
            bollards[i].ToggleBollard();
            if (i != 8)
            {
                bollards[i + 1].ToggleBollard();
                bollards[bollards.Length - 1 - i].ToggleBollard();
                bollards[bollards.Length - 2 - i].ToggleBollard();
            }
            yield return new WaitForSeconds(1f);
        }

        for(int i = 8; i >= 0; i --)
        {
            for (int j = 0; j < bollards.Length; j++)
                bollards[j].SetColor(true);

            bollards[i].SetColor(false);
            if (i != 8)
                bollards[bollards.Length - 1 - i].SetColor(false);

            yield return new WaitForSeconds(0.1176f);
        }

        /*for(int i = 0; i < bollards.Length; i++)
        {
            for (int j = 0; j < bollards.Length; j++)
                bollards[j].SetColor(true);

            bollards[i].SetColor(false);
            yield return new WaitForSeconds(0.0294f);
        }

        for(int i = bollards.Length - 1; i >= 0; i--)
        {
            for (int j = bollards.Length - 1; j >= 0; j--)
                bollards[j].SetColor(true);

            bollards[i].SetColor(false);
            yield return new WaitForSeconds(0.0294f);
        }*/
        bollards[0].SetColor(true);
        bollards[16].SetColor(true);
        StartClock();
    }

    IEnumerator ResetClockAnim()
    {
        for (int i = 8; i >= 0; i--)
        {
            bollards[i].ToggleBollard();
            if (i != 8)
                bollards[bollards.Length - 1 - i].ToggleBollard();

            yield return new WaitForSeconds(0.25f);
        }
    }
}
