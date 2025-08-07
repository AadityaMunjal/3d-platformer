using UnityEngine;

public class ParkourGameSetup : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool createPlayer = true;
    [SerializeField] private bool createCamera = true;
    [SerializeField] private bool createLevel = true;
    [SerializeField] private bool createUI = true;
    [SerializeField] private bool createGameManager = true;
    
    [Header("Player Setup")]
    [SerializeField] private Vector3 playerStartPosition = new Vector3(0, 2, 0);
    
    [Header("Camera Setup")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 1.5f, 0);
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupParkourGame();
        }
    }
    
    [ContextMenu("Setup Parkour Game")]
    public void SetupParkourGame()
    {
        if (createPlayer) SetupPlayer();
        if (createCamera) SetupCamera();
        if (createLevel) SetupLevel();
        if (createGameManager) SetupGameManager();
        if (createUI) SetupUI();
        
    }
    
    private void SetupPlayer()
    {
        PlayerMovement existingPlayer = FindObjectOfType<PlayerMovement>();
        if (existingPlayer != null) return;
        
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = playerStartPosition;
        
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.freezeRotation = true;
        
        GameObject orientation = new GameObject("Orientation");
        orientation.transform.SetParent(player.transform);
        orientation.transform.localPosition = Vector3.zero;
        
        PlayerMovement playerMovement = player.AddComponent<PlayerMovement>();
        playerMovement.orientation = orientation.transform;
        
    }
    
    private void SetupCamera()
    {
        PlayerCameraMovement existingCam = FindObjectOfType<PlayerCameraMovement>();
        if (existingCam != null) return;
        
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }
        
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            GameObject cameraPos = new GameObject("CameraPos");
            cameraPos.transform.SetParent(player.transform);
            cameraPos.transform.localPosition = cameraOffset;
            
            PlayerCameraMovement cameraMovement = mainCamera.gameObject.AddComponent<PlayerCameraMovement>();
            cameraMovement.cameraPos = cameraPos.transform;
            
            PlayerCamOrientation cameraOrientation = mainCamera.gameObject.AddComponent<PlayerCamOrientation>();
            Transform orientation = player.transform.Find("Orientation");
            if (orientation != null)
                cameraOrientation.orientation = orientation;
        }
        
    }
    
    private void SetupLevel()
    {
        ParkourLevelGenerator existingLevel = FindObjectOfType<ParkourLevelGenerator>();
        if (existingLevel != null) return;
        
        GameObject levelGenerator = new GameObject("Level Generator");
        ParkourLevelGenerator levelGen = levelGenerator.AddComponent<ParkourLevelGenerator>();
        
    }
    
    private void SetupGameManager()
    {
        ParkourGameManager existingManager = FindObjectOfType<ParkourGameManager>();
        if (existingManager != null) return;
        
        GameObject gameManager = new GameObject("Game Manager");
        ParkourGameManager manager = gameManager.AddComponent<ParkourGameManager>();
        
    }
    
    private void SetupUI()
    {
        ParkourUI existingUI = FindObjectOfType<ParkourUI>();
        if (existingUI != null) return;
        
        GameObject uiManager = new GameObject("UI Manager");
        ParkourUI ui = uiManager.AddComponent<ParkourUI>();
    }
}