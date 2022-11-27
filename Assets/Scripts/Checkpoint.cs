using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Checkpoint : MonoBehaviour
{
    private SpriteRenderer rend;
    private ParticleSystem PopParticles;
    private WaitForSeconds PopWait = new(1.5f);
    // Start is called before the first frame update
    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        PopParticles = GetComponent<ParticleSystem>();
    }

    public void OnCheckpointMoved()
    {
        rend.enabled = false;
        StartCoroutine(PopIn());
    }

    private IEnumerator PopIn()
    {
        yield return PopWait;
        rend.enabled = true;
        PopParticles.Play();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
