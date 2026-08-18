using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";

    [Header("PlayerPrefs Keys")]
    [SerializeField] private string musicPrefKey = "Audio_Music";

    [Header("Music Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float defaultMusicSourceVolume = 1f;
    [SerializeField] private float musicFadeDuration = 0.35f;

    [Header("Music Clips")]
    [SerializeField] private AudioClip preloadMusic;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip shopMusic;

    [Header("Scene Names")]
    [SerializeField] private string preloadSceneName = "PreLoad";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string optionsSceneName = "Options";
    [SerializeField] private string shopSceneName = "Shop";
    [SerializeField] private string carShopSceneName = "CarShop";

    private Coroutine fadeRoutine;

    private const float MinLinearVolume = 0.0001f;
    private bool usingMusicSourceFallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        
        // Make sure this is a root GameObject before calling DontDestroyOnLoad
        if (transform.parent != null)
        {
            transform.SetParent(null, false);
        }
        
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = defaultMusicSourceVolume;

        ApplySavedVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        UpdateMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene(scene.name);
    }

    public void SetMusicVolume(float linearValue)
    {
        float clamped = Mathf.Clamp01(linearValue);
        bool appliedToMixer = SetMixerVolume(musicVolumeParam, clamped);
        
        // Her durumda musicSource'un volume'ünü de ayarla (fallback veya direct)
        if (musicSource != null)
        {
            EnableMusicSourceFallback();
            musicSource.volume = clamped;
        }

        PlayerPrefs.SetFloat(musicPrefKey, clamped);
        PlayerPrefs.Save();
    }

    public float GetMusicVolumeLinear()
    {
        return PlayerPrefs.GetFloat(musicPrefKey, 1f);
    }

    public void PlayMusic(AudioClip clip, bool forceRestart = false)
    {
        if (musicSource == null)
            return;

        if (clip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        if (!forceRestart && musicSource.clip == clip && musicSource.isPlaying)
            return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeAndSwitchMusic(clip));
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        musicSource.Stop();
    }

    public void RefreshSceneMusic()
    {
        UpdateMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void ApplySavedVolumes()
    {
        float music = GetMusicVolumeLinear();

        SetMixerVolume(musicVolumeParam, music);
        
        // Her durumda musicSource'un volume'ünü de ayarla
        if (musicSource != null)
        {
            EnableMusicSourceFallback();
            musicSource.volume = music;
        }
    }

    private bool SetMixerVolume(string parameterName, float linearValue)
    {
        if (audioMixer == null || string.IsNullOrEmpty(parameterName))
            return false;

        try
        {
            float decibel = Mathf.Log10(Mathf.Max(linearValue, MinLinearVolume)) * 20f;
            audioMixer.SetFloat(parameterName, decibel);
            return true;
        }
        catch (UnityException)
        {
            // Exposed parameter does not exist
            return false;
        }
    }

    private void UpdateMusicForScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        if (SceneEquals(sceneName, preloadSceneName))
        {
            PlayMusic(preloadMusic != null ? preloadMusic : menuMusic);
            return;
        }

        if (SceneEquals(sceneName, mainMenuSceneName) || SceneEquals(sceneName, optionsSceneName))
        {
            PlayMusic(menuMusic);
            return;
        }

        if (SceneEquals(sceneName, shopSceneName) || SceneEquals(sceneName, carShopSceneName))
        {
            PlayMusic(shopMusic != null ? shopMusic : menuMusic);
            return;
        }

        PlayMusic(gameplayMusic != null ? gameplayMusic : menuMusic);
    }

    private bool SceneEquals(string a, string b)
    {
        return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerator FadeAndSwitchMusic(AudioClip nextClip)
    {
        float targetVolume = usingMusicSourceFallback
            ? Mathf.Clamp01(GetMusicVolumeLinear())
            : defaultMusicSourceVolume;

        if (musicSource.isPlaying && musicFadeDuration > 0f)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / musicFadeDuration);
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }
        }

        musicSource.Stop();
        musicSource.clip = nextClip;
        musicSource.Play();

        if (musicFadeDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / musicFadeDuration);
                musicSource.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }
        }

        musicSource.volume = targetVolume;
        fadeRoutine = null;
    }

    private void EnableMusicSourceFallback()
    {
        usingMusicSourceFallback = true;

        // Mixer parametreleri yoksa sesi dogrudan AudioSource uzerinden kontrol et.
        if (musicSource != null && musicSource.outputAudioMixerGroup != null)
        {
            musicSource.outputAudioMixerGroup = null;
        }
    }
}
