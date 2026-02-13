using UnityEngine;
using System.Collections;

public class NinjaAI : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float attackRange = 0.8f;
    public float floorHeightThreshold = 1.0f;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float ladderSpeedMultiplier = 0.5f;
    public float attackDuration = 0.6f;
    public float attackCooldown = 0.2f;
    public float maxFallSpeed = 1.0f;

    [Header("Attack Detection")]
    public Transform attackPoint;
    public float attackEnemyRange = 0.5f;
    public LayerMask playerLayer;

    [Header("Attack Timing")]
    public float attackWindUp = 0.2f; // Czas rozmachu (brak obra¿eñ)
    private bool hasHitTargetInCurrentSwing = false; // Zapobiega wielokrotnym hitom

    [Header("Hurt Settings")]
    public float hurtDuration = 0.4f;
    public float hurtRecoveryTime = 0.5f;
    public float knockbackForce = 3f;
    private bool isHurt = false;
    public int fistDamage = 1;

    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    public bool isDead = false;

    [Header("Detection")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    // STARY groundChecker zostaje tylko do wykrywania Bruce'a pod stopami (stomp)
    public Transform groundChecker;
    public float groundCheckDistance = 0.2f; // U¿ywane teraz jako dystans rzutowania pude³ka
    public float wallCheckDistance = 0.3f;

    [Header("Ground CheckBox")]
    public float groundCheckWidthOffset = 0f;
    public float groundCheckHeight = 0.2f;

    [Header("AI Logic")]
    public float reactionDelay = 0.5f;
    private bool isReacting = false;

    private Rigidbody2D rb2d;
    private NinjaAnimations ninjaAnim;
    private BoxCollider2D boxCollider; // Potrzebujemy kolidera do wymiarów boxa
    private bool isGrounded;
    private bool isOnLadder;
    private bool isAttacking;
    private bool wasGroundedInLastFrame;

    private float searchDirection = 0f;
    private float desiredVelocityX;

    private Coroutine attackCoroutine;
    private int playerLayerMask;

    // Obliczaj¹ ró¿nice pozycji "w locie"
    private float XDiff => target != null ? target.position.x - transform.position.x : 0f;
    private float YDiff => target != null ? target.position.y - transform.position.y : 0f;

    // Czytelne warunki logiczne
    private bool IsPlayerOnSameFloor => Mathf.Abs(YDiff) < floorHeightThreshold;
    private bool IsPlayerAbove => YDiff >= floorHeightThreshold;
    private bool IsPlayerBelow => YDiff <= -floorHeightThreshold;
    private bool IsInAttackRange => Mathf.Abs(XDiff) <= attackRange;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        ninjaAnim = GetComponent<NinjaAnimations>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerLayerMask = LayerMask.GetMask("Player");
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        isGrounded = CheckGround();

        if (!isHurt && !isOnLadder)
        {
            rb2d.gravityScale = 1f;
        }

        if (isHurt) return;

        if (Input.GetKeyDown(KeyCode.L))
        {
            TriggerHurt(hurtDuration, true);
            return;
        }

        // 3. Detekcja spadania na g³owê Bruce'a (Dual Raycast)
        if (rb2d.linearVelocity.y < -0.1f) // Sprawdzamy tylko gdy Ninja spada
        {
            float sideOffset = boxCollider.size.x * 0.45f; // Promienie blisko krawêdzi kolidera
            Vector2 leftOrigin = (Vector2)transform.position + new Vector2(-sideOffset, boxCollider.offset.y - (boxCollider.size.y / 2));
            Vector2 rightOrigin = (Vector2)transform.position + new Vector2(sideOffset, boxCollider.offset.y - (boxCollider.size.y / 2));

            // Rzucamy dwa promienie w dó³
            RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, groundCheckDistance*2, playerLayerMask);
            RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, groundCheckDistance*2, playerLayerMask);

            // Jeœli którykolwiek promieñ trafi Bruce'a
            RaycastHit2D finalHit = hitLeft.collider != null ? hitLeft : hitRight;

            if (finalHit.collider != null && finalHit.collider.CompareTag("Player"))
            {
                if (finalHit.collider.TryGetComponent<PlayerHurt>(out var bruceHurt))
                {
                    bruceHurt.TakeDamage(0, transform);
                }
            }
        }

        if (!isGrounded && !isOnLadder)
        {
            HandleFalling();
            wasGroundedInLastFrame = false; // Zapobiega b³êdom l¹dowania
            return;
        }

        // 4. Reakcja na l¹dowanie
        if (isGrounded && !wasGroundedInLastFrame)
        {
            if (!isReacting && rb2d.linearVelocity.y <= 0.1f)
            {
                StartCoroutine(LandingReactionRoutine());
            }
        }
        wasGroundedInLastFrame = isGrounded;

        // 5. ŒCIANY BLOKUJ¥CE
        if (isAttacking) return;

        // Jeœli jest w powietrzu, nie podejmuje decyzji o ruchu, tylko spada
        if (!isGrounded && rb2d.linearVelocity.y < -0.1f)
        {
            HandleFalling();
            return;
        }

        // 6. Normalna logika AI (tylko gdy na ziemi)
        if (isGrounded)
        {
            DetermineBehavior();
        }
    }

    void FixedUpdate()
    {
        if (isDead || isHurt) return;

        // 1. Zastosowanie prêdkoœci poziomej obliczonej w Update
        rb2d.linearVelocity = new Vector2(desiredVelocityX, rb2d.linearVelocity.y);

        // 2. Ograniczenie prêdkoœci spadania (clamping)
        if (rb2d.linearVelocity.y < -maxFallSpeed)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, -maxFallSpeed);
        }
    }

    IEnumerator LandingReactionRoutine()
    {
        isReacting = true;
        yield return new WaitForSeconds(reactionDelay);
        isReacting = false;
        searchDirection = 0f;
    }

    void DetermineBehavior()
    {
        if (target == null) return;

        // Logika reakcji po l¹dowaniu
        if (isReacting)
        {
            if (searchDirection != 0f) MoveInDirection(searchDirection);
            else Idle();
            return;
        }

        // G£ÓWNA LOGIKA DECYZYJNA
        if (IsPlayerOnSameFloor)
        {
            searchDirection = 0f;

            if (IsInAttackRange)
            {
                LookAt(target.position.x);
                attackCoroutine ??= StartCoroutine(AttackRoutine());
            }
            else
            {
                MoveInDirection(XDiff > 0 ? 1f : -1f);
            }
        }
        else if (IsPlayerAbove)
        {
            searchDirection = 0f;
            LookAt(target.position.x);
            Idle();
        }
        else if (IsPlayerBelow)
        {
            SearchForDrop(XDiff);
        }
    }

    void SearchForDrop(float xDiff)
    {
        if (searchDirection == 0f)
        {
            searchDirection = xDiff > 0 ? 1f : -1f;
        }

        if (IsWallInFront())
        {
            searchDirection *= -1f;
        }

        MoveInDirection(searchDirection);
    }

    bool IsWallInFront()
    {
        Vector2 direction = new(searchDirection, 0);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);
        Debug.DrawRay(transform.position, direction * wallCheckDistance, Color.blue);
        return hit.collider != null;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        desiredVelocityX = 0f;
        hasHitTargetInCurrentSwing = false; // Resetujemy flagê na pocz¹tku ciosu

        ninjaAnim.PlayHitForce();

        float timer = 0f;

        while (timer < attackDuration)
        {
            // 1. Przerwij, jeœli Ninja spadnie z krawêdzi
            if (!isGrounded)
            {
                isAttacking = false;
                attackCoroutine = null;
                yield break;
            }

            // 2. FAZA AKTYWNA: Sprawdzamy cios tylko PO up³ywie czasu rozmachu
            if (timer >= attackWindUp && !hasHitTargetInCurrentSwing)
            {
                Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackEnemyRange, playerLayer);
                if (hit != null)
                {
                    if (hit.TryGetComponent<PlayerHurt>(out var bruceHurt))
                    {
                        bruceHurt.TakeDamage(fistDamage, transform);
                        hasHitTargetInCurrentSwing = true; // Zada³ obra¿enia, stop do koñca tego zamachu
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Koniec animacji ciosu
        ninjaAnim.PlayIdle();
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        attackCoroutine = null;
    }

    void MoveInDirection(float dir)
    {
        float currentSpeed = isOnLadder ? moveSpeed * ladderSpeedMultiplier : moveSpeed;

        // ZMIANA: Tylko zapisujemy intencjê ruchu
        desiredVelocityX = dir * currentSpeed;

        ninjaAnim.Flip(dir);
        if (isOnLadder) ninjaAnim.PlayLadder();
        else ninjaAnim.PlayRun();
    }

    void Idle()
    {
        // ZMIANA: Chcemy staæ w miejscu
        desiredVelocityX = 0f;

        if (isOnLadder) ninjaAnim.PlayLadderIdle();
        else ninjaAnim.PlayIdle();
    }

    void HandleFalling()
    {
        // ZMIANA: Spadaj¹c pionowo, nie chcemy prêdkoœci X
        desiredVelocityX = 0f;
        if (!isOnLadder) rb2d.gravityScale = 1f;
        ninjaAnim.PlayFall();
    }

    void LookAt(float targetX)
    {
        float direction = (targetX > transform.position.x) ? 1f : -1f;
        ninjaAnim.Flip(direction);
    }

    // --- NOWA METODA CHECKGROUND (BOXCAST) ---
    bool CheckGround()
    {
        if (boxCollider == null) return false;

        Vector2 checkCenter = new(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 checkSize = new(boxCollider.bounds.size.x + groundCheckWidthOffset, groundCheckHeight);

        RaycastHit2D hit = Physics2D.BoxCast(checkCenter, checkSize, 0f, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider != null;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder") || collision.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            isOnLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder") || collision.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            isOnLadder = false;
        }
    }

    public void TriggerHurt(float duration, bool applyKnockback = true, bool isStrongHit = true)
    {
        // Zamiast StopAllCoroutines, zatrzymaj tylko konkretne akcje
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        // Zatrzymujemy poprzedni HurtRoutine, ¿eby nie nak³ada³y siê na siebie
        StopAllCoroutines();

        // WYMUSZAMY reset grawitacji przed startem nowej korutyny
        rb2d.gravityScale = 1f;

        isReacting = false;
        StartCoroutine(HurtRoutine(duration, applyKnockback, isStrongHit));
    }

    // Wewn¹trz NinjaAI.cs
    public void TakeDamage(int damage, bool isStrong = false)
    {
        if (isDead) return;

        currentHealth -= damage;

        // ZAWSZE odpalaj efekt uderzenia (odrzut + animacja)
        TriggerHurt(hurtDuration, true, isStrong);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Wy³¹czamy kolizjê natychmiast, ¿eby nie blokowa³ gracza
        if (boxCollider != null) boxCollider.enabled = false;

        // Zmieniamy warstwê, ¿eby inne systemy go ignorowa³y
        gameObject.layer = LayerMask.NameToLayer("Default");

        // Uruchamiamy now¹ sekwencjê zamiast prostego Destroy
        StartCoroutine(DeathSequenceRoutine());
    }

    IEnumerator DeathSequenceRoutine()
    {
        // FAZA 1: Pozwalamy Ninjy lecieæ po ciosie (odrzut z HurtRoutine)
        yield return new WaitForSeconds(0.4f);

        // FAZA 2: ZAMRO¯ENIE
        // Zatrzymujemy fizykê ca³kowicie
        rb2d.linearVelocity = Vector2.zero;
        rb2d.bodyType = RigidbodyType2D.Static; // To "wbija go w miejsce", przestaje nawet spadaæ

        // Opcjonalnie: zatrzymanie animacji w obecnej klatce
        if (TryGetComponent<Animator>(out var animator)) animator.speed = 0f;

        // FAZA 3: Czekamy sekundê w zamro¿eniu
        yield return new WaitForSeconds(0.7f);

        // FAZA 4: Znikniêcie
        Destroy(gameObject);
    }

    IEnumerator HurtRoutine(float duration, bool applyKnockback, bool isStrongHit)
    {
        isHurt = true;
        isAttacking = false;
        float originalGravity = rb2d.gravityScale;

        if (target != null)
        {
            float faceDir = (target.position.x > transform.position.x) ? 1f : -1f;
            ninjaAnim.Flip(faceDir);
        }

        rb2d.gravityScale = 0f;
        rb2d.linearVelocity = Vector2.zero;

        if (applyKnockback)
        {
            float knockDir = (target.position.x > transform.position.x) ? -1f : 1f;
            float finalKnockback = isStrongHit ? knockbackForce : knockbackForce * 0.5f;
            rb2d.linearVelocity = new Vector2(knockDir * finalKnockback, 0f);
        }

        if (isStrongHit) ninjaAnim.PlayHurt();
        else ninjaAnim.PlayIdle();

        float finalDuration = isStrongHit ? duration : duration * 0.5f;
        yield return new WaitForSeconds(finalDuration);

        if (rb2d.bodyType != RigidbodyType2D.Static)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.gravityScale = originalGravity;
        }

        if (isDead) yield break; // Jeœli nie ¿yje, zatrzymujemy korutynê tutaj (zostaje w animacji Hurt)

        // Jeœli ¿yje, kontynuuj normalne odzyskiwanie si³:
        isGrounded = CheckGround();
        if (!isGrounded) ninjaAnim.PlayFall();
        else ninjaAnim.PlayIdle();

        float finalRecovery = isStrongHit ? hurtRecoveryTime : hurtRecoveryTime * 0.5f;
        yield return new WaitForSeconds(finalRecovery);

        isHurt = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackEnemyRange);
        }

        // --- RYSOWANIE BOXA SPRAWDZAJ¥CEGO ZIEMIÊ ---
        // Upewniamy siê, ¿e mamy kolider, ¿eby pobraæ jego wymiary
        if (TryGetComponent<BoxCollider2D>(out var colliderForGizmo))
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;

            Vector2 gizmoCenter = new(colliderForGizmo.bounds.center.x, colliderForGizmo.bounds.min.y - groundCheckDistance);
            Vector2 gizmoSize = new(colliderForGizmo.bounds.size.x + groundCheckWidthOffset, groundCheckHeight);

            Gizmos.DrawWireCube(gizmoCenter, gizmoSize);
        }

        // Rysujemy stary groundChecker na niebiesko, s³u¿y do wykrywania stompa
        if (boxCollider != null)
        {
            Gizmos.color = Color.cyan;
            float sideOffset = boxCollider.size.x * 0.45f;
            Vector2 bottomCenter = (Vector2)transform.position + new Vector2(0, boxCollider.offset.y - (boxCollider.size.y / 2));

            Vector3 leftRay = bottomCenter + new Vector2(-sideOffset, 0);
            Vector3 rightRay = bottomCenter + new Vector2(sideOffset, 0);

            Gizmos.DrawRay(leftRay, Vector2.down * groundCheckDistance * 2);
            Gizmos.DrawRay(rightRay, Vector2.down * groundCheckDistance * 2);
        }
    }
}