using UnityEngine;

public class ExtraLife : MonoBehaviour
{
    public string itemID;

    void Start()
    {
        // Jeœli ju¿ zebrane w tej sesji, usuñ
        if (GameControl.instance != null && GameControl.instance.IsItemCollected(itemID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameControl.instance != null)
            {
                GameControl.instance.AddLife();
                GameControl.instance.RegisterCollection(itemID);
            }

            Destroy(gameObject);
        }
    }
}