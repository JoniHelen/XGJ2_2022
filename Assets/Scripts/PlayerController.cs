using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UniRx;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool PointerDown = false;
    private bool Grounded = false;
    private bool NoJump = false;
    private static readonly ReactiveProperty<float> willpower = new(0);
    private Vector2 MousePosition = Vector2.zero;
    private Vector3 Direction = Vector3.zero;
    private Transform[] IndicatorCircles = new Transform[5];
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer rend;
    private CapsuleCollider2D Capsule;

    [SerializeField] Transform CirclePrefab;
    [SerializeField] LayerMask GroundMask;
    [SerializeField] Camera MainCamera;
    [SerializeField] float JumpStrength;
    [SerializeField] float TurnThreshold;
    [SerializeField] float CircleMaxSize;
    [SerializeField] float CircleMinSize;

    private Vector3 Position { get => transform.position + new Vector3(Capsule.offset.x, Capsule.offset.y); }

    public float Willpower { get => willpower.Value; set => willpower.Value = value; }
    public static ReactiveProperty<float> WillpowerSubscriber { get => willpower; }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDown = true;
        if (Grounded)
        {
            animator.SetBool("Crouching", true);
            for (int i = 0; i < IndicatorCircles.Length; i++)
                IndicatorCircles[i].gameObject.SetActive(true);
        }
        else NoJump = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerDown = false;
        animator.SetBool("Crouching", false);
        for (int i = 0; i < IndicatorCircles.Length; i++)
            IndicatorCircles[i].gameObject.SetActive(false);
        if (Grounded && !NoJump)
            rb.AddForce(Direction * JumpStrength, ForceMode2D.Impulse);
        else NoJump = false;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.ObserveEveryValueChanged(x => x.velocity.y).Subscribe(vel => {
            if (vel > 0.1f)
                animator.SetBool("Rising", true);
            else if (vel < -0.1f)
                animator.SetBool("Falling", true);
            else {
                animator.SetBool("Rising", false);
                animator.SetBool("Falling", false);
            }
        });
        rend = GetComponent<SpriteRenderer>();
        Capsule = GetComponent<CapsuleCollider2D>();
        for (int i = 0; i < IndicatorCircles.Length; i++) {
            IndicatorCircles[i] = Instantiate(CirclePrefab);
            IndicatorCircles[i].localScale = Vector3.one * (CircleMinSize + (CircleMaxSize - CircleMinSize) * (i / (float)IndicatorCircles.Length));
        }
    }

    private void Update()
    {
        if (PointerDown && Grounded)
        {
            MousePosition = MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Direction = Position - new Vector3(MousePosition.x, MousePosition.y);
            for (int i = 0; i < IndicatorCircles.Length; i++)
                IndicatorCircles[i].position = Vector3.Lerp(Position, Position - Direction, (i + 1f) / IndicatorCircles.Length);

            if (MousePosition.x > transform.position.x + TurnThreshold)
                rend.flipX = true;
            else if(MousePosition.x < transform.position.x - TurnThreshold)
                rend.flipX = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
            Willpower++;
        if (collision.gameObject.layer == 6)
            Grounded = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6)
            Grounded = false;
    }
}
