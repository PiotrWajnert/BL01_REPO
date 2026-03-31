using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;

    [Header("Death UI")]
    public GameObject deathPanel;
    public TextMeshProUGUI deathLivesText;
    public TextMeshProUGUI HUDLivesText;

    private int pointsTowardsExtraLife = 0;
    public int extraLifePoints = 500;


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
            pointsTowardsExtraLife += amount;

            if (pointsTowardsExtraLife >= extraLifePoints)
            {
                GameControl.instance.AddLife();
                pointsTowardsExtraLife -= extraLifePoints;
            }

            UpdateScoreUI();
        }
    }

    public void UpdateScoreUI()
    {
        if (GameControl.instance != null)
        {
            if (scoreText != null)
                scoreText.text = "SCORE: " + GameControl.instance.totalScore.ToString("D6");

//            if (livesText != null)
//                livesText.text = "LIVES: " + GameControl.instance.totalLives.ToString();

            if (HUDLivesText != null)
            {
                HUDLivesText.text = "FALLS " + GameControl.instance.totalLives.ToString();
            }
        }
    }

    public void ShowDeathScreen(int livesRemaining)
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            if (livesRemaining > 0)
                deathLivesText.text = "LIVES LEFT: " + livesRemaining;
            else
                deathLivesText.text = "GAME OVER";
        }
    }

}
