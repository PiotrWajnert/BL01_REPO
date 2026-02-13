using UnityEngine;
using System.Collections;

public class PlayerFight : MonoBehaviour
{
    [Header("Attack Settings")]
    public float kickDuration = 0.5f; // Czas trwania blokady podczas kopniaka
    public float hitDuration = 0.3f;  // Czas trwania animacji uderzenia

    public Transform attackPoint;
    public float attackRange = 0.5f;   // Zasiêg ra¿enia
    public LayerMask enemyLayers;

    private PlayerMovement playerMovement;
    private PlayerAnimations playerAnimations;
    private PlayerHurt playerHurt;
    private Rigidbody2D rb2d;
    private bool isAttacking;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimations = GetComponent<PlayerAnimations>();
        rb2d = GetComponent<Rigidbody2D>();
        playerHurt = GetComponent<PlayerHurt>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking && IsGrounded())
        {
            StartCoroutine(PerformAttack());
        }
    }

    private bool IsGrounded()
    {
        // Pobieramy stan uziemienia ze skryptu ruchu
        return playerMovement.IsGrounded();
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        float currentInput = Input.GetAxisRaw("Horizontal");
        bool isMoving = currentInput != 0;

        playerHurt.isAttacking = true;
        playerAnimations.PlayAttackAnimation(isMoving);

        float originalGravity = rb2d.gravityScale;

        if (isMoving)
        {
            // --- LOGIKA KOPNIAKA (KICK) ---
            rb2d.gravityScale = 0f;
            // Zachowujemy pêd ruchu w locie
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 0f);

            float timer = 0f;


            while (timer < kickDuration)
            {
                    if (CheckForHit(true))
                    {
                        // --- EFEKT TRAFIENIA ---
                        // 1. Zatrzymujemy Bruce'a w miejscu (Hit-Stop)
                        rb2d.linearVelocity = Vector2.zero;

                        // 2. Czekamy chwilê w pozie kopniaka, ¿eby gracz poczu³ uderzenie
                        // Mo¿esz dostosowaæ tê wartoœæ (0.2f to solidne "pukniêcie")
                        yield return new WaitForSeconds(0.4f);

                        // 3. Wychodzimy z pêtli wczeœniej - Bruce nie leci ju¿ dalej
                        break;
                    }

                timer += Time.deltaTime;
                yield return null;
            }

            rb2d.gravityScale = originalGravity;
        }
        else
        {
            // --- LOGIKA UDERZENIA (HIT) ---
            rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
            CheckForHit(false); // W miejscu wystarczy sprawdziæ raz
            yield return new WaitForSeconds(hitDuration);
        }

        isAttacking = false;
        playerHurt.isAttacking = false;
    }

    private bool CheckForHit(bool isStrong)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        bool hitFound = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<NinjaAI>(out NinjaAI ninja))
            {
                // Jeœli komponent istnieje, 'ninja' jest ju¿ gotowy do u¿ycia
                //                ninja.TriggerHurt(ninja.hurtDuration, true, isStrong);
                int damage = isStrong ? 2 : 1; // Mocny atak (kopniak) za 2 HP, s³aby (cios) za 1 HP
                ninja.TakeDamage(damage, isStrong);
                hitFound = true;
            }
        }
        return hitFound;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}