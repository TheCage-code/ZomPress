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
    private bool isOnCarSide = false;
    private Transform carTarget = null;
    private float damageTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Top-down oyun - aşağı düşmesin
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

    void Update()
    {
        // Arabanın yanında ise zamanla hasar ver
        if (isOnCarSide && carTarget != null)
        {
            damageTimer += Time.deltaTime;
            
            if (damageTimer >= 1f)
            {
                var carHealth = carTarget.GetComponent<CarHealth>();
                if (carHealth != null)
                {
                    carHealth.TakeDamage(1f);
                }
                damageTimer = 0f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Car"))
        {
            // Hangi collider tarafına çarptığını kontrol et
            string collidedPart = other.gameObject.name;
            
            // Yan taraf check FIRST - daha spesifik
            if (collidedPart.Contains("Left") || collidedPart.Contains("Right"))
            {
                // Yan taraf - hasar veriyor
                isOnCarSide = true;
                carTarget = other.transform.parent; // Ana araba GameObject'i
                damageTimer = 0f;
            }
            else if (collidedPart.Contains("Front") || collidedPart.Contains("Back"))
            {
                // Ön veya arka - ölüyor
                HandleKill();
                ReturnToPool();
            }
        }
        else if (other.transform == target)
        {
            HandleKill();
            ReturnToPool();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Car"))
        {
            isOnCarSide = false;
            carTarget = null;
            damageTimer = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform == target)
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
        isOnCarSide = false;
        carTarget = null;
        damageTimer = 0f;
    }
}
