using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParkourUI : MonoBehaviour
{
    [Header("UI Creation Settings")]
    [SerializeField] private bool createUIOnStart = true;
    
    [Header("UI Style")]
    [SerializeField] private Font uiFont;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color panelColor = new Color(0, 0, 0, 0.7f);
    
    private Canvas mainCanvas;
    private GameObject timerUI;
    private GameObject winPanel;
    
    void Start()
    {
        if (createUIOnStart)
        {
            CreateGameUI();
        }
    }
    
    [ContextMenu("Create Game UI")]
    public void CreateGameUI()
    {
        CreateMainCanvas();
        CreateTimerUI();
        CreateWinPanel();
        
        AssignUIToGameManager();
    }
    
    private void CreateMainCanvas()
    {
        mainCanvas = FindObjectOfType<Canvas>();
        
        if (mainCanvas == null)
        {
            GameObject canvasObject = new GameObject("Main Canvas");
            mainCanvas = canvasObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;
            
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObject.AddComponent<GraphicRaycaster>();
        }
    }
    
    private void CreateTimerUI()
    {
        timerUI = new GameObject("Timer UI");
        timerUI.AddComponent<RectTransform>();
        timerUI.transform.SetParent(mainCanvas.transform, false);
        
        TextMeshProUGUI timerText = timerUI.AddComponent<TextMeshProUGUI>();
        timerText.text = "Time: 00:00.00";
        timerText.fontSize = 24;
        timerText.color = textColor;
        timerText.fontStyle = FontStyles.Bold;
        
        RectTransform rectTransform = timerUI.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(20, -20);
        rectTransform.sizeDelta = new Vector2(200, 50);
    }
    

    
    private void CreateWinPanel()
    {
        winPanel = new GameObject("Win Panel");
        winPanel.transform.SetParent(mainCanvas.transform, false);
        
        Image background = winPanel.AddComponent<Image>();
        background.color = panelColor;
        
        RectTransform panelRect = winPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        GameObject content = new GameObject("Content");
        content.AddComponent<RectTransform>();
        content.transform.SetParent(winPanel.transform, false);
        
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(400, 300);
        
        Image contentBg = content.AddComponent<Image>();
        contentBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        GameObject titleObj = new GameObject("Win Title");
        titleObj.AddComponent<RectTransform>();
        titleObj.transform.SetParent(content.transform, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "LEVEL COMPLETE!";
        titleText.fontSize = 36;
        titleText.color = Color.green;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        GameObject timeObj = new GameObject("Final Time");
        timeObj.AddComponent<RectTransform>();
        timeObj.transform.SetParent(content.transform, false);
        
        TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
        timeText.text = "Final Time: 00:00.00";
        timeText.fontSize = 24;
        timeText.color = textColor;
        timeText.fontStyle = FontStyles.Bold;
        timeText.alignment = TextAlignmentOptions.Center;
        
        RectTransform timeRect = timeObj.GetComponent<RectTransform>();
        timeRect.anchorMin = new Vector2(0, 0.5f);
        timeRect.anchorMax = new Vector2(1, 0.7f);
        timeRect.offsetMin = Vector2.zero;
        timeRect.offsetMax = Vector2.zero;
        
        GameObject buttonObj = new GameObject("Restart Button");
        buttonObj.AddComponent<RectTransform>();
        buttonObj.transform.SetParent(content.transform, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        Button restartButton = buttonObj.AddComponent<Button>();
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.25f, 0.1f);
        buttonRect.anchorMax = new Vector2(0.75f, 0.3f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Button text
        GameObject buttonTextObj = new GameObject("Button Text");
        buttonTextObj.AddComponent<RectTransform>();
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "RESTART (R)";
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        restartButton.onClick.AddListener(() => {
            ParkourGameManager gameManager = FindObjectOfType<ParkourGameManager>();
            if (gameManager != null)
                gameManager.RestartGame();
        });
        
        winPanel.SetActive(false);
    }
    
    private void AssignUIToGameManager()
    {
        ParkourGameManager gameManager = FindObjectOfType<ParkourGameManager>();
        if (gameManager != null)
        {
            var timerField = gameManager.GetType().GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var winPanelField = gameManager.GetType().GetField("winPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var finalTimeField = gameManager.GetType().GetField("finalTimeText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (timerField != null && timerUI != null)
                timerField.SetValue(gameManager, timerUI.GetComponent<TextMeshProUGUI>());
            
            if (winPanelField != null && winPanel != null)
                winPanelField.SetValue(gameManager, winPanel);
            
            if (finalTimeField != null && winPanel != null)
            {
                TextMeshProUGUI finalTimeText = winPanel.GetComponentInChildren<TextMeshProUGUI>();
                if (finalTimeText != null && finalTimeText.name == "Final Time")
                    finalTimeField.SetValue(gameManager, finalTimeText);
            }
        }
    }
}