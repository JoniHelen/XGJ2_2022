using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    private Vector2 InitialPosition;
    [SerializeField] float FloatHeight;
    [SerializeField] float FloatSpeed;

    private bool collected = false;
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
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.Lerp(InitialPosition, InitialPosition + Vector2.up * FloatHeight, (Mathf.Sin(Time.time * FloatSpeed) + 1) / 2);
    }
}
