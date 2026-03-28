using UnityEngine;
using System.Collections;

public class FlameTrap : MonoBehaviour
{
    private Animator animator;
    private bool isTriggered = false; // Czy pu³apka ju¿ zaczê³a odliczaæ?
    private bool isLethal = false;    // Czy ogieñ ju¿ parzy?

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Ktoœ nadepn¹³ na kropkê (Bruce lub Ninja)
        if (!isTriggered && (collision.CompareTag("Player") || collision.CompareTag("Enemy")))
        {
            StartCoroutine(TrapSequence());
        }

        // 2. Ktoœ wszed³ w ju¿ pal¹cy siê ogieñ
        if (isLethal)
        {
            HandleLethalContact(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Wa¿ne: Jeœli Bruce stoi w miejscu, gdy ogieñ wybucha
        if (isLethal)
        {
            HandleLethalContact(collision);
        }
    }

    IEnumerator TrapSequence()
    {
        isTriggered = true;

        // Czekamy sekundê (kropka jeszcze nie zabija)
        yield return new WaitForSeconds(1.0f);

        // Wybuch ognia
        isLethal = true;
        if (animator != null) animator.Play("Flame_Burn");

        // Ogieñ p³onie przez 2 sekundy
        yield return new WaitForSeconds(2.0f);

        // Pu³apka znika
//        Destroy(gameObject);

        isLethal = false;
        isTriggered = false;
        animator.Play("Flame_Idle");
    }

    private void HandleLethalContact(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHurt ph = collision.GetComponent<PlayerHurt>();
            if (ph != null)
            {
                // Wywo³ujemy Twoj¹ standardow¹ œmieræ "freeze"
                ph.StartCoroutine(ph.InstantDeathRoutine());
            }
        }
        else if (collision.CompareTag("Enemy"))
        {
            // Ninja po prostu znika (lub mo¿esz wywo³aæ jego metodê œmierci)
            Destroy(collision.gameObject);
        }
    }


}