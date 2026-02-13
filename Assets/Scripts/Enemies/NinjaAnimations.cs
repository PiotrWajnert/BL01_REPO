using UnityEngine;

public class NinjaAnimations : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private string currentState;

    // Nazwy stanów z Twojego Animatora
    const string NINJA_IDLE = "NinjaIdle";
    const string NINJA_RUN = "NinjaRun";
    const string NINJA_FALL = "NinjaFall";
    const string NINJA_HIT = "NinjaHit";
    const string NINJA_HURT = "NinjaHurt";
    const string NINJA_LADDER = "NinjaLadder";
    const string NINJA_LADDER_IDLE = "NinjaLadderIdle";

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Podstawowa funkcja zmieniaj¹ca stany
    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);
        currentState = newState;
    }

    // --- METODY DO WYWO£ANIA PRZEZ SKRYPT AI ---

    public void PlayIdle() => ChangeAnimationState(NINJA_IDLE);

    public void PlayRun() => ChangeAnimationState(NINJA_RUN);

    public void PlayFall() => ChangeAnimationState(NINJA_FALL);

    public void PlayLadder() => ChangeAnimationState(NINJA_LADDER);

    public void PlayLadderIdle() => ChangeAnimationState(NINJA_LADDER_IDLE);

    public void PlayHit()
    {
        currentState = "";
        animator.Play(NINJA_HIT, 0, 0f);
        currentState = NINJA_HIT;
    }
    public void PlayHurt()
    {
        // Czyœcimy stan, aby wymusiæ ponowne odegranie, jeœli dostanie seriê ciosów
        currentState = "";
        animator.Play(NINJA_HURT, 0, 0f);
        currentState = NINJA_HURT;
    }

    
    public void PlayHitForce()
    {
        // Resetujemy currentState, ¿eby ChangeAnimationState nie zablokowa³o ataku
        currentState = "";
        // Wymuszamy start animacji od klatki 0
        animator.Play("NinjaHit", 0, 0f);
        currentState = "NinjaHit";
    }

    /// <summary>
    /// Obraca Ninja w stronê celu.
    /// direction > 0 to prawo, direction < 0 to lewo.
    /// </summary>
    public void Flip(float direction)
    {
        if (direction > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}