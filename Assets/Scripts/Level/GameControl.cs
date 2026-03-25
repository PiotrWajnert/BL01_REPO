using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    public static GameControl instance;
    public string targetSpawnPointID; //spawn point

    public int totalScore = 0;

    private HashSet<string> collectedItems = new HashSet<string>();

    [Header("Player Stats")]
    public int totalLives = 5;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPoints(int amount)
    {
        totalScore += amount;
    }

    //sprawdzenie, czy przedmior zostal zebrany
    public bool IsItemCollected(string id)
    {
        return collectedItems.Contains(id);
    }

    //zapisanie zebranego ID
    public void RegisterCollection(string id)
    {
        if(!collectedItems.Contains(id))
        {
            collectedItems.Add(id);
        }
    }

    public void LoseLife()
    {
        totalLives--;

        if(totalLives <= 0)
        {
            Debug.Log("GAME OVER");
        }
        else
        {
            Debug.Log("Straciles zycie, zostalo " + totalLives);
        }
    }

    public bool IsGameOver()
    {
        return totalLives < 0;
    }
}
