using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    public AudioClip levelCompleteSound;
    public AudioClip errorSound;
    public AudioClip uiClickSound;

    public AudioClip boxMatchSound;
    public AudioClip portalRejectSound;
    public AudioClip portalTeleportSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            if (musicSource == null && sfxSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            else if (musicSource != null && sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            else if (musicSource == null && sfxSource != null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            if (musicSource == null) musicSource = sources[0];
            if (sfxSource == null) sfxSource = sources[1];
        }

        ApplySettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AttachUISounds();
    }

    public void AttachUISounds()
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in buttons)
        {
            if (btn.gameObject.scene.isLoaded)
            {
                btn.onClick.RemoveListener(PlayUIClick);
                btn.onClick.AddListener(PlayUIClick);
            }
        }
    }

    private void PlayUIClick()
    {
        PlayCoreSFX(uiClickSound);
    }

    public void ApplySettings()
    {
        if (musicSource != null) musicSource.mute = !SettingsManager.IsMusicOn;
        if (sfxSource != null) sfxSource.mute = !SettingsManager.IsSFXOn;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null || musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null && SettingsManager.IsSFXOn)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayCoreSFX(AudioClip specificClip)
    {
        PlaySFX(specificClip);
    }
}