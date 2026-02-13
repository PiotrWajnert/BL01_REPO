using UnityEngine;

public class MovingLadder : MonoBehaviour
{
    [Header("Auto Movement Settings")]
    public float autoSpeed = 1.5f; // Dodatnia góra, ujemna dó³

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Obs³uga przeciwników (oni po prostu p³yn¹ góra/dó³)
        if (collision.CompareTag("Enemy"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, autoSpeed);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null) rb.gravityScale = 1f; // Przywrócenie grawitacji wrogom
        }
    }
}