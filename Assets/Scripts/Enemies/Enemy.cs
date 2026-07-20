using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyPool pool;
    [HideInInspector] public Enemies manager;
    [HideInInspector] public GameObject goldDropPrefab;
    public Transform target;
    public float moveSpeed = 3f;
    public float rotationSpeed = 720f;
    public float rotationOffset = -90f;
    public int goldValue = 10;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (target == null)
            return;

        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
            float nextAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(nextAngle);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform == target || collision.collider.CompareTag("Car"))
        {
            HandleKill();
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == target || other.CompareTag("Car"))
        {
            HandleKill();
            ReturnToPool();
        }
    }

    void HandleKill()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(goldValue);
        }

        GameObject dropPrefab = goldDropPrefab;
        if (dropPrefab == null && pool != null)
        {
            dropPrefab = pool.goldDropPrefab;
        }

        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
    }

    public void ReturnToPool()
    {
        if (manager != null)
        {
            manager.ReturnEnemyToPool(this);
        }
        else if (pool != null)
        {
            pool.ReturnEnemy(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetState()
    {
        target = null;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}
