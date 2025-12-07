using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float totalTime = 121f; // 2 minutes
    public float currentTime;

    public TMP_Text timerText;
    public TMP_Text scoreText;
    public int score = 0;

    public bool IsGameOver = false;

    public GameObject GameOverPanel;
    public GameObject RestartButton;
    public GameObject ExitButton;
    public bool GameOverPanelShown = false;



    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        currentTime = totalTime;
        UpdateScoreUI();
    }

    void Update()
    {
        // Countdown
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
           if (!GameOverPanelShown)
           {
                currentTime = 0;
                UpdateTimerUI();
                ShowGameOverPanel();
           }
        }
    }

    public void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    public void RestartGame()
    {
        GameOverPanel.SetActive(false);
        IsGameOver = false;
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ShowGameOverPanel()
    {
        GameOverPanel.SetActive(true);
        GameOverPanelShown = true;
    }
}

