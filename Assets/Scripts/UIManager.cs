using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

public class UIManager : MonoBehaviour
{
    [SerializeField] RectTransform WillpowerMask;
    [SerializeField] ParticleSystem CheckpointSystem;
    [SerializeField] RectTransform WillpowerMeter;
    [SerializeField] ParticleSystem WillpowerParticles;
    [SerializeField] Checkpoint checkpoint;
    [SerializeField] GameObject CPButton;

    [SerializeField] float transitionTime;

    private float maxHeight = 0;
    private float maxWillpower = 10;

    private IDisposable fillRoutine;

    ParticleSystem.MainModule main;

    private void Awake()
    {
        main = CheckpointSystem.main;
        maxHeight = WillpowerMask.sizeDelta.y;
        PlayerController.WillpowerSubscriber.Subscribe(w => {
            if (fillRoutine != null) fillRoutine.Dispose();
            fillRoutine = Observable.FromMicroCoroutine(() => FillWillpower(w > maxWillpower ? maxWillpower : w), false, FrameCountType.EndOfFrame).Subscribe();
            if (w >= maxWillpower && WillpowerParticles.isStopped)
            {
                CPButton.SetActive(true);
                WillpowerParticles.Play();
            }
            else if(WillpowerParticles.isPlaying)
            {
                CPButton.SetActive(false);
                WillpowerParticles.Stop();
            }
        });
        PlayerController.CheckpointSubscriber.Subscribe(c =>
        {
            if (c.x != float.NegativeInfinity)
            {
                checkpoint.transform.position = c;
                checkpoint.OnCheckpointMoved();
                main.startSpeed = -Vector3.Distance(CheckpointSystem.transform.position, checkpoint.transform.position) + 40;
                CheckpointSystem.transform.position = Camera.main.ScreenToWorldPoint(new(WillpowerMeter.position.x, WillpowerMeter.position.y, -Camera.main.transform.position.z));
                CheckpointSystem.Play();
            }
        });
    }

    private IEnumerator FillWillpower(float willpower)
    {
        float height1 = maxHeight * ((willpower - 1) / maxWillpower);
        float height2 = maxHeight * (willpower / maxWillpower);

        float elapsed = 0;

        while (elapsed < transitionTime)
        {
            WillpowerMask.sizeDelta = Vector2.Lerp(new(WillpowerMask.sizeDelta.x, height1), new(WillpowerMask.sizeDelta.x, height2), EaseInQuint(elapsed / transitionTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        WillpowerMask.sizeDelta = new(WillpowerMask.sizeDelta.x, height2);
    }

    private float EaseInQuint(float t)
    {
        return 1 - Mathf.Pow(1 - t, 5);
    }
}
