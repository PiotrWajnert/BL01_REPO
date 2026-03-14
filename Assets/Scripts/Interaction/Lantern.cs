using UnityEngine;

public class Lantern : MonoBehaviour
{
    [Header("Settings")]
    public int pointsValue = 500;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ScoreManager.instance.AddPoints(pointsValue);

            // Tutaj w przysz³oœci dodamy dŸwiêk lub cz¹steczki
            Debug.Log("Lampion zebrany! + " + pointsValue);

            // Usuwamy lampion ze sceny
            Destroy(gameObject);
        }
    }
}