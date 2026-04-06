using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRewardTrigger : MonoBehaviour
{
    [SerializeField]
    public string sceneName;

    void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // WPISZ TUTAJ NAZWÊ SWOJEJ PIERWSZEJ SCENY
        if (currentScene == sceneName)
        {
            return; // Przerywa dzia³anie, nie daje punktów
        }

        if (GameControl.instance != null)
        {
            GameControl.instance.RewardLevelEntry(currentScene);
        }
    }
}