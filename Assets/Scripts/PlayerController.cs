using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool PointerDown = false;
    private Vector2 MousePosition = Vector2.zero;
    private Vector3 Direction = Vector3.zero;
    private Rigidbody2D rb;
    [SerializeField] Camera MainCamera;
    [SerializeField] float JumpStrength;
    [SerializeField] float CircleMaxSize;
    [SerializeField] float CircleMinSize;
    [SerializeField] GameObject[] IndicatorCircles = new GameObject[5];
    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDown = true;
        for (int i = 0; i < IndicatorCircles.Length; i++)
            IndicatorCircles[i].SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerDown = false;
        for (int i = 0; i < IndicatorCircles.Length; i++)
            IndicatorCircles[i].SetActive(false);
        rb.AddForce(Direction * JumpStrength, ForceMode2D.Impulse);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        for(int i = 0; i < IndicatorCircles.Length; i++)
            IndicatorCircles[i].transform.localScale = Vector3.one * (CircleMinSize + (CircleMaxSize - CircleMinSize) * (i / (float)IndicatorCircles.Length));
    }

    private void Update()
    {
        if (PointerDown)
        {
            MousePosition = MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Direction = transform.position - new Vector3(MousePosition.x, MousePosition.y);
            for (int i = 0; i < IndicatorCircles.Length; i++)
                IndicatorCircles[i].transform.position = Vector3.Lerp(transform.position, -Direction, (i + 1) / IndicatorCircles.Length);
        }
    }
}
