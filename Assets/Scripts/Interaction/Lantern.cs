using UnityEngine;

public class Lantern : MonoBehaviour
{
    [Header("Settings")]
    public int pointsValue = 500;

    [Header("Name")]
    public string itemID; //unikalne ID

    void Start()
    {
        //sprawdzenie czy juz zostalo zebrane
        if(GameControl.instance != null && GameControl.instance.IsItemCollected(itemID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //dodanie punktow
            ScoreManager.instance.AddPoints(pointsValue);

            //rejestrujemy zebranie
            if(GameControl.instance != null)
            {
                GameControl.instance.RegisterCollection(itemID);
            }

            // Tutaj w przysz³oœci dodamy dŸwiêk lub cz¹steczki
            Debug.Log("Lampion zebrany! + " + pointsValue);

            // Usuwamy lampion ze sceny
            Destroy(gameObject);
        }
    }
}