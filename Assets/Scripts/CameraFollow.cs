using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(0, target.position.y, -10), Time.deltaTime * 2);
    }
}
