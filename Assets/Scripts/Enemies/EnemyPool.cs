using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject defaultZombiePrefab; // Varsayılan zombi prefab'ı
    
    public GameObject enemyBigZombiePrefab;
    public GameObject enemyBossZombiePrefab;
    public GameObject goldDropPrefab;
    public Transform enemyParent;
    public enum EnemyType { Normal = 0, Big = 1, Boss = 2, Little = 3 }
    public int initialPoolSize = 20;
    private readonly Dictionary<EnemyType, Queue<Enemy>> pools = new Dictionary<EnemyType, Queue<Enemy>>();
    private readonly Queue<Enemy> defaultZombiePool = new Queue<Enemy>();
    private readonly Dictionary<Enemy, Queue<Enemy>> enemyOriginPool = new Dictionary<Enemy, Queue<Enemy>>();

    void Awake()
    {
        if (enemyParent == null)
        {
            enemyParent = transform;
        }

        pools[EnemyType.Normal] = new Queue<Enemy>();
        pools[EnemyType.Big] = new Queue<Enemy>();
        pools[EnemyType.Boss] = new Queue<Enemy>();
        pools[EnemyType.Little] = new Queue<Enemy>();

        PrewarmPool();
    }

    void PrewarmPool()
    {
        int normalWarmSize = Mathf.Max(1, initialPoolSize / 4);

        // Default zombi pooluna başlangıç yükle
        if (defaultZombiePrefab != null)
        {
            for (int i = 0; i < normalWarmSize; i++)
            {
                CreatePooledEnemy(defaultZombiePrefab, EnemyType.Little, defaultZombiePool);
            }
        }

        // enemyPrefab normal pooluna başlangıç yükle
        if (enemyPrefab != null)
        {
            for (int i = 0; i < normalWarmSize; i++)
            {
                CreatePooledEnemy(enemyPrefab, EnemyType.Normal, pools[EnemyType.Normal]);
            }
        }
        else
        {
            Debug.LogWarning("EnemyPool: enemyPrefab is missing. Only defaultZombiePrefab can be spawned as normal zombie.");
        }

        // Big zombi pooluna başlangıç yükle
        if (enemyBigZombiePrefab != null)
        {
            for (int i = 0; i < normalWarmSize; i++)
            {
                CreatePooledEnemy(enemyBigZombiePrefab, EnemyType.Big);
            }
        }

        // Boss zombi pooluna başlangıç yükle
        if (enemyBossZombiePrefab != null)
        {
            for (int i = 0; i < normalWarmSize; i++)
            {
                CreatePooledEnemy(enemyBossZombiePrefab, EnemyType.Boss);
            }
        }
    }

    private Enemy CreatePooledEnemy()
    {
        return CreatePooledEnemy(enemyPrefab, EnemyType.Normal);
    }

    private Enemy CreatePooledEnemy(GameObject prefab, EnemyType type = EnemyType.Normal)
    {
        return CreatePooledEnemy(prefab, type, pools[type]);
    }

    private Enemy CreatePooledEnemy(GameObject prefab, EnemyType type, Queue<Enemy> targetPool)
    {
        if (prefab == null)
        {
            Debug.LogError("EnemyPool: prefab is null.");
            return null;
        }

        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity, enemyParent);
        Enemy enemyScript = instance.GetComponent<Enemy>();

        if (enemyScript == null)
        {
            Debug.LogError("EnemyPool: prefab does not contain an Enemy component.");
            Destroy(instance);
            return null;
        }

        // Zombi tipini ayarla
        enemyScript.zombieType = ConvertToZombieType(type);
        
        // Health değerini türe göre set et
        if (type == EnemyType.Boss)
            enemyScript.maxHealth = 250;
        else if (type == EnemyType.Big)
            enemyScript.maxHealth = 150;
        else if (type == EnemyType.Little)
            enemyScript.maxHealth = 50;
        else
            enemyScript.maxHealth = 100;
        
        // Gold değerini türe göre set et
        if (type == EnemyType.Boss)
            enemyScript.goldValue = 15;
        else if (type == EnemyType.Big)
            enemyScript.goldValue = 5;
        else if (type == EnemyType.Little)
            enemyScript.goldValue = 2;
        else
            enemyScript.goldValue = 1;
        
        enemyScript.pool = this;
        enemyScript.goldDropPrefab = goldDropPrefab;
        instance.SetActive(false);
        targetPool.Enqueue(enemyScript);
        enemyOriginPool[enemyScript] = targetPool;
        return enemyScript;
    }

    private Enemy TakeFromPoolOrCreate(Queue<Enemy> targetPool, GameObject prefab, EnemyType type)
    {
        if (targetPool.Count == 0)
        {
            Enemy created = CreatePooledEnemy(prefab, type, targetPool);
            if (created == null)
                return null;
        }

        return targetPool.Count > 0 ? targetPool.Dequeue() : null;
    }

    private void PrepareEnemyForSpawn(Enemy enemy, EnemyType type, Transform target, Vector3 position)
    {
        enemy.zombieType = ConvertToZombieType(type);

        // Health değerini türe göre set et
        if (type == EnemyType.Boss)
            enemy.maxHealth = 250;
        else if (type == EnemyType.Big)
            enemy.maxHealth = 150;
        else if (type == EnemyType.Little)
            enemy.maxHealth = 50;
        else
            enemy.maxHealth = 100;

        // Gold değerini türe göre set et
        if (type == EnemyType.Boss)
            enemy.goldValue = 15;
        else if (type == EnemyType.Big)
            enemy.goldValue = 5;
        else if (type == EnemyType.Little)
            enemy.goldValue = 2;
        else
            enemy.goldValue = 1;

        enemy.target = target;
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
    }

    public Enemy GetDefaultZombie(Transform target, Vector3 position)
    {
        GameObject prefabToUse = defaultZombiePrefab != null ? defaultZombiePrefab : enemyPrefab;
        Enemy enemy = TakeFromPoolOrCreate(defaultZombiePool, prefabToUse, EnemyType.Little);
        if (enemy == null)
            return null;

        PrepareEnemyForSpawn(enemy, EnemyType.Little, target, position);
        return enemy;
    }

    public Enemy GetEnemy(Transform target, Vector3 position)
    {
        return GetEnemy(EnemyType.Normal, target, position);
    }

    public Enemy GetEnemy(EnemyType type, Transform target, Vector3 position)
    {
        Queue<Enemy> targetPool = pools[type];

        // Type'a uygun prefab seç
        GameObject prefabToUse = enemyPrefab;
        if (type == EnemyType.Big && enemyBigZombiePrefab != null)
            prefabToUse = enemyBigZombiePrefab;
        else if (type == EnemyType.Boss && enemyBossZombiePrefab != null)
            prefabToUse = enemyBossZombiePrefab;
        else if (type == EnemyType.Normal && prefabToUse == null)
            prefabToUse = defaultZombiePrefab;
        else if (type == EnemyType.Little && prefabToUse == null)
            prefabToUse = defaultZombiePrefab;

        Enemy enemy = TakeFromPoolOrCreate(targetPool, prefabToUse, type);
        if (enemy == null)
            return null;

        PrepareEnemyForSpawn(enemy, type, target, position);
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        enemy.gameObject.SetActive(false);
        if (enemyOriginPool.TryGetValue(enemy, out Queue<Enemy> originPool))
        {
            originPool.Enqueue(enemy);
            return;
        }

        EnemyType poolType = ConvertToEnemyType(enemy.zombieType);
        pools[poolType].Enqueue(enemy);
    }

    private Enemy.ZombieType ConvertToZombieType(EnemyType type)
    {
        return type switch
        {
            EnemyType.Normal => Enemy.ZombieType.Normal,
            EnemyType.Big => Enemy.ZombieType.Big,
            EnemyType.Boss => Enemy.ZombieType.Boss,
            EnemyType.Little => Enemy.ZombieType.Little,
            _ => Enemy.ZombieType.Normal
        };
    }

    private EnemyType ConvertToEnemyType(Enemy.ZombieType type)
    {
        return type switch
        {
            Enemy.ZombieType.Normal => EnemyType.Normal,
            Enemy.ZombieType.Big => EnemyType.Big,
            Enemy.ZombieType.Boss => EnemyType.Boss,
            Enemy.ZombieType.Little => EnemyType.Little,
            _ => EnemyType.Normal
        };
    }
}
