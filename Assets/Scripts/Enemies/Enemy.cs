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
    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask blockerLayerMask = ~0;
    [SerializeField] private float obstacleCheckDistance = 1.1f;
    [SerializeField] private float sideCheckAngle = 35f;
    [SerializeField] private float stuckSpeedThreshold = 0.05f;
    [SerializeField] private float stuckTimeBeforeEscape = 0.5f;
    [SerializeField] private float escapeDuration = 0.35f;
    [SerializeField] private int _goldValue = 10;
    public int goldValue { get => _goldValue; set => _goldValue = value; }
    public enum ZombieType { Normal = 0, Big = 1, Boss = 2, Little = 3 }
    public ZombieType zombieType = ZombieType.Normal;
    public int maxHealth = 100;
    private int currentHealth = 100;
    
    [SerializeField] private ParticleSystem bloodEffect;
    [SerializeField] private float deathEffectDuration = 0.5f;


    private Rigidbody2D rb;
    private Collider2D[] cachedColliders;
    private bool isOnCarSide = false;
    private Transform carTarget = null;
    private float damageTimer = 0f;
    private bool isDead = false;
    private float stuckTimer = 0f;
    private float escapeTimer = 0f;
    private Vector2 escapeDirection = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cachedColliders = GetComponents<Collider2D>();
        rb.gravityScale = 0f; // Top-down oyun - aşağı düşmesin
    }

    private void SetCollisionState(bool enabled)
    {
        if (cachedColliders == null)
            return;

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] != null)
                cachedColliders[i].enabled = enabled;
        }
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        if (target == null)
            return;

        // Zombi tipine göre hız ayarla
        float speed = moveSpeed;
        if (zombieType == ZombieType.Big)
            speed = moveSpeed * 0.7f;
        else if (zombieType == ZombieType.Little)
            speed = moveSpeed * 1.2f;

        Vector2 targetOffset = (Vector2)target.position - rb.position;
        if (targetOffset.sqrMagnitude <= 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 desiredDirection = targetOffset.normalized;
        UpdateStuckAndEscape(desiredDirection);

        Vector2 direction = escapeTimer > 0f
            ? escapeDirection
            : GetSteeredDirection(desiredDirection);

        rb.linearVelocity = direction * speed;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
            float nextAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(nextAngle);
        }
    }

    private Vector2 GetSteeredDirection(Vector2 desiredDirection)
    {
        if (!IsDirectionBlocked(desiredDirection))
            return desiredDirection;

        Vector2 leftDirection = Rotate(desiredDirection, sideCheckAngle);
        Vector2 rightDirection = Rotate(desiredDirection, -sideCheckAngle);

        bool leftBlocked = IsDirectionBlocked(leftDirection);
        bool rightBlocked = IsDirectionBlocked(rightDirection);

        if (!leftBlocked && rightBlocked)
            return leftDirection;

        if (!rightBlocked && leftBlocked)
            return rightDirection;

        if (!leftBlocked && !rightBlocked)
        {
            Vector2 toTarget = ((Vector2)target.position - rb.position).normalized;
            float leftScore = Vector2.Dot(leftDirection, toTarget);
            float rightScore = Vector2.Dot(rightDirection, toTarget);
            return leftScore >= rightScore ? leftDirection : rightDirection;
        }

        return Random.value < 0.5f ? leftDirection : rightDirection;
    }

    private void UpdateStuckAndEscape(Vector2 desiredDirection)
    {
        if (escapeTimer > 0f)
        {
            escapeTimer -= Time.fixedDeltaTime;
            if (escapeTimer <= 0f)
            {
                escapeTimer = 0f;
                escapeDirection = Vector2.zero;
            }
            return;
        }

        bool isTryingToMove = desiredDirection.sqrMagnitude > 0.0001f;
        bool isAlmostStopped = rb.linearVelocity.sqrMagnitude < (stuckSpeedThreshold * stuckSpeedThreshold);

        if (isTryingToMove && isAlmostStopped)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeBeforeEscape)
            {
                StartEscape(desiredDirection);
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private void StartEscape(Vector2 desiredDirection)
    {
        stuckTimer = 0f;
        escapeTimer = escapeDuration;

        Vector2 left = Rotate(desiredDirection, 90f);
        Vector2 right = Rotate(desiredDirection, -90f);

        bool leftBlocked = IsDirectionBlocked(left);
        bool rightBlocked = IsDirectionBlocked(right);

        if (!leftBlocked && rightBlocked)
        {
            escapeDirection = left;
            return;
        }

        if (!rightBlocked && leftBlocked)
        {
            escapeDirection = right;
            return;
        }

        escapeDirection = Random.value < 0.5f ? left : right;
    }

    private bool IsDirectionBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, obstacleCheckDistance, blockerLayerMask);
        if (hit.collider == null)
            return false;

        if (IsOwnCollider(hit.collider))
            return false;

        if (hit.collider.isTrigger)
            return false;

        if (hit.collider.CompareTag("Car"))
            return false;

        if (target != null && (hit.collider.transform == target || hit.collider.transform.IsChildOf(target)))
            return false;

        return true;
    }

    private bool IsOwnCollider(Collider2D other)
    {
        if (cachedColliders == null)
            return false;

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] == other)
                return true;
        }

        return false;
    }

    private static Vector2 Rotate(Vector2 input, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            input.x * cos - input.y * sin,
            input.x * sin + input.y * cos
        ).normalized;
    }

    void Update()
    {
        // Arabanın collider'ında ise hasar ver
        if (isOnCarSide && carTarget != null)
        {
            var carHealth = carTarget.GetComponent<CarHealth>();
            if (carHealth == null)
                return;

            // Araba hızına göre hasar scale et
            Rigidbody2D carRb = carTarget.GetComponent<Rigidbody2D>();
            float speedMultiplier = 1.5f;
            
            if (carRb != null)
            {
                float currentSpeed = carRb.linearVelocity.magnitude;
                float referenceSpeed = 2f;
                speedMultiplier = (currentSpeed / referenceSpeed) * 1.5f;
                speedMultiplier = Mathf.Max(0.5f, speedMultiplier);
            }
            
            int baseDamage = carHealth.CarDamage;
            int finalDamage = (int)(baseDamage * speedMultiplier);
            
            // Araba tarafından alınan hasar (sabit) - SADECE INSTANT KILL EDEMEDIĞIMIZDE
            int carDamageAmount = 3; // Normal zombie
            if (zombieType == ZombieType.Big)
                carDamageAmount = 6;
            else if (zombieType == ZombieType.Boss)
                carDamageAmount = 9;
            else if (zombieType == ZombieType.Little)
                carDamageAmount = 2;

            // Instant kill kurallari araba seviyesine gore belirlenir.
            if (CanInstantKillZombie(carHealth.CarLevel))
            {
                TakeDamage(currentHealth);
                return;
            }
            
            // Diğer durumlar: Instant kill EDEMIYORUZ - 0.2 saniyede hasar al
            damageTimer += Time.deltaTime;
            
            if (damageTimer >= 0.2f)
            {
                TakeDamage(finalDamage);
                carHealth.TakeDamage(carDamageAmount);  // SADECE BURADA araba hasar alsın!
                damageTimer = 0f;
            }
        }
    }

    private bool CanInstantKillZombie(int carLevel)
    {
        // Little zombiler tüm araba seviyelerinde anında ölür.
        if (zombieType == ZombieType.Little)
            return true;

        // 0: default araba, hicbir zombiyi instant olduremez.
        if (carLevel <= 0)
            return false;

        // 1-3: sadece normal(kucuk) zombiler.
        if (carLevel <= 3)
            return zombieType == ZombieType.Normal || zombieType == ZombieType.Little;

        // 4-6: normal(kucuk) + big zombiler.
        if (carLevel <= 6)
            return zombieType != ZombieType.Boss;

        // 7-9: tum zombiler (normal, big, boss).
        return true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Car"))
        {
            isOnCarSide = true;
            carTarget = other.transform.parent ?? other.transform;
            damageTimer = 0f;
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

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {
            isOnCarSide = false;
            carTarget = null;
            damageTimer = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {
            isOnCarSide = true;
            carTarget = collision.transform.parent ?? collision.transform;
            damageTimer = 0f;
            return;
        }
        
        // Target ile çarpma - direkt öl
        if (collision.transform == target)
        {
            HandleKill();
            ReturnToPool();
            return;
        }
    }

    void HandleKill()
    {
        if (manager != null)
        {
            manager.EnemyDefeated();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        if (!isActiveAndEnabled)
        {
            ReturnToPool();
            return;
        }

        isDead = true;
        isOnCarSide = false;
        carTarget = null;
        damageTimer = 0f;
        target = null;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        SetCollisionState(false);

        // Sprite'ı kapat
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Kan efekti oynat
        if (bloodEffect != null)
        {
            bloodEffect.Play();
        }
        
        // Gold drop oluştur
        if (pool != null && goldDropPrefab != null)
        {
            GameObject goldDrop = Instantiate(goldDropPrefab, transform.position, Quaternion.identity);
            var goldDropScript = goldDrop.GetComponent<GoldDrop>();
            if (goldDropScript != null)
            {
                goldDropScript.SetGoldValue(goldValue);
            }
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterKill(zombieType);
        }

        if (manager != null)
        {
            manager.EnemyDefeated();
        }

        // Effect bitene kadar bekle, sonra pool'a dön
        StartCoroutine(DeathSequence());
    }

    private System.Collections.IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathEffectDuration);
        ReturnToPool();
    }

    void ReturnToPool()
    {
        // Sprite'ı geri aç
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        currentHealth = maxHealth;
        isDead = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        SetCollisionState(true);
        gameObject.SetActive(false);
        if (pool != null)
        {
            pool.ReturnEnemy(this);
        }
    }
}
