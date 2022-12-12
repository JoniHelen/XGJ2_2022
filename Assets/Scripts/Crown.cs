using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Crown : MonoBehaviour
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
            OnPlaySound?.Invoke("CP_2", reverb: true);
            Instantiate(CollectSystem, transform.position, Quaternion.identity);
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<ParticleSystem>().Stop();
            StartCoroutine(End());
        }
    }

    private IEnumerator End()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.Lerp(InitialPosition, InitialPosition + Vector2.up * FloatHeight, (Mathf.Sin(Time.time * FloatSpeed) + 1) / 2);
    }
}
