using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints (int amount)  // wywolywane z innych skryptow
    {
        if (GameControl.instance != null);
        {
            GameControl.instance.AddPoints (amount);
            UpdateScoreUI();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null && GameControl.instance != null)
        {
            scoreText.text = "SCORE: " + GameControl.instance.totalScore.ToString("D6");
        }
    }

}
