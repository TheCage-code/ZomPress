using System.Collections.Generic;
using UnityEngine;


public class MapGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform car;
    public Transform segmentParent;
    public GameObject[] segmentPrefabs;

    [Header("Path Variation")]
    public float pathVariationScale = 0.2f;
    public int pathVariationSeed = 12345;

    [Header("Blocker Settings")]
    public GameObject[] blockerPrefabs;
    public float blockerSpawnChance = 0.03f;
    public int maxBlockersPerChunk = 1;
    public float blockerMargin = 0.25f;
    public float blockerChunkSpacing = 2f;
    public float minBlockerDistance = 3f;

    [Header("Rare Pickup Settings")]
    public GameObject rarePickupPrefab;
    public GameObject[] rarePickupPrefabs;
    [Range(0f, 1f)] public float rarePickupSpawnChance = 0.02f;
    public float rarePickupChunkSpacing = 6f;
    public float rarePickupMinDistance = 10f;
    [Range(1, 10)] public int maxActiveRarePickups = 5;

    [Header("Spawn Settings")]
    public float segmentSize = 3f;
    public int initialRadius = 5;
    public int spawnRadius = 3;
    public int destroyRadius = 4;

    private readonly Dictionary<Vector2Int, GameObject> spawnedSegments = new Dictionary<Vector2Int, GameObject>();
    private readonly HashSet<Vector2Int> blockerChunks = new HashSet<Vector2Int>();
    private readonly List<Vector3> blockerPositions = new List<Vector3>();
    private readonly HashSet<Vector2Int> rarePickupChunks = new HashSet<Vector2Int>();
    private readonly List<GameObject> activeRarePickups = new List<GameObject>();

    void Start()
    {
        if (car == null)
        {
            Debug.LogError("MapGenerator: Car reference is missing.");
            enabled = false;
            return;
        }

        if (segmentPrefabs == null || segmentPrefabs.Length == 0)
        {
            Debug.LogError("MapGenerator: Segment prefab list is empty.");
            enabled = false;
            return;
        }

        if (segmentParent == null)
        {
            segmentParent = transform;
        }

        SpawnAroundCar(initialRadius);
    }

    void Update()
    {
        if (car == null)
            return;

        Vector2Int centerChunk = WorldToChunk(car.position);
        SpawnChunksAround(centerChunk, spawnRadius);
        RemoveFarChunks(centerChunk, destroyRadius);
    }

    void SpawnAroundCar(int radius)
    {
        Vector2Int centerChunk = WorldToChunk(car.position);
        SpawnChunksAround(centerChunk, radius);
    }

    void SpawnChunksAround(Vector2Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int chunkPos = new Vector2Int(center.x + x, center.y + y);
                if (!spawnedSegments.ContainsKey(chunkPos))
                {
                    SpawnChunk(chunkPos);
                }
            }
        }
    }

    void RemoveFarChunks(Vector2Int center, int radius)
    {
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in spawnedSegments)
        {
            Vector2Int chunkPos = kvp.Key;
            int dx = Mathf.Abs(chunkPos.x - center.x);
            int dy = Mathf.Abs(chunkPos.y - center.y);
            if (Mathf.Max(dx, dy) > radius)
            {
                toRemove.Add(chunkPos);
            }
        }

        foreach (var chunkPos in toRemove)
        {
            if (spawnedSegments.TryGetValue(chunkPos, out GameObject segment))
            {
                if (segment != null)
                {
                    Destroy(segment);
                }
                spawnedSegments.Remove(chunkPos);
                blockerChunks.Remove(chunkPos);
                    rarePickupChunks.Remove(chunkPos);
                blockerPositions.RemoveAll(pos => WorldToChunk(pos) == chunkPos);
            }
        }
    }

    void SpawnChunk(Vector2Int chunkPos)
    {
        GameObject prefab = ChooseSegmentPrefab(chunkPos);
        Vector3 worldPos = ChunkToWorld(chunkPos);
        GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity, segmentParent);
        spawnedSegments.Add(chunkPos, instance);
        PlaceBlockersInChunk(instance, worldPos);
        PlaceRarePickupInChunk(instance, worldPos);
    }

    GameObject ChooseSegmentPrefab(Vector2Int chunkPos)
    {
        if (segmentPrefabs == null || segmentPrefabs.Length == 0)
        {
            return null;
        }

        if (segmentPrefabs.Length == 1)
        {
            return segmentPrefabs[0];
        }

        float x = (chunkPos.x + pathVariationSeed) * pathVariationScale;
        float y = (chunkPos.y + pathVariationSeed) * pathVariationScale;
        float noise = Mathf.PerlinNoise(x, y);
        int index = Mathf.FloorToInt(noise * segmentPrefabs.Length);
        index = Mathf.Clamp(index, 0, segmentPrefabs.Length - 1);
        return segmentPrefabs[index];
    }

    void PlaceBlockersInChunk(GameObject chunk, Vector3 chunkWorldPos)
    {
        if (blockerPrefabs == null || blockerPrefabs.Length == 0)
            return;

        Vector2Int chunkCoord = WorldToChunk(chunkWorldPos);
        if (blockerChunkSpacing > 0)
        {
            for (int x = -Mathf.FloorToInt(blockerChunkSpacing); x <= Mathf.FloorToInt(blockerChunkSpacing); x++)
            {
                for (int y = -Mathf.FloorToInt(blockerChunkSpacing); y <= Mathf.FloorToInt(blockerChunkSpacing); y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    if (blockerChunks.Contains(new Vector2Int(chunkCoord.x + x, chunkCoord.y + y)))
                    {
                        return;
                    }
                }
            }
        }

        if (Random.value > blockerSpawnChance)
            return;

        float spawnRadius = segmentSize * 0.25f;
        Vector3 candidatePosition = chunkWorldPos + new Vector3(segmentSize * 0.5f, segmentSize * 0.5f, 0f);
        float xOffset = Random.Range(-spawnRadius, spawnRadius);
        float yOffset = Random.Range(-spawnRadius, spawnRadius);
        candidatePosition += new Vector3(xOffset, yOffset, 0f);

        if (IsPositionTooClose(candidatePosition))
            return;

        GameObject blockerPrefab = blockerPrefabs[Random.Range(0, blockerPrefabs.Length)];
        if (blockerPrefab == null)
            return;

        Instantiate(blockerPrefab, candidatePosition, Quaternion.identity, chunk.transform);
        blockerPositions.Add(candidatePosition);
        blockerChunks.Add(chunkCoord);
    }

    bool IsPositionTooClose(Vector3 position)
    {
        for (int i = 0; i < blockerPositions.Count; i++)
        {
            if (Vector3.Distance(blockerPositions[i], position) < minBlockerDistance)
                return true;
        }
        return false;
    }

    void PlaceRarePickupInChunk(GameObject chunk, Vector3 chunkWorldPos)
    {
        GameObject pickupPrefab = GetRarePickupPrefab();
        if (pickupPrefab == null)
            return;

        if (activeRarePickups.Count >= maxActiveRarePickups)
            return;

        Vector2Int chunkCoord = WorldToChunk(chunkWorldPos);
        if (rarePickupChunkSpacing > 0)
        {
            for (int x = -Mathf.FloorToInt(rarePickupChunkSpacing); x <= Mathf.FloorToInt(rarePickupChunkSpacing); x++)
            {
                for (int y = -Mathf.FloorToInt(rarePickupChunkSpacing); y <= Mathf.FloorToInt(rarePickupChunkSpacing); y++)
                {
                    if (rarePickupChunks.Contains(new Vector2Int(chunkCoord.x + x, chunkCoord.y + y)))
                    {
                        return;
                    }
                }
            }
        }

        if (Random.value > rarePickupSpawnChance)
            return;

        float spawnRadius = segmentSize * 0.25f;
        Vector3 candidatePosition = chunkWorldPos + new Vector3(segmentSize * 0.5f, segmentSize * 0.5f, 0f);
        float xOffset = Random.Range(-spawnRadius, spawnRadius);
        float yOffset = Random.Range(-spawnRadius, spawnRadius);
        candidatePosition += new Vector3(xOffset, yOffset, 0f);

        if (IsRarePickupTooClose(candidatePosition))
            return;

        GameObject pickupInstance = Instantiate(pickupPrefab, candidatePosition, Quaternion.identity, chunk.transform);
        activeRarePickups.Add(pickupInstance);
        rarePickupChunks.Add(chunkCoord);

        RareMagnetPickup magnetPickup = pickupInstance.GetComponent<RareMagnetPickup>();
        if (magnetPickup != null)
        {
            magnetPickup.BindOwner(this, chunkCoord);
            return;
        }

        RareHealPickup healPickup = pickupInstance.GetComponent<RareHealPickup>();
        if (healPickup != null)
        {
            healPickup.BindOwner(this, chunkCoord);
        }
    }

    GameObject GetRarePickupPrefab()
    {
        if (rarePickupPrefabs != null && rarePickupPrefabs.Length > 0)
        {
            return rarePickupPrefabs[Random.Range(0, rarePickupPrefabs.Length)];
        }

        return rarePickupPrefab;
    }

    bool IsRarePickupTooClose(Vector3 position)
    {
        if (rarePickupMinDistance <= 0f)
            return false;

        for (int i = 0; i < blockerPositions.Count; i++)
        {
            if (Vector3.Distance(blockerPositions[i], position) < rarePickupMinDistance)
                return true;
        }

        return false;
    }

    public void NotifyRarePickupCollected(GameObject pickup, Vector2Int chunkCoord)
    {
        if (activeRarePickups.Contains(pickup))
        {
            activeRarePickups.Remove(pickup);
        }

        rarePickupChunks.Remove(chunkCoord);
    }

    Vector2Int WorldToChunk(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / segmentSize);
        int y = Mathf.FloorToInt(worldPosition.y / segmentSize);
        return new Vector2Int(x, y);
    }

    Vector3 ChunkToWorld(Vector2Int chunkPos)
    {
        return new Vector3(chunkPos.x * segmentSize, chunkPos.y * segmentSize, 0f);
    }
}
