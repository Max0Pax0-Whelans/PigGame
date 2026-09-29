using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerPickup humanPlayer;
    [SerializeField] private PlayerPickup aiPlayer;

    [Header("Scoring")]
    [SerializeField] private float pointsInterval = 3f;    // Seconds between point awards
    [SerializeField] private int pointsPerInterval = 1;    // Points awarded each interval

    [Header("Game Timer")]
    [SerializeField] private float gameDuration = 60f;      // Total game length in seconds

    [Header("UI - Scoreboard")]
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI aiScoreText;

    [Header("UI - Timer")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("UI - Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private int playerScore = 0;
    private int aiScore = 0;
    private float playerHoldTimer = 0f;
    private float aiHoldTimer = 0f;
    private float gameTimer = 0f;
    private bool gameActive = true;

    // Public accessors
    public int PlayerScore => playerScore;
    public int AIScore => aiScore;
    public float TimeRemaining => Mathf.Max(0f, gameDuration - gameTimer);
    public bool IsGameActive => gameActive;

    private void Start()
    {
        gameTimer = 0f;
        gameActive = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateScoreUI();
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!gameActive) return;

        // Advance game clock
        gameTimer += Time.deltaTime;

        // Check for time up
        if (gameTimer >= gameDuration)
        {
            EndGame();
            return;
        }

        // Track how long each side has held the ball
        if (humanPlayer != null && humanPlayer.IsHoldingItem())
        {
            playerHoldTimer += Time.deltaTime;
            if (playerHoldTimer >= pointsInterval)
            {
                playerHoldTimer -= pointsInterval;
                playerScore += pointsPerInterval;
                UpdateScoreUI();

                if (debugLogs) Debug.Log($"[Score] Player +{pointsPerInterval} (total: {playerScore})");
            }
        }
        else
        {
            // Optional: reset the timer when they lose the ball (see notes below)
            // playerHoldTimer = 0f;
        }

        if (aiPlayer != null && aiPlayer.IsHoldingItem())
        {
            aiHoldTimer += Time.deltaTime;
            if (aiHoldTimer >= pointsInterval)
            {
                aiHoldTimer -= pointsInterval;
                aiScore += pointsPerInterval;
                UpdateScoreUI();

                if (debugLogs) Debug.Log($"[Score] AI +{pointsPerInterval} (total: {aiScore})");
            }
        }
        else
        {
            // aiHoldTimer = 0f;
        }

        UpdateTimerUI();
    }

    private void UpdateScoreUI()
    {
        if (playerScoreText != null)
            playerScoreText.text = $"You: {playerScore}";

        if (aiScoreText != null)
            aiScoreText.text = $"AI: {aiScore}";
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            float t = TimeRemaining;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void EndGame()
    {
        gameActive = false;

        if (debugLogs) Debug.Log($"[Score] Game over. Player: {playerScore}, AI: {aiScore}");

        // Whoever has MORE points LOSES
        string result;
        if (playerScore > aiScore)
            result = "YOU LOSE!";
        else if (aiScore > playerScore)
            result = "YOU WIN!";
        else
            result = "DRAW!";

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverText != null)
        {
            gameOverText.text = $"{result}\n\nFinal Score\nYou: {playerScore}\nAI: {aiScore}";
        }
    }

    // Call this if you want a restart button
    public void RestartGame()
    {
        playerScore = 0;
        aiScore = 0;
        playerHoldTimer = 0f;
        aiHoldTimer = 0f;
        gameTimer = 0f;
        gameActive = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateScoreUI();
        UpdateTimerUI();
    }
}