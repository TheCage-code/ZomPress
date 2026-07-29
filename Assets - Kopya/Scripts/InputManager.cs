using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Mobile Joystick")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    public float joystickRadius = 50f;

    private Vector2 joystickInput = Vector2.zero;
    private bool isJoystickActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        // Mobil dokunma veya PC mouse girdilerini işle
        HandleJoystickInput();
    }

    void HandleJoystickInput()
    {
        if (joystickBackground == null) return;

        bool isTouching = false;
        Vector2 inputPosition = Vector2.zero;

        // 1. Önce Mobil Dokunmayı Kontrol Et
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = touch.position;

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                ResetJoystick();
                return;
            }
            isTouching = true;
        }
        // 2. Mobil dokunma yoksa PC Mouse Sol Tıkını Kontrol Et
        else if (Input.GetMouseButton(0))
        {
            inputPosition = Input.mousePosition;
            isTouching = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ResetJoystick();
            return;
        }

        // Eğer bir dokunma/tıklama varsa hesapla
        if (isTouching)
        {
            Vector2 joystickCenter = joystickBackground.position;
            float distanceToTouch = Vector2.Distance(inputPosition, joystickCenter);

            // Joystick alanına basıldıysa veya basılı tutulup sürükleniyorsa aktif et
            if (isJoystickActive || distanceToTouch <= joystickRadius + 50f)
            {
                isJoystickActive = true;

                Vector2 touchDelta = inputPosition - joystickCenter;

                // Maksimum yarıçapı geçmesin
                if (touchDelta.magnitude > joystickRadius)
                {
                    touchDelta = touchDelta.normalized * joystickRadius;
                }

                // Joystick topuzunu hareket ettir
                if (joystickHandle != null)
                {
                    joystickHandle.position = joystickCenter + touchDelta;
                }

                // Input'u -1 ile 1 arasında normalize et
                joystickInput = touchDelta / joystickRadius;
            }
        }
        else
        {
            ResetJoystick();
        }
    }

    void ResetJoystick()
    {
        isJoystickActive = false;
        joystickInput = Vector2.zero;

        if (joystickHandle != null && joystickBackground != null)
        {
            joystickHandle.position = joystickBackground.position;
        }
    }

    public Vector2 GetMovementInput()
    {
        // WASD / Ok Tuşları
        Vector2 desktopInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Hangisi daha büyük değer veriyorsa (aktifse) onu döndür
        float finalX = Mathf.Abs(joystickInput.x) > Mathf.Abs(desktopInput.x) ? joystickInput.x : desktopInput.x;
        float finalY = Mathf.Abs(joystickInput.y) > Mathf.Abs(desktopInput.y) ? joystickInput.y : desktopInput.y;

        return new Vector2(finalX, finalY);
    }

    public float GetAccelerateInput()
    {
        // Mobil: Joystick Y pozitifse veya Klavye 'W'
        return GetMovementInput().y > 0 ? GetMovementInput().y : 0f;
    }

    public float GetBrakeInput()
    {
        // Mobil: Joystick Y negatifse veya Klavye 'S'
        return GetMovementInput().y < 0 ? Mathf.Abs(GetMovementInput().y) : 0f;
    }

    public Vector2 GetSteerInput()
    {
        return new Vector2(GetMovementInput().x, 0f);
    }
}