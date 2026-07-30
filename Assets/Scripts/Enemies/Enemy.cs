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
    [SerializeField] private int _goldValue = 10;
    public int goldValue { get => _goldValue; set => _goldValue = value; }
    public enum ZombieType { Normal, Big, Boss }
    public ZombieType zombieType = ZombieType.Normal;
    public int maxHealth = 100;
    private int currentHealth = 100;
    
    [SerializeField] private ParticleSystem bloodEffect;
    [SerializeField] private float deathEffectDuration = 0.5f;


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

        // Zombi tipine göre hız ayarla
        float speed = zombieType == ZombieType.Big ? moveSpeed * 0.7f : moveSpeed;

        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        rb.linearVelocity = direction * speed;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
            float nextAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(nextAngle);
        }
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
        // 0: default araba, hicbir zombiyi instant olduremez.
        if (carLevel <= 0)
            return false;

        // 1-3: sadece normal(kucuk) zombiler.
        if (carLevel <= 3)
            return zombieType == ZombieType.Normal;

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
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
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
        gameObject.SetActive(false);
        if (pool != null)
        {
            pool.ReturnEnemy(this);
        }
    }
}
