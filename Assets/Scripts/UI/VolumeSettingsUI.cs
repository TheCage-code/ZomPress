using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;

    private bool listenersBound;

    private void Start()
    {
        InitializeSliders();
    }

    private void OnDestroy()
    {
        UnbindListeners();
    }

    private void InitializeSliders()
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogWarning("VolumeSettingsUI: AudioManager instance not found.");
            return;
        }

        ConfigureSlider(musicSlider, audioManager.GetMusicVolumeLinear());

        UnbindListeners();

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
        }

        listenersBound = true;
    }

    private void ConfigureSlider(Slider slider, float value)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    private void UnbindListeners()
    {
        if (!listenersBound)
            return;

        AudioManager audioManager = AudioManager.Instance;
        if (audioManager == null)
            return;

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(audioManager.SetMusicVolume);
        }

        listenersBound = false;
    }
}
