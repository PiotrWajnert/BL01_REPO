using UnityEngine;

public class Ladder : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 2f;
    public float horizontalClimbMultiplier = 2f;
    public LayerMask ladderLayer;

    [Header("Animation Step")]
    public float stepDistance = 0.2f;

    private Rigidbody2D rb2d;
    private PlayerMovement playerMovement;
    private PlayerAnimations playerAnimations;
    private Animator animator;

    private bool isOnLadder;
    private float defaultGravityScale;
    private float distanceMoved;
    private int currentFrame = 0;

    // NOWE: Przechowuje prêdkoœæ automatyczn¹ z JJJJ
    private float autoSpeedFromLadder = 0f;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimations = GetComponent<PlayerAnimations>();
        animator = GetComponent<Animator>();
        defaultGravityScale = rb2d.gravityScale;
    }

    void Update()
    {
        if (isOnLadder)
        {
            HandleLadderInput();
        }
    }

    void HandleLadderInput()
    {
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        Vector2 moveDir = Vector2.zero;
        float currentMoveSpeed = climbSpeed;
        float animationMultiplier = 1f;

        // 1. Sprawdzamy kierunek wybrany przez gracza
        if (v != 0)
        {
            moveDir = new Vector2(0, v);
        }
        else if (h != 0)
        {
            moveDir = new Vector2(h, 0);
            currentMoveSpeed = climbSpeed * horizontalClimbMultiplier;
            animationMultiplier = 2f;
        }

        // 2. FIZYKA: £¹czymy ruch gracza z automatycznym ruchem schodów
        float finalVerticalVel = (moveDir.y * currentMoveSpeed) + autoSpeedFromLadder;
        float finalHorizontalVel = moveDir.x * currentMoveSpeed;
        rb2d.linearVelocity = new Vector2(finalHorizontalVel, finalVerticalVel);

        // 3. ANIMACJA: Liczymy dystans TYLKO jeœli gracz naciska klawisze (moveDir.magnitude > 0)
        if (moveDir.magnitude > 0)
        {
            // Tutaj u¿ywamy prêdkoœci wspinania gracza, ignoruj¹c autoSpeedFromLadder
            distanceMoved += (moveDir.magnitude * currentMoveSpeed * animationMultiplier) * Time.deltaTime;

            if (distanceMoved >= stepDistance)
            {
                ToggleLadderFrame();
                distanceMoved = 0f;
            }
        }
        // Jeœli gracz nic nie klika, distanceMoved nie roœnie, 
        // wiêc ToggleLadderFrame() nie zostanie wywo³ane.
    }

    void ToggleLadderFrame()
    {
        currentFrame = (currentFrame == 0) ? 1 : 0;
        animator.Play("BruceLadder", 0, currentFrame == 0 ? 0f : 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & ladderLayer) != 0)
        {
            // Sprawdzamy czy to zwyk³a drabina czy MovingLadder (JJJJ)
            MovingLadder ml = collision.GetComponent<MovingLadder>();
            autoSpeedFromLadder = (ml != null) ? ml.autoSpeed : 0f;

            StartClimbing();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & ladderLayer) != 0)
        {
            autoSpeedFromLadder = 0f;
            StopClimbing();
        }
    }

    void StartClimbing()
    {
        isOnLadder = true;
        rb2d.gravityScale = 0f;
        rb2d.linearVelocity = Vector2.zero;
        distanceMoved = 0f;
        animator.Play("BruceLadder", 0, 0f);
        animator.speed = 0f;
        if (playerMovement != null) playerMovement.enabled = false;
    }

    void StopClimbing()
    {
        isOnLadder = false;
        rb2d.gravityScale = defaultGravityScale;
        animator.speed = 1f;
        rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
        if (playerAnimations != null) playerAnimations.PlayFallAnimation();
        if (playerMovement != null) playerMovement.enabled = true;
    }
}