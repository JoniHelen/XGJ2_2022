using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI willText;
    [SerializeField] RectTransform WillpowerMask;
    [SerializeField] ParticleSystem CheckpointSystem;
    [SerializeField] RectTransform WillpowerMeter;
    [SerializeField] Transform Checkpoint;

    private float maxHeight = 0;
    private float maxWillpower = 10;

    ParticleSystem.MainModule main;

    private void Awake()
    {
        main = CheckpointSystem.main;
        maxHeight = WillpowerMask.sizeDelta.y;
        PlayerController.WillpowerSubscriber.Subscribe(w => {
            willText.text = $"Willpower: {w}";
            WillpowerMask.sizeDelta = new(WillpowerMask.sizeDelta.x, maxHeight * (w > maxWillpower ? maxWillpower : w / maxWillpower));
        });
        PlayerController.CheckpointSubscriber.Subscribe(c =>
        {
            if (c.x != float.NegativeInfinity)
            {
                Checkpoint.position = c;
                main.startSpeed = -Vector3.Distance(CheckpointSystem.transform.position, Checkpoint.position) + 40;
                CheckpointSystem.transform.position = Camera.main.ScreenToWorldPoint(new(WillpowerMeter.position.x, WillpowerMeter.position.y, -Camera.main.transform.position.z));
                CheckpointSystem.Play();
            }
        });
    }
}
