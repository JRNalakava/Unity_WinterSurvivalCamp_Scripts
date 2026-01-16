using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class WildernessSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Drag a Prefab from your Project window here (not a scene object).")]
    public GameObject enemyPrefab; 
    public int maxEnemies = 3;
    public float spawnInterval = 15f;
    
    [Header("Spawn Location")]
    [Range(0f, 100f)] public float minSpawnRadius = 10f; // Safe Zone
    [Range(0f, 100f)] public float maxSpawnRadius = 30f; // Outer Limit
    
    public List<Transform> explicitSpawnPoints;

    [Header("Debug")]
    // [SerializeField] private List<GameObject> spawnedInstances = new List<GameObject>(); // Causes errors if items deleted inspecting
    private List<GameObject> spawnedInstances = new List<GameObject>(); // Hidden from Inspector to prevent errors

    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"WildernessSpawner on {gameObject.name}: No enemyPrefab assigned! Please assign a Prefab from the Project folder.");
            return;
        }

        // Validate Radius
        if (minSpawnRadius >= maxSpawnRadius)
        {
            maxSpawnRadius = minSpawnRadius + 5f;
            Debug.LogWarning("WildernessSpawner: Min Radius was >= Max Radius. Adjusting Max Radius automatically.");
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            spawnedInstances.RemoveAll(item => item == null);

            if (spawnedInstances.Count < maxEnemies)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(1f); // Fast fill
            }
            else
            {
                yield return new WaitForSeconds(spawnInterval); // Maintain
            }
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPos = Vector3.zero;
        bool validPositionFound = false;

        // A. Explicit Points
        if (explicitSpawnPoints != null && explicitSpawnPoints.Count > 0)
        {
            Transform pt = explicitSpawnPoints[Random.Range(0, explicitSpawnPoints.Count)];
            if (pt != null)
            {
                spawnPos = pt.position;
                validPositionFound = true;
            }
        }

        // B. NavMesh Annulus (Safe Zone)
        if (!validPositionFound)
        {
            spawnPos = GetRandomPointInAnnulus();
            NavMeshHit hit;
            // Sample closest NavMesh point to our random donut point
            if (NavMesh.SamplePosition(spawnPos, out hit, 10f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
                // Double check distance to ensure NavMesh sample didn't pull it into the safe zone
                float dist = Vector3.Distance(transform.position, spawnPos);
                if (dist >= minSpawnRadius) 
                {
                    validPositionFound = true;
                }
            }
        }

        if (validPositionFound)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            spawnedInstances.Add(newEnemy);
        }
    }

    private Vector3 GetRandomPointInAnnulus()
    {
        // 1. Get random direction
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        
        // 2. Get random distance between Min and Max
        float randomDist = Random.Range(minSpawnRadius, maxSpawnRadius);
        
        // 3. Scale
        Vector2 point = randomCircle * randomDist;
        
        // 4. Convert to 3D (X, Z) relative to spawner
        return transform.position + new Vector3(point.x, 0, point.y);
    }

    private void OnDrawGizmosSelected()
    {
        // Outer Limit (Cyan)
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);

        // Safe Zone (Red)
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, minSpawnRadius);
    }
}
