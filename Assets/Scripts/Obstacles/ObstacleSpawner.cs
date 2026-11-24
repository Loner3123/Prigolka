using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Префабы или спрайты препятствий")]
    [Tooltip("Можно назначить префабы...")]
    public GameObject[] obstaclePrefabs;

    [Tooltip("...или просто перетащить сюда спрайты препятствий (Шипы, Горгулья, Мышь)!")]
    public Sprite[] obstacleSprites;

    [Header("Интервалы появления (в секундах)")]
    public float minSpawnTime = 1.6f;
    public float maxSpawnTime = 3.0f;

    [Header("Позиция появления")]
    public float spawnX = 12f;
    public float groundSpawnY = -2.6f;
    public float minFlyingY = -1.6f;
    public float maxFlyingY = -0.6f;

    private float timer;
    private float nextSpawnTime;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnObstacle();
            timer = 0f;
            SetNextSpawnTime();
        }
    }

    void SpawnObstacle()
    {
        // 1. Если назначены готовые префабы в инспекторе
        if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
        {
            int index = Random.Range(0, obstaclePrefabs.Length);
            GameObject prefab = obstaclePrefabs[index];
            if (prefab != null)
            {
                bool isFlying = prefab.name.ToLower().Contains("fly") 
                             || prefab.name.ToLower().Contains("bat") 
                             || prefab.name.ToLower().Contains("gargoyle");

                float y = isFlying ? Random.Range(minFlyingY, maxFlyingY) : groundSpawnY;
                Instantiate(prefab, new Vector3(spawnX, y, 0f), Quaternion.identity);
                return;
            }
        }

        // 2. Если префабы не создавали, а просто закинули спрайты в массив obstacleSprites
        if (obstacleSprites != null && obstacleSprites.Length > 0)
        {
            int index = Random.Range(0, obstacleSprites.Length);
            Sprite sprite = obstacleSprites[index];
            if (sprite != null)
            {
                GameObject newObs = new GameObject("Obstacle_" + sprite.name);
                bool isFlying = sprite.name.ToLower().Contains("fly") 
                             || sprite.name.ToLower().Contains("bat") 
                             || sprite.name.ToLower().Contains("gargoyle");

                float y = isFlying ? Random.Range(minFlyingY, maxFlyingY) : groundSpawnY;
                newObs.transform.position = new Vector3(spawnX, y, 0f);

                SpriteRenderer sr = newObs.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                newObs.AddComponent<BoxCollider2D>();
                newObs.AddComponent<Obstacle>();
            }
        }
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }
}
