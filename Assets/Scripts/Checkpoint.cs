using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Checkpoint : MonoBehaviour
{
    public static event AudioHandler.AudioEventHandler OnPlaySound;

    private SpriteRenderer rend;
    private ParticleSystem PopParticles;
    private WaitForSeconds PopWait = new(1.5f);
    // Start is called before the first frame update
    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        PopParticles = GetComponent<ParticleSystem>();
        PlayerController.OnReturn += () => PopParticles.Play();
    }

    public void OnCheckpointMoved()
    {
        rend.enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        StartCoroutine(PopIn());
    }

    private IEnumerator PopIn()
    {
        yield return PopWait;
        rend.enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
        OnPlaySound?.Invoke("CP_2", reverb: true);
        PopParticles.Play();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
