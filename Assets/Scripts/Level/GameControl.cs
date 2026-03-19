using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    public static GameControl instance;
    public string targetSpawnPointID; //spawn point

    public int totalScore = 0;

    private HashSet<string> collectedItems = new HashSet<string>();

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

}
