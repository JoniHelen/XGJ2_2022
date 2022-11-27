using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] RectTransform WillpowerMask;
    [SerializeField] ParticleSystem CheckpointSystem;
    [SerializeField] RectTransform WillpowerMeter;
    [SerializeField] ParticleSystem WillpowerParticles;
    [SerializeField] Checkpoint checkpoint;
    [SerializeField] GameObject CPButton;
    [SerializeField] Transform player;

    [SerializeField] float transitionTime;

    private float maxHeight = 0;
    private float maxWillpower = 10;
    private float currentWillpower = 0;

    private bool OvelappingUI = false;

    private IDisposable fillRoutine;

    private Dictionary<Image, float> Images = new();

    ParticleSystem.MainModule main;

    Vector3[] worldCorners = new Vector3[4];

    CompositeDisposable disposables = new CompositeDisposable();

    private void Awake()
    {
        Image[] imgs = WillpowerMeter.GetComponentsInChildren<Image>();
        for (int i = 0; i < imgs.Length; i++)
            Images.Add(imgs[i], imgs[i].color.a);

        main = CheckpointSystem.main;
        maxHeight = WillpowerMask.sizeDelta.y;
        PlayerController.WillpowerSubscriber.Subscribe(w => {
            if (!(w == maxWillpower && currentWillpower == maxWillpower))
            {
                if (fillRoutine != null)
                    fillRoutine.Dispose();
                fillRoutine = Observable.FromMicroCoroutine(() => FillWillpower(w > maxWillpower ? maxWillpower : w), false, FrameCountType.EndOfFrame).Subscribe();
            }

            currentWillpower = w;

            if (w == maxWillpower && WillpowerParticles.isStopped)
            {
                CPButton.SetActive(true);
                WillpowerParticles.Play();
            }
            else if(WillpowerParticles.isPlaying)
            {
                CPButton.SetActive(false);
                WillpowerParticles.Stop();
            }
        }).AddTo(disposables);
        PlayerController.CheckpointSubscriber.Subscribe(c =>
        {
            if (c != Vector2.zero)
            {
                checkpoint.transform.position = c;
                checkpoint.OnCheckpointMoved();
                main.startSpeed = -Vector3.Distance(CheckpointSystem.transform.position, checkpoint.transform.position) + 40;
                CheckpointSystem.transform.position = Camera.main.ScreenToWorldPoint(new(WillpowerMeter.position.x, WillpowerMeter.position.y, -Camera.main.transform.position.z));
                CheckpointSystem.Play();
            }
        }).AddTo(disposables);
        player.transform.ObserveEveryValueChanged(t => t.position).Subscribe(p =>
        {
            WillpowerMeter.GetWorldCorners(worldCorners);
            if (PointTouchesRect(worldCorners, Camera.main.WorldToScreenPoint(player.position)) && !OvelappingUI)
            {
                OvelappingUI = true;
                foreach(var kvp in Images)
                    MainThreadDispatcher.StartEndOfFrameMicroCoroutine(FadeImage(kvp.Key, kvp.Value, 0.3f));
            }
            else if (!PointTouchesRect(worldCorners, Camera.main.WorldToScreenPoint(player.position)) && OvelappingUI)
            {
                OvelappingUI = false;
                foreach (var kvp in Images)
                    MainThreadDispatcher.StartEndOfFrameMicroCoroutine(FadeImage(kvp.Key, 0.3f, kvp.Value));
            }
        }).AddTo(disposables);
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    private bool PointTouchesRect(Vector3[] corners, Vector3 point)
    {
        return point.x >= corners[0].x && point.y >= corners[0].y
                && point.x >= corners[1].x && point.y <= corners[1].y
                && point.x <= corners[2].x && point.y <= corners[2].y
                && point.x <= corners[3].x && point.y >= corners[3].y;
    }

    private IEnumerator FadeImage(Image img, float alphaFrom, float alphaTo)
    {
        float elapsed = 0;

        while(elapsed < 0.25f)
        {
            img.color = Color.Lerp(new(1, 1, 1, alphaFrom), new(1, 1, 1, alphaTo), elapsed / 0.25f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        img.color = new(1, 1, 1, alphaTo);
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
