using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI willText;

    private void Awake()
    {
        PlayerController.WillpowerSubscriber.Subscribe(w => willText.text = $"Willpower: {w}");
    }
}
