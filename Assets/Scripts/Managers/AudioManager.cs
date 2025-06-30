using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource; // For sound effects
    [SerializeField] private AudioClip[] buildClips;
    [SerializeField] private AudioClip[] selectClips;

    [Header("Music Settings")]
    [SerializeField] private AudioSource musicSource; // For music
    [SerializeField] private AudioClip backgroundMusicClip;
    public int masterVolume = 100;
    public int musicVolume = 100;
    public int effectVolume = 100;

    private void Awake()
    {
        Instance = this;

        // Ensure audioSource for sound effects exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure musicSource exists and is configured for looping
        musicSource = transform.Find("MusicSource")?.GetComponent<AudioSource>();
        if (musicSource == null)
        {
            GameObject musicSourceGO = new GameObject("MusicSource");
            musicSourceGO.transform.SetParent(transform);
            musicSource = musicSourceGO.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // backgroundMusicClip will now be assigned directly in the Inspector
        // No Resources.Load needed here anymore.
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SettingsPanelUI.Instance.OnSettingChanged += OnSettingChanged;
    }

    public void OnSettingChanged(string id, object obj)
    {
        
        if (obj is not int && obj is not float)
            return;
        
        int absolute = obj is int ? (int)obj : Mathf.RoundToInt((float)obj);
        switch (id)
        {
            case SettingsPanelUI.SETTING_ITEM_01:
                masterVolume = absolute;
                break;
            case SettingsPanelUI.SETTING_ITEM_02:
                musicVolume = absolute;
                break;
            case SettingsPanelUI.SETTING_ITEM_03:
                effectVolume = absolute;
                break;
        }
        CalculSoundsVolume();
    }

    public void CalculSoundsVolume()
    {
        float miniMaster = masterVolume / 100f;
        float miniMusic = musicVolume / 100f;
        float miniEffect = effectVolume / 100f;
        float music = miniMaster * miniMusic;
        float effect = miniMaster * miniEffect;
        
        musicSource.volume = music;
        audioSource.volume = effect;
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 2) // Assuming scene 2 is your main game scene
        {
            PlayBackgroundMusic();
        }
        else
        {
            // Stop music for other scenes
            StopMusic();
        }
    }

    private void Update()
    {
        // Mute music (AZERTY: ';')
        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            ToggleMute(true);
        }

        // Unmute music (AZERTY: 'P')
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleMute(false);
        }
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusicClip != null)
        {
            if (musicSource.clip != backgroundMusicClip || !musicSource.isPlaying)
            {
                musicSource.clip = backgroundMusicClip;
                musicSource.Play();
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void ToggleMute(bool mute)
    {
        if (musicSource != null)
        {
            musicSource.mute = mute;
        }
    }

    // Existing methods for sound effects
    public void PlayBuildClip()
    {
        audioSource.PlayOneShot(buildClips[Random.Range(0, buildClips.Length)]);
    }

    public void PlaySelectClip()
    {
        audioSource.PlayOneShot(selectClips[Random.Range(0, selectClips.Length)]);
    }
}
