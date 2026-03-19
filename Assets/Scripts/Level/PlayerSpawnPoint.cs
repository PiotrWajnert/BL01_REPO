using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    public string spawnPointID; // unikalne nazwy

    void Start()
    {
        // Sprawdzamy, czy to my jesteœmy wybranym punktem wejœcia
        if (GameControl.instance != null && GameControl.instance.targetSpawnPointID == spawnPointID)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;
            }
        }
    }
}