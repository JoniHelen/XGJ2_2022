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
    private bool NoJump = false;
    private bool PointerDown = false;

    public static event AudioHandler.AudioEventHandler OnPlaySound;

    private readonly ReactiveProperty<bool> Splatted = new(false);
    private readonly ReactiveProperty<bool> Rising = new(false);
    private readonly ReactiveProperty<bool> Falling = new(false);
    private readonly ReactiveProperty<bool> Grounded = new(false);
    private static readonly ReactiveProperty<float> willpower = new(0);
    private static readonly ReactiveProperty<Vector2> CurrentCheckpointPosition = new(Vector2.negativeInfinity);

    private Vector2 MousePosition = Vector2.zero;
    private Vector3 Direction = Vector3.zero;

    private Transform[] IndicatorCircles = new Transform[5];
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer rend;
    private CapsuleCollider2D Capsule;

    private WaitForSeconds WakeupTime = new(1f);

    [SerializeField] LayerMask GroundMask;
    [SerializeField] Transform CirclePrefab;
    [SerializeField] float JumpStrength;
    [SerializeField] float TurnThreshold;
    [SerializeField] float CircleMaxSize;
    [SerializeField] float CircleMinSize;

    private Vector3 Position { get => transform.position + new Vector3(Capsule.offset.x, Capsule.offset.y); }

    private Vector2 CurrentCheckpoint { get => CurrentCheckpointPosition.Value; set => CurrentCheckpointPosition.Value = value; }
    public static ReactiveProperty<Vector2> CheckpointSubscriber { get => CurrentCheckpointPosition; }
    public float Willpower { get => willpower.Value; set => willpower.Value = value > 10 ? 10 : value; }
    public static ReactiveProperty<float> WillpowerSubscriber { get => willpower; }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDown = true;
        if (Grounded.Value && !Splatted.Value)
        {
            animator.SetBool("Crouching", true);
            OnPlaySound?.Invoke("Blip_1");
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
        if (Grounded.Value && !NoJump)
        {
            rb.AddForce(new Vector3(Direction.x * JumpStrength, Direction.y * JumpStrength * 1.5f), ForceMode2D.Impulse);
            OnPlaySound?.Invoke("Jump_2", Direction.magnitude / 10);
        }
        else NoJump = false;
    }

    public void SetCheckpoint()
    {
        if (Grounded.Value && Willpower >= 10)
        {
            Willpower -= 10;
            CurrentCheckpoint = transform.position;
            OnPlaySound?.Invoke("CP_1", reverb: true);
        }
    }
    public void GoToCheckpoint() 
    { 
        if (CurrentCheckpoint.x != float.NegativeInfinity)
            transform.position = CurrentCheckpoint;
    }

    private IEnumerator WakeUp()
    {
        yield return WakeupTime;
        Splatted.Value = false;
        animator.SetBool("Get Up", true);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.ObserveEveryValueChanged(x => x.velocity.y).Subscribe(vel => {
            if (vel > 0.1f && !Grounded.Value)
            {
                Rising.Value = true;
                Falling.Value = false;
            }
            else if (vel < -0.1f && !Grounded.Value)
            {
                Falling.Value = true;
                Rising.Value = false;
            }
            else {
                Falling.Value = Rising.Value = false;
            }
        });
        rend = GetComponent<SpriteRenderer>();
        Capsule = GetComponent<CapsuleCollider2D>();
        for (int i = 0; i < IndicatorCircles.Length; i++) {
            IndicatorCircles[i] = Instantiate(CirclePrefab);
            IndicatorCircles[i].localScale = Vector3.one * (CircleMinSize + (CircleMaxSize - CircleMinSize) * (i / (float)IndicatorCircles.Length));
        }
        Grounded.Subscribe(g => { animator.SetBool("Grounded", g);
            if (Splatted.Value)
                StartCoroutine(WakeUp());
        });
        Splatted.Subscribe(s => animator.SetBool("Splatted", s));
        Rising.Subscribe(r => animator.SetBool("Rising", r));
        Falling.Subscribe(f => animator.SetBool("Falling", f));
    }

    private void Update()
    {
        if (PointerDown && Grounded.Value && !Splatted.Value)
        {
            MousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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
        if (collision.gameObject.layer == 6)
        {
            Grounded.Value = true;
            if (Splatted.Value)
            {
                OnPlaySound?.Invoke("Hit_1");
            }
            else
                OnPlaySound?.Invoke("Hit_4");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6)
            Grounded.Value = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6 && !Grounded.Value)
        {
            Vector3 point = transform.InverseTransformPoint(collision.GetContact(0).point) - new Vector3(Capsule.offset.x, Capsule.offset.y);
            float angle = 180f / Mathf.PI * Mathf.Atan2(Vector3.Dot(Vector3.Cross(Vector3.right, point), Vector3.forward), Vector3.Dot(Vector3.right, point));
            if (angle > -45f && angle < 45f)
            {
                // Collision from right
                animator.SetTrigger("Side Splat");
                Splatted.Value = true;
                OnPlaySound?.Invoke("Hit_3");
                animator.SetBool("Get Up", false);
            }
            else if (angle > 45f && angle < 135f)
            {
                // Collision from top
                animator.SetTrigger("Top Splat");
                Splatted.Value = true;
                OnPlaySound?.Invoke("Hit_2");
                animator.SetBool("Get Up", false);
            }
            else if (angle > 135f || angle < -135f)
            {
                // Collision from left
                animator.SetTrigger("Side Splat");
                Splatted.Value = true;
                OnPlaySound?.Invoke("Hit_3");
                rend.flipX = true;
                animator.SetBool("Get Up", false);
            }
        }
    }
}
