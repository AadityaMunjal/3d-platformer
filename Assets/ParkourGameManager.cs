using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParkourGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform goalPosition;
    [SerializeField] private float goalRadius = 3f;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    
    private float gameTime = 0f;
    private bool gameActive = true;
    private bool gameWon = false;
    private Vector3 startPosition;
    
    private Rigidbody playerRb;
    private PlayerMovement playerMovement;
    
    void Start()
    {
        InitializeGame();
        SetupUI();
    }
    
    void Update()
    {
        if (gameActive && !gameWon)
        {
            gameTime += Time.deltaTime;
            UpdateUI();
            CheckWinCondition();
            CheckFallReset();
        }
        
        HandleInput();
    }
    
    private void InitializeGame()
    {
        if (player == null)
            player = FindObjectOfType<PlayerMovement>()?.transform;
        
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            playerMovement = player.GetComponent<PlayerMovement>();
            startPosition = player.position;
        }
        
        if (goalPosition == null)
        {
            GameObject goal = GameObject.Find("Goal Platform");
            if (goal != null)
                goalPosition = goal.transform;
        }
        
        if (winPanel != null)
            winPanel.SetActive(false);
    }
    
    private void SetupUI()
    {
        if (instructionsText != null)
        {
            instructionsText.text = "WASD - Move\\nSpace - Jump/Double Jump\\nShift - Sprint\\nC - Crouch\\n\\nRun along walls to wall run!\\nReach the goal as fast as possible!";
        }
    }
    
    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            int milliseconds = Mathf.FloorToInt((gameTime * 100) % 100);
            timerText.text = $"Time: {minutes:00}:{seconds:00}.{milliseconds:00}";
        }
        
        if (speedText != null && playerRb != null)
        {
            float speed = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z).magnitude;
            speedText.text = $"Speed: {speed:F1} m/s";
            
            if (playerMovement != null)
            {
                speedText.text += $"\\nState: {playerMovement.state}";
            }
        }
    }
    
    private void CheckWinCondition()
    {
        if (goalPosition != null && player != null)
        {
            float distance = Vector3.Distance(player.position, goalPosition.position);
            if (distance <= goalRadius)
            {
                WinGame();
            }
        }
    }
    
    private void CheckFallReset()
    {
        if (player.position.y < -10f)
        {
            ResetToStart();
        }
    }
    
    private void ResetToStart()
    {
        if (player != null)
        {
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
            
            player.position = startPosition + Vector3.up * 2f;
        }
    }
    
    private void WinGame()
    {
        if (!gameWon)
        {
            gameWon = true;
            gameActive = false;
            
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                
                if (finalTimeText != null)
                {
                    int minutes = Mathf.FloorToInt(gameTime / 60);
                    int seconds = Mathf.FloorToInt(gameTime % 60);
                    int milliseconds = Mathf.FloorToInt((gameTime * 100) % 100);
                    finalTimeText.text = $"Final Time: {minutes:00}:{seconds:00}.{milliseconds:00}";
                }
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    public void RestartGame()
    {
        gameTime = 0f;
        gameActive = true;
        gameWon = false;
        
        if (player != null)
        {
            player.position = startPosition + Vector3.up * 2f;
            
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
        }
        
        if (winPanel != null)
            winPanel.SetActive(false);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void OnDrawGizmosSelected()
    {
        if (goalPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(goalPosition.position, goalRadius);
        }
    }
}