using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

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

        ConfigureSlider(masterSlider, audioManager.GetMasterVolumeLinear());
        ConfigureSlider(musicSlider, audioManager.GetMusicVolumeLinear());
        ConfigureSlider(sfxSlider, audioManager.GetSfxVolumeLinear());

        UnbindListeners();

        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(audioManager.SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(audioManager.SetSfxVolume);
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

        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(audioManager.SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(audioManager.SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(audioManager.SetSfxVolume);
        }

        listenersBound = false;
    }
}
