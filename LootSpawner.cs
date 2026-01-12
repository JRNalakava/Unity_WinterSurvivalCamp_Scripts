using UnityEngine;

public static class LootSpawner
{
    public static void Spawn(GameObject prefab, Vector3 centerPosition, int count, float explosionForce = 300f)
    {
        if (prefab == null) return;

        for (int i = 0; i < count; i++)
        {
            SpawnSingle(prefab, centerPosition, explosionForce);
        }
    }

    public static void SpawnSingle(GameObject prefab, Vector3 centerPosition, float explosionForce = 300f)
    {
        if (prefab == null) return;

        // Spread spawn position slightly so they don't clip instantly
        Vector3 spawnPos = centerPosition + Vector3.up * 1.0f + Random.insideUnitSphere * 0.5f;
        
        GameObject item = Object.Instantiate(prefab, spawnPos, Quaternion.identity);

        // Check for RB, if missing, add one so it can drop properly
        if (!item.TryGetComponent(out Rigidbody rb))
        {
            rb = item.AddComponent<Rigidbody>();
            Debug.LogWarning($"LootSpawner: Prefab '{prefab.name}' was missing a Rigidbody. Added one dynamically.");
        }

        if (rb != null)
        {
            // Apply standardized "Pop" force
            // Upwards bias + Random spread
            Vector3 forceDir = (Vector3.up + Random.insideUnitSphere).normalized;
            rb.AddForce(forceDir * explosionForce);
            
            // Add random rotation tumble
            rb.AddTorque(Random.insideUnitSphere * explosionForce);
        }
    }
}
