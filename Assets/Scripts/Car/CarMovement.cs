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

        float effectiveSpeed = currentSpeed;
        
        // Joystick X, Y yönünü direkt kullan
        Vector2 moveDirection = new Vector2(steerInput, moveInput);
        float magnitude = moveDirection.magnitude;
        
        if (magnitude > 0.01f)
        {
            moveDirection = moveDirection.normalized;
            
            // Joystick yönüne göre araba dön
            float angle = Mathf.Atan2(moveInput, steerInput) * Mathf.Rad2Deg - 90f;
            rb2D.rotation = angle;
        }

        rb2D.linearVelocity = moveDirection * magnitude * effectiveSpeed;
        
        // Tuşa basarken hızlan
        if (Mathf.Abs(magnitude) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, recoveryRate * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, recoveryRate * Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        // InputManager varsa joystick/WASD'yi birleştir
        if (InputManager.Instance != null)
        {
            Vector2 movementInput = InputManager.Instance.GetMovementInput();
            
            // steerInput = X (sağ/sol), moveInput = Y (yukarı/aşağı)
            steerInput = movementInput.x;
            moveInput = movementInput.y;
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
