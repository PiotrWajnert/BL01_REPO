using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHurt : MonoBehaviour
{
    [Header("Health")]
    public int currentHealth = 3;

    [Header("Hurt Settings")]
    public float hurtAnimationDuration = 0.5f;
    public float strongKnockbackForce = 2f;
    public float weakKnockbackForce = 0.7f;

    [HideInInspector] public bool isHurt;
    [HideInInspector] public bool isAttacking;


    private Rigidbody2D rb2d;
    private PlayerAnimations playerAnimations;
    private PlayerMovement movement;

    private bool isDead = false;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerAnimations = GetComponent<PlayerAnimations>();
        movement = GetComponent<PlayerMovement>();
    }
    public void TakeDamage(int amount, Transform attacker)
    {
        if (isHurt) return;

        currentHealth -= amount;
        if (currentHealth < 1) currentHealth = 1;

        StartCoroutine(HurtRoutine(hurtAnimationDuration, attacker, amount));
    }
    public void TriggerHurt(float duration)
    {
        if (!isHurt)
        {
            // Traktujemy to jako "s³abe" uderzenie bez konkretnego kierunku (np. pionowe)
            StartCoroutine(HurtRoutine(duration, null, 0));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lethal") && !isDead)
        {
            StartCoroutine(InstantDeathRoutine());
        }
    }

    IEnumerator HurtRoutine(float duration, Transform attacker, int damageAmount)
    {
        isHurt = true;
        isAttacking = false;
        float originalGravity = rb2d.gravityScale;

        // 1. Obrót (bez zmian)
        if (attacker != null && damageAmount > 0)
        {
            float faceDir = (attacker.position.x > transform.position.x) ? 1f : -1f;
            transform.localScale = new Vector3(faceDir, 1, 1);
        }

        // 2. Fizyka (bez zmian)
        rb2d.gravityScale = 0f;
        rb2d.linearVelocity = Vector2.zero;

        if (attacker != null)
        {
            float knockDir = (attacker.position.x > transform.position.x) ? -1f : 1f;
            float force = (damageAmount == 1) ? weakKnockbackForce : (damageAmount >= 2 ? strongKnockbackForce : 0f);
            rb2d.linearVelocity = new Vector2(knockDir * force, 0f);
        }

        // --- 3. KLUCZOWA POPRAWKA ANIMACJI ---
        if (playerAnimations != null)
        {
            if (damageAmount == 0 || damageAmount >= 2)
                playerAnimations.PlayHurtAnimation(); // U¿ywamy metody ze skryptu!
            else
                playerAnimations.UpdateMovementAnimation(0f); // Dla damage 1 wymuszamy Idle
        }

        float finalWait = (damageAmount == 1) ? duration * 0.5f : duration;
        yield return new WaitForSeconds(finalWait);

        // --- 4. POWRÓT DO IDLE ---
        rb2d.linearVelocity = Vector2.zero;
        rb2d.gravityScale = originalGravity;

        if (playerAnimations != null)
        {
            // Tutaj "oszukujemy" system, ¿eby na pewno odœwie¿y³ animacjê
            playerAnimations.ChangeAnimationState("BruceIdle");
        }

        yield return new WaitForSeconds(0.1f);
        isHurt = false;
    }

    IEnumerator InstantDeathRoutine()
    {
        isDead = true;

        if (movement != null)
        {
            movement.enabled = false;
        }

        // 1. ZATRZYMANIE RUCHU
        rb2d.linearVelocity = Vector2.zero;
        rb2d.bodyType = RigidbodyType2D.Static; // Bruce nie spada i nie reaguje na fizykê

        // 2. ANIMACJA ZAMRO¯ENIA
        playerAnimations.ChangeAnimationState("BruceFreeze");

        // 3. CZEKANIE 1 SEKUNDÊ
        yield return new WaitForSeconds(1.0f);

        // 4. RESET SCENY
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}