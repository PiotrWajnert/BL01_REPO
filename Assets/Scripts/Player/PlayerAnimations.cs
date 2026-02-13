using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private Animator animator;
    private string currentAnimation;

    const string PLAYER_IDLE = "BruceIdle";
    const string PLAYER_RUN = "BruceRun";
    const string PLAYER_JUMP_IDLE = "BruceJumpIdle";
    const string PLAYER_JUMP_MOVE = "BruceJumpMove";
    const string PLAYER_FALL = "BruceFall";
    const string PLAYER_LADDER = "BruceLadder";
    const string PLAYER_LYING = "BruceLying";
    const string PLAYER_HIT = "BruceHit";
    const string PLAYER_KICK = "BruceKick";
    const string PLAYER_HURT = "BruceHurt";

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateMovementAnimation(float input)
    {
        if (input != 0)
        {
            ChangeAnimationState(PLAYER_RUN);
        }
        else
        {
            ChangeAnimationState(PLAYER_IDLE);
        }
    }

    public void PlayJumpAnimation(bool isDirectional)
    {
        if(isDirectional)
        {
            ChangeAnimationState(PLAYER_JUMP_MOVE);
        }
        else
        {
            ChangeAnimationState(PLAYER_JUMP_IDLE);
        }
    }

    public void PlayFallAnimation()
    {
        ChangeAnimationState(PLAYER_FALL);
    }

    public void PlayLyingAnimation()
    {
        ChangeAnimationState(PLAYER_LYING);
    }

    public void PlayClimbingAnimation()
    {
        ChangeAnimationState(PLAYER_LADDER);
    }

    public void PlayAttackAnimation(bool isMoving)
    {
        if (isMoving)
        {
            ChangeAnimationState(PLAYER_KICK);
        }
        else
        {
            ChangeAnimationState(PLAYER_HIT);
        }
    }

    public void PlayHurtAnimation()
    {
        ChangeAnimationState(PLAYER_HURT);
    }

    // Funkcja zapobiegaj¹ca restartowaniu tej samej animacji w ka¿dej klatce
    public void ChangeAnimationState(string newState)
    {
        if (currentAnimation == newState) return;

        animator.Play(newState);
        currentAnimation = newState;
    }

}