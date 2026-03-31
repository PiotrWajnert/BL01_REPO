using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRewardTrigger : MonoBehaviour
{
    void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // WPISZ TUTAJ NAZWÊ SWOJEJ PIERWSZEJ SCENY
        if (currentScene == "SampleScene")
        {
            return; // Przerywa dzia³anie, nie daje punktów
        }

        if (GameControl.instance != null)
        {
            GameControl.instance.RewardLevelEntry(currentScene);
        }
    }
}