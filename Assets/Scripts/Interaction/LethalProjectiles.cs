using UnityEngine;

public class LethalProjectiles : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 3f;
    public float lifetime = 5f;

    void Start()
    {
        //zniszcz po czasie lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Ruch w kierunku, w którym jest zwrócony obiekt (jego oœ X)
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHurt ph = collision.GetComponent<PlayerHurt>();
            if (ph != null)
            {
                // WA¯NE: Musisz u¿yæ StartCoroutine na obiekcie gracza (ph), 
                // bo pocisk zaraz zostanie zniszczony i jego w³asne korutyny przestan¹ dzia³aæ!
                ph.StartCoroutine(ph.InstantDeathRoutine());
            }

            Destroy(gameObject); // Pocisk znika
        }
    }
}