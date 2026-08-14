using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SelectedCarImageUI : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite fallbackSprite;
    [SerializeField] private bool useNativeSize;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void Start()
    {
        // Start is kept as a second refresh point to cover scene load order edge cases.
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (CarSelectionManager.Instance != null)
        {
            CarSelectionManager.Instance.SelectedCarChanged += OnSelectedCarChanged;
        }
    }

    private void Unsubscribe()
    {
        if (CarSelectionManager.Instance != null)
        {
            CarSelectionManager.Instance.SelectedCarChanged -= OnSelectedCarChanged;
        }
    }

    private void Refresh()
    {
        if (targetImage == null)
            return;

        if (CarSelectionManager.Instance != null &&
            CarSelectionManager.Instance.TryGetSelectedCar(out _, out Sprite selectedSprite))
        {
            ApplySprite(selectedSprite);
            return;
        }

        if (fallbackSprite != null)
        {
            ApplySprite(fallbackSprite);
        }
    }

    private void OnSelectedCarChanged(Sprite selectedSprite, string selectedCarId)
    {
        if (selectedSprite == null)
            return;

        ApplySprite(selectedSprite);
    }

    private void ApplySprite(Sprite sprite)
    {
        targetImage.sprite = sprite;

        if (useNativeSize)
        {
            targetImage.SetNativeSize();
        }
    }
}
