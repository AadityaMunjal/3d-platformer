using UnityEngine;

public class ParkourLevelGenerator : MonoBehaviour
{
    [Header("Level Generation Consts")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private Material platformMaterial;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material goalMaterial;
    
    [Header("Platform Consts")]
    [SerializeField] private int numberOfPlatforms = 15;
    [SerializeField] private float platformWidth = 3f;
    [SerializeField] private float platformLength = 3f;
    [SerializeField] private float platformHeight = 0.5f;
    [SerializeField] private float minPlatformGap = 2f;
    [SerializeField] private float maxPlatformGap = 6f;
    [SerializeField] private float heightVariation = 3f;
    
    [Header("Wall Consts")]
    [SerializeField] private int numberOfWalls = 8;
    [SerializeField] private float wallWidth = 0.5f;
    [SerializeField] private float wallHeight = 4f;
    [SerializeField] private float wallLength = 8f;
    
    [Header("Level Bounds")]
    [SerializeField] private float levelWidth = 30f;
    [SerializeField] private float levelLength = 100f;
    [SerializeField] private float startHeight = 2f;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateLevel();
        }
    }
    
    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        ClearLevel();
        CreateStartPlatform();
        CreateParkourCourse();
        CreateGoalPlatform();
    }
    
    [ContextMenu("Clear Level")]
    public void ClearLevel()
    {
        // Remove all children 
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    
    private void CreateStartPlatform()
    {
        GameObject startPlatform = CreatePlatform(Vector3.zero, "Start Platform");
        startPlatform.transform.localScale = new Vector3(platformWidth * 2, platformHeight, platformLength * 2);
    }
    
    private void CreateGoalPlatform()
    {
        Vector3 goalPosition = new Vector3(0, startHeight + heightVariation, levelLength);
        GameObject goalPlatform = CreatePlatform(goalPosition, "Goal Platform");
        goalPlatform.transform.localScale = new Vector3(platformWidth * 2, platformHeight, platformLength * 2);
        
        if (goalMaterial != null)
        {
            goalPlatform.GetComponent<Renderer>().material = goalMaterial;
        }
        
        GameObject goalMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goalMarker.transform.SetParent(transform);
        goalMarker.transform.position = goalPosition + Vector3.up * 3f;
        goalMarker.transform.localScale = new Vector3(0.5f, 3f, 0.5f);
        goalMarker.name = "Goal Marker";
        
        if (goalMaterial != null)
        {
            goalMarker.GetComponent<Renderer>().material = goalMaterial;
        }
    }
    
    private void CreateParkourCourse()
    {
        Vector3 currentPosition = Vector3.zero;
        
        for (int i = 0; i < numberOfPlatforms; i++)
        {
            float progress = (float)i / numberOfPlatforms;
            float zDistance = Random.Range(minPlatformGap, maxPlatformGap);
            float xOffset = Random.Range(-levelWidth * 0.3f, levelWidth * 0.3f);
            float yOffset = Random.Range(-1f, 2f);
            
            currentPosition += new Vector3(xOffset - currentPosition.x * 0.1f, yOffset, zDistance);
            currentPosition.y = Mathf.Clamp(currentPosition.y, startHeight - 1f, startHeight + heightVariation);
            
            GameObject platform = CreatePlatform(currentPosition, $"Platform_{i}");
            
            if (i > 2 && Random.Range(0f, 1f) < 0.4f)
            {
                CreateWallNearPlatform(currentPosition, i);
            }
        }
        
        CreateStandaloneWalls();
    }
    
    private GameObject CreatePlatform(Vector3 position, string name)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.SetParent(transform);
        platform.transform.position = position;
        platform.transform.localScale = new Vector3(platformWidth, platformHeight, platformLength);
        platform.name = name;
        
        if (platformMaterial != null)
        {
            platform.GetComponent<Renderer>().material = platformMaterial;
        }
        
        platform.layer = LayerMask.NameToLayer("Default");
        
        return platform;
    }
    
    private void CreateWallNearPlatform(Vector3 platformPosition, int platformIndex)
    {
        Vector3 wallPosition = platformPosition;
        wallPosition.x += Random.Range(-6f, 6f);
        wallPosition.y += wallHeight * 0.5f;
        wallPosition.z += Random.Range(-2f, 4f);
        
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(transform);
        wall.transform.position = wallPosition;
        wall.transform.localScale = new Vector3(wallWidth, wallHeight, wallLength);
        wall.name = $"Wall_Platform_{platformIndex}";
        
        wall.transform.rotation = Quaternion.Euler(0, Random.Range(0, 180), 0);
        
        if (wallMaterial != null)
        {
            wall.GetComponent<Renderer>().material = wallMaterial;
        }
        
        wall.layer = LayerMask.NameToLayer("Default");
    }
    
    private void CreateStandaloneWalls()
    {
        for (int i = 0; i < numberOfWalls; i++)
        {
            Vector3 wallPosition = new Vector3(
                Random.Range(-levelWidth * 0.4f, levelWidth * 0.4f),
                startHeight + Random.Range(1f, heightVariation),
                Random.Range(10f, levelLength - 10f)
            );
            
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.SetParent(transform);
            wall.transform.position = wallPosition;
            wall.transform.localScale = new Vector3(wallWidth, wallHeight, wallLength);
            wall.name = $"Wall_{i}";
            
            wall.transform.rotation = Quaternion.Euler(0, Random.Range(0, 180), 0);
            
            if (wallMaterial != null)
            {
                wall.GetComponent<Renderer>().material = wallMaterial;
            }
            
            wall.layer = LayerMask.NameToLayer("Default");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3(0, startHeight, levelLength * 0.5f), 
                           new Vector3(levelWidth, heightVariation * 2, levelLength));
    }
}