using UnityEditor.Tilemaps;
using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float jumpForce = 10f;
    public float jumpHorizontalSpeed = 4f;
    public float jumpDistance = 2f;
    public float maxFallSpeed = 2.5f;

    [Header("Wall Bounce Settings")]
    public float wallBounceForce = 1.5f;
    public float wallBounceDistance = 0.5f; // Jak daleko ma odskoczyæ przed blokad¹ X
    public LayerMask wallLayer;

    private bool isBouncing;
    private float bounceStartX;
    private bool bounceDistanceReached;

    [Header("Jump Settings")]
    public float jumpDelay = 0.15f; // Czas opóŸnienia skoku w IDLE

    [Header("GroundChecker")]
    public Transform groundChecker;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;
    public float groundWidthOffset = 0.1f;
    public float groundCheckHeight = 0.1f;

    [Header("Lying Collider Settings")]
    public Vector2 lyingColliderSize = new (0.2f, 0.1f);   // Nowy rozmiar (szerokoœæ, wysokoœæ)
    public Vector2 lyingColliderOffset = new (0f, -0.05f); // Nowy offset (przesuniêcie œrodka)

    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private bool isLying;

    private Rigidbody2D rb2d;
    private float horizontalInput;
    private PlayerAnimations playerAnimations;
    private BoxCollider2D boxCollider;
    private PlayerHurt playerHurt;

    private bool isGrounded;
    private bool isJumping;
    private bool isWaitingToJump; // Nowa flaga blokuj¹ca ruch podczas opóŸnienia
    private float startX;
    private bool distanceReached;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerAnimations = GetComponent<PlayerAnimations>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerHurt = GetComponent<PlayerHurt>();


        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;
    }

    void Update()
    {
        if (playerHurt.isAttacking || playerHurt.isHurt) return; // Blokada przy ataku LUB zranieniu
                                           //        if (isAttacking) return; // Jeœli walczymy, nie czytamy inputu skoku ani ruchu

        // Bruce pobiera input tylko gdy stoi pewnie na ziemi i nie przygotowuje siê do skoku
        if (isGrounded && !isLying && !isWaitingToJump)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }
        else if (isLying || isWaitingToJump)
        {
            horizontalInput = 0; // Blokada ruchu podczas le¿enia i czekania na skok
        }

        isGrounded = IsGrounded();

        // --- LOGIKA: Spadanie na g³owê Ninjy ---
        if (!isGrounded && rb2d.linearVelocity.y < -0.5f) // Zwiêkszony próg prêdkoœci spadania
        {
            int enemyLayerMask = LayerMask.GetMask("Enemy");
            RaycastHit2D hit = Physics2D.Raycast(groundChecker.position, Vector2.down, groundCheckDistance, enemyLayerMask);

            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                // POBIERAMY krawêdzie colliderów
                float feetY = groundChecker.position.y;
                float enemyHeadY = hit.collider.bounds.max.y; // Najwy¿szy punkt collidera przeciwnika

                // WARUNEK: Stopy Bruce'a musz¹ byæ wyraŸnie powy¿ej g³owy Ninjy 
                // lub na jej poziomie w momencie uderzenia (tolerancja 0.1f)
                if (feetY >= enemyHeadY - 0.1f)
                {
                    if (hit.collider.TryGetComponent<NinjaAI>(out var ninja))
                    {
                        ninja.TriggerHurt(ninja.hurtDuration, false);
//                        ninja.TakeDamage(0);


                        isJumping = true;
                        distanceReached = false;
                        startX = transform.position.x;
                    }
                }
            }
        }

        float verticalInput = Input.GetAxisRaw("Vertical");
        bool wantsToLie = isGrounded && !isJumping && !isWaitingToJump && (verticalInput < -0.5f || Input.GetKey(KeyCode.S));

        if (wantsToLie != isLying) // Zmieñ rozmiar tylko, gdy zmienia siê stan
        {
            isLying = wantsToLie;
            HandleColliderSize();
        }

        if (isGrounded && rb2d.linearVelocity.y <= 0.1f)
        {
            isJumping = false;
            distanceReached = false;
            isBouncing = false;
            bounceDistanceReached = false;
        }

        // Wyzwalanie skoku (TYLKO jeœli nie le¿ymy)
        if (isGrounded && !isJumping && !isWaitingToJump && !isLying)
        {
            bool jumpInput = Input.GetKeyDown(KeyCode.W) || Input.GetAxisRaw("Vertical") > 0.5f;
            if (jumpInput)
            {
                PerformJump();
            }
        }

        // --- ZARZ¥DZANIE ANIMACJAMI ---
        if (playerAnimations != null)
        {
            if (!isGrounded && !isJumping)
            {
                playerAnimations.PlayFallAnimation();
            }
            else if (isJumping)
            {
                // Logika skoku
            }
            else if (isGrounded && !isWaitingToJump)
            {
                if (isLying)
                {
                    playerAnimations.PlayLyingAnimation();
                }
                else
                {
                    playerAnimations.UpdateMovementAnimation(horizontalInput);
                }
            }
        }

        if (isJumping && !distanceReached)
        {
            float traveledDistance = Mathf.Abs(transform.position.x - startX);
            if (traveledDistance >= jumpDistance)
            {
                distanceReached = true;
                playerAnimations.PlayFallAnimation();
            }
        }

        // Kontrola dystansu odbicia od œciany
        if (isBouncing && !bounceDistanceReached)
        {
            float traveledDistance = Mathf.Abs(transform.position.x - bounceStartX);
            if (traveledDistance >= wallBounceDistance)
            {
                bounceDistanceReached = true;
                // Po osi¹gniêciu dystansu zerujemy prêdkoœæ X, aby spada³ pionowo
                rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
            }
        }

        Flip();
    }

    void FixedUpdate()
    {
        if (playerHurt.isAttacking || playerHurt.isHurt) return;
        //        if (isAttacking) return; // Jeœli walczymy, nie nadpisujemy prêdkoœci (Bruce zachowa pêd przy kopniaku)

        if(isLying || isWaitingToJump)
{
            // Blokujemy ruch poziomy podczas le¿enia ORAZ przygotowania do skoku
            rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
            return;
        }

        if (isGrounded && !isJumping)
        {
            isBouncing = false;
            bounceDistanceReached = false;
            rb2d.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb2d.linearVelocity.y);
        }
        else if (isJumping)
        {
            float targetXVelocity = distanceReached ? 0 : rb2d.linearVelocity.x;
            rb2d.linearVelocity = new Vector2(targetXVelocity, rb2d.linearVelocity.y);
        }
        else if (!isGrounded && !isJumping)
        {
            // Jeœli jesteœmy w trakcie odbicia:
            if (isBouncing)
            {
                // Jeœli osi¹gn¹³ dystans odbicia -> blokuj X. Jeœli nie -> leæ dalej (fizyka)
                float targetX = bounceDistanceReached ? 0 : rb2d.linearVelocity.x;
                rb2d.linearVelocity = new Vector2(targetX, rb2d.linearVelocity.y);
            }
            else
            {
                // Zwyk³e spadanie z krawêdzi -> blokuj X
                rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
            }
        }

        // Ograniczenie prêdkoœci spadania
        if (rb2d.linearVelocity.y < -maxFallSpeed)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, -maxFallSpeed);
        }
    }

    void PerformJump()
    {
        if (horizontalInput == 0)
        {
            // Skok pionowy - uruchamiamy rutynê z opóŸnieniem
            StartCoroutine(VerticalJumpRoutine());
        }
        else
        {
            // Skok kierunkowy - natychmiastowy
            ExecuteJump(true);
        }
    }

    IEnumerator VerticalJumpRoutine()
    {
        isWaitingToJump = true;

        // 2. RESET ANIMACJI: Wymuszamy start od 0 sekundy (klatka przygotowania)
        // Zak³adam, ¿e Twoja animacja skoku w miejscu nazywa siê "BruceJumpIdle" (zmieñ jeœli trzeba)
        if (playerAnimations != null)
        {
            // U¿ywamy bezpoœredniego dostêpu do Animatora dla precyzyjnego resetu
            GetComponent<Animator>().Play("BruceJumpIdle", 0, 0f);
        }

        yield return new WaitForSeconds(jumpDelay);

        isWaitingToJump = false;
        ExecuteJump(false);
    }

    void ExecuteJump(bool isDirectional)
    {
        // Odœwie¿amy input i flip tu¿ przed wybiciem
        horizontalInput = Input.GetAxisRaw("Horizontal");
        Flip();

        isJumping = true;
        startX = transform.position.x;

        if (!isDirectional)
        {
            distanceReached = true;
            rb2d.linearVelocity = new Vector2(0, jumpForce);
            playerAnimations.PlayJumpAnimation(false);
        }
        else
        {
            distanceReached = false;
            float direction = horizontalInput > 0 ? 1 : -1;
            rb2d.linearVelocity = new Vector2(direction * jumpHorizontalSpeed, jumpForce);
            playerAnimations.PlayJumpAnimation(true);
        }
    }

    // Pozosta³e metody: IsGrounded, Flip, OnDrawGizmos zostaj¹ bez zmian
    public bool IsGrounded()
    {
        if (boxCollider == null) return false;
        Vector2 checkSize = new(boxCollider.bounds.size.x + groundWidthOffset, groundCheckHeight);
        Vector2 checkCenter = new(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        RaycastHit2D hit = Physics2D.BoxCast(checkCenter, checkSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    void Flip()
    {
        if (isLying) return; // Bruce nie obraca siê le¿¹c

        // Bruce mo¿e siê obróciæ TYLKO na ziemi LUB gdy przygotowuje siê do skoku (jumpDelay)
        // W locie (isJumping) lub przy spadaniu Flip nie zadzia³a
        if (isGrounded || isWaitingToJump)
        {
            if (horizontalInput > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (horizontalInput < 0) transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 visualSize = new(boxCollider.bounds.size.x + groundWidthOffset, groundCheckHeight, 1f);
            Vector3 visualCenter = new(boxCollider.bounds.center.x, boxCollider.bounds.min.y - (groundCheckDistance / 2), 0f);
            Gizmos.DrawWireCube(visualCenter, visualSize);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Sprawdzamy, czy jesteœmy w powietrzu (skok lub spadek) i czy uderzyliœmy w œcianê
        if (!isGrounded && ((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            HandleWallBounce(collision);
        }
    }

    private void HandleWallBounce(Collision2D collision)
    {
        isJumping = false;
        distanceReached = true;

        isBouncing = true;
        bounceDistanceReached = false;
        bounceStartX = transform.position.x; // Zapamiêtujemy punkt startu odskoku

        float bounceDir = (collision.contacts[0].point.x > transform.position.x) ? -1f : 1f;
        rb2d.linearVelocity = new Vector2(bounceDir * wallBounceForce, rb2d.linearVelocity.y * 0.5f);

        if (playerAnimations != null)
        {
            playerAnimations.PlayFallAnimation();
        }
    }

    void HandleColliderSize()
    {
        if (isLying)
        {
            boxCollider.size = lyingColliderSize;
            boxCollider.offset = lyingColliderOffset;
        }
        else
        {
            boxCollider.size = originalColliderSize;
            boxCollider.offset = originalColliderOffset;
        }
    }

    

}