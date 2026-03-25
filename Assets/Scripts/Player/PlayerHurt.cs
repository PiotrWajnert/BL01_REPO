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
        if (isHurt || isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(LethalHitRoutine(attacker)); // Œmieræ od ciosu
        }
        else
        {
            StartCoroutine(HurtRoutine(hurtAnimationDuration, attacker, amount)); // Zwyk³y cios
        }
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

    public IEnumerator InstantDeathRoutine()
    {
        if (isDead) yield break; // Zabezpieczenie przed podwójn¹ œmierci¹
        isDead = true;

        if (movement != null) movement.enabled = false;

        // 1. ZATRZYMANIE I ANIMACJA
        rb2d.linearVelocity = Vector2.zero;
        rb2d.bodyType = RigidbodyType2D.Static;

        if (playerAnimations != null)
            playerAnimations.PlayFreezeAnimation();

        // 2. CZEKAMY CHWILÊ (¿eby gracz zobaczy³, ¿e zgin¹³)
        yield return new WaitForSeconds(1.0f);

        Time.timeScale = 0f; // zatrzymanie gry

        // 3. LOGIKA ¯YÆ I PANELU
        if (GameControl.instance != null)
        {
            GameControl.instance.LoseLife(); // Odejmujemy ¿ycie w GameControl

            if (ScoreManager.instance != null)
            {
                // Pokazujemy czarn¹ planszê z iloœci¹ pozosta³ych ¿yæ
                ScoreManager.instance.ShowDeathScreen(GameControl.instance.totalLives);
            }
        }

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;

        HandleDeathUIAndRestart();
    }

    private void HandleDeathUIAndRestart()
    {
        if (GameControl.instance != null)
        {
            GameControl.instance.LoseLife();
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.ShowDeathScreen(GameControl.instance.totalLives);
            }
        }

        StartCoroutine(ExecuteSceneRestart());
    }

    IEnumerator ExecuteSceneRestart()
    {
        Time.timeScale = 0f; // zatrzymanie gry

        yield return new WaitForSecondsRealtime(2.5f);

        Time.timeScale = 1f;

        if (GameControl.instance.totalLives < 0)
            SceneManager.LoadScene("MainMenu");
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator LethalHitRoutine(Transform attacker)
    {
        isDead = true;
        if (movement != null) movement.enabled = false;

        float direction = (attacker.position.x > transform.position.x) ? -1f : 1f;
        rb2d.linearVelocity = new Vector2(direction * strongKnockbackForce, 0.0f); // 0.0f to lekki podskok

        if (playerAnimations != null) playerAnimations.PlayHurtAnimation();

        yield return new WaitForSeconds(0.4f); // Czas lotu

        rb2d.linearVelocity = Vector2.zero;
        rb2d.bodyType = RigidbodyType2D.Static;

        HandleDeathUIAndRestart();
    }
}