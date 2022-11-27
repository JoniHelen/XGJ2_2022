using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    private Vector2 InitialPosition;
    [SerializeField] float FloatHeight;
    [SerializeField] float FloatSpeed;
    [SerializeField] ParticleSystem CollectSystem;

    private bool collected = false;

    public static event AudioHandler.AudioEventHandler OnPlaySound;

    private void Awake()
    {
        InitialPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player) && !collected)
        {
            collected = true;
            player.Willpower++;
            OnPlaySound?.Invoke("Blip_2");
            Instantiate(CollectSystem, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.Lerp(InitialPosition, InitialPosition + Vector2.up * FloatHeight, (Mathf.Sin(Time.time * FloatSpeed) + 1) / 2);
    }
}
