using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float slowSpeed = 2f;
    public float recoveryRate = 1.5f;
    public float turnSpeed = 180f;
    public float extraSpeed = 0f;
    float currentSpeed;

    Rigidbody2D rb2D;
    SpriteRenderer spriteRenderer;
    float moveInput;
    float steerInput;
    bool isFlipped = false;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        currentSpeed = moveSpeed;
    }

    void FixedUpdate()
    {
        float targetSpeed = moveSpeed + extraSpeed;

        if (UpgradeManager.Instance != null)
        {
            targetSpeed += UpgradeManager.Instance.totalSpeedBonus;
        }

        // Geri giderken daha yavaş git
        float effectiveSpeed = currentSpeed;
        if (moveInput < 0) // Geri gidiyor
        {
            effectiveSpeed *= 0.65f; // Geri hızı ileri hızının %65'i
        }

        rb2D.linearVelocity = transform.up * moveInput * effectiveSpeed;
        
        // Tuşa basarken hızlan, bırakınca yavaşça dur
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, recoveryRate * Time.fixedDeltaTime);
        }
        else
        {
            // Hız sıfırlanıyor - durunca hız sıfırdan başlasın
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, recoveryRate * Time.fixedDeltaTime);
        }

        // Araba hareket ederken dönebilsin
        if (Mathf.Abs(moveInput) > 0.01f && Mathf.Abs(steerInput) > 0.01f)
        {
            float rotationAmount = -steerInput * turnSpeed * Time.fixedDeltaTime;
            rb2D.MoveRotation(rb2D.rotation + rotationAmount);
            
            // Sprite'ı yöne göre flip et
            if (steerInput < 0 && !isFlipped) // Sağa dönüyor
            {
                FlipSprite(true);
            }
            else if (steerInput > 0 && isFlipped) // Sola dönüyor
            {
                FlipSprite(false);
            }
        }
    }

    void Update()
    {
        // InputManager varsa joystick/WASD'yi birleştir
        if (InputManager.Instance != null)
        {
            Vector2 movementInput = InputManager.Instance.GetMovementInput();
            moveInput = movementInput.y;
            steerInput = movementInput.x;
        }
        else
        {
            // InputManager yoksa sadece WASD
            moveInput = Input.GetAxisRaw("Vertical");
            steerInput = Input.GetAxisRaw("Horizontal");
        }
    }

    void FlipSprite(bool flip)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flip;
            isFlipped = flip;
        }
    }

    public void HitTree()
    {
        currentSpeed = Mathf.Min(currentSpeed, slowSpeed);
    }
}
