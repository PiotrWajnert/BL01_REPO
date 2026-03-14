using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI Reference")]
    public TextMeshProUGUI scoreText;

    private int currentScore = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints (int amount)  // wywolywane z innych skryptow
    {
        currentScore += amount;
        UpdateScoreUI();
        Debug.Log("punkty: " + currentScore);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + currentScore.ToString("D6");
        }
    }

}
