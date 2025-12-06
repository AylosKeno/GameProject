using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;






public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private MinigameButton _muteButton;
    [SerializeField] private Image _muteButtonImage;
    [SerializeField] private Sprite _volumeSprite;
    [SerializeField] private Sprite _muteSprite;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip _hoverSFX;
    [SerializeField] private AudioClip _clickSFX;
    [SerializeField] private AudioClip _notificationSFX;

    [Header("Audio Source")]
    [SerializeField] private AudioSource _sfxAudioSource;

    [Header("Resolution Settings")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;

    [Header("Framerate Settings (Optional)")]
    [SerializeField] private TMP_Dropdown _framerateDropdown;
    [SerializeField] private Toggle _vsyncToggle;

    
    private bool _isMuted = false;
    private float _volumeBeforeMute = 1f;

    
    private Resolution[] _resolutions;
    private List<Resolution> _filteredResolutions;

    
    private int[] _framerateOptions = { 30, 60, 120, 144, 240 };

    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        

        
        SetupAudioSource();
        SetupResolutions();
        SetupFramerate();
    }

    private void Start()
    {
        
        SubscribeToUIEvents();

        
        LoadSettings();
    }

    #region Audio System

    
    
    
    private void SetupAudioSource()
    {
        if (_sfxAudioSource == null)
        {
            _sfxAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource component auto-created");
        }

        _sfxAudioSource.playOnAwake = false;
        _sfxAudioSource.loop = false;

        if (_sfxMixerGroup != null)
        {
            _sfxAudioSource.outputAudioMixerGroup = _sfxMixerGroup;
            Debug.Log($"AudioSource assigned to mixer group: {_sfxMixerGroup.name}");
        }
        else
        {
            Debug.LogWarning("SFX Mixer Group is not assigned!");
        }

        Debug.Log("Audio system initialized");
    }

    
    
    
    private void SubscribeToUIEvents()
    {
        if (_volumeSlider != null)
            _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (_muteButton != null)
            _muteButton.OnClicked += OnMuteButtonClicked;

        if (_resolutionDropdown != null)
            _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        if (_fullscreenToggle != null)
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        if (_framerateDropdown != null)
            _framerateDropdown.onValueChanged.AddListener(OnFramerateChanged);

        if (_vsyncToggle != null)
            _vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
    }

    
    
    
    private void OnVolumeChanged(float value)
    {
        SetMasterVolume(value);
        
        
        if (value > 0 && _isMuted)
        {
            _isMuted = false;
            UpdateMuteButtonSprite();
        }
    }

    
    
    
    private void OnMuteButtonClicked()
    {
        ToggleMute();
    }

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (_audioMixer != null)
        {
            float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            _audioMixer.SetFloat("MasterVolume", db);
        }

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        Debug.Log($"Master volume set to: {volume}");
    }

    public void ToggleMute()
    {
        _isMuted = !_isMuted;

        if (_isMuted)
        {
            if (_volumeSlider != null)
                _volumeBeforeMute = _volumeSlider.value;

            if (_audioMixer != null)
                _audioMixer.SetFloat("MasterVolume", -80f);

            Debug.Log("Audio muted");
        }
        else
        {
            if (_volumeSlider != null)
                SetMasterVolume(_volumeSlider.value);
            else
                SetMasterVolume(_volumeBeforeMute);

            Debug.Log("Audio unmuted");
        }

        UpdateMuteButtonSprite();

        PlayerPrefs.SetInt("IsMuted", _isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateMuteButtonSprite()
    {
        if (_muteButtonImage == null) return;

        _muteButtonImage.sprite = _isMuted ? _muteSprite : _volumeSprite;
    }

    #endregion

    #region SFX Playback

    public void PlayHoverSFX()
    {
        if (_hoverSFX != null && !_isMuted)
        {
            _sfxAudioSource.PlayOneShot(_hoverSFX);
            Debug.Log("Hover SFX played");
        }
        else if (_hoverSFX == null)
        {
            Debug.LogWarning("Hover SFX clip is not assigned!");
        }
        else if (_isMuted)
        {
            Debug.Log("Audio is muted - hover sound skipped");
        }
    }

    public void PlayClickSFX()
    {
        if (_clickSFX != null && !_isMuted)
        {
            _sfxAudioSource.PlayOneShot(_clickSFX);
            Debug.Log("Click SFX played");
        }
        else if (_clickSFX == null)
        {
            Debug.LogWarning("Click SFX clip is not assigned!");
        }
        else if (_isMuted)
        {
            Debug.Log("Audio is muted - click sound skipped");
        }
    }

    public void PlayNotificationSFX()
    {
        if (_notificationSFX != null && !_isMuted)
        {
            _sfxAudioSource.PlayOneShot(_notificationSFX);
            Debug.Log("Notification SFX played");
        }
        else if (_notificationSFX == null)
        {
            Debug.LogWarning("Notification SFX clip is not assigned!");
        }
        else if (_isMuted)
        {
            Debug.Log("Audio is muted - notification sound skipped");
        }
    }

    public void PlayCustomSFX(AudioClip clip)
    {
        if (clip != null && !_isMuted)
        {
            _sfxAudioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Resolution System

    private void SetupResolutions()
    {
        if (_resolutionDropdown == null) return;

        _resolutions = Screen.resolutions;
        _filteredResolutions = new List<Resolution>();

        _resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int[] allowedWidths = { 1280, 1920, 2560, 3840 }; 
        
        double currentRefreshRate = Screen.currentResolution.refreshRateRatio.value;
        int currentResolutionIndex = 0;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            bool isAllowed = false;
            foreach (int width in allowedWidths)
            {
                if (_resolutions[i].width == width)
                {
                    isAllowed = true;
                    break;
                }
            }

            if (isAllowed && _resolutions[i].refreshRateRatio.value == currentRefreshRate)
            {
                _filteredResolutions.Add(_resolutions[i]);

                string option = $"{_resolutions[i].width} x {_resolutions[i].height}";
                options.Add(option);

                if (_resolutions[i].width == Screen.width && 
                    _resolutions[i].height == Screen.height)
                {
                    currentResolutionIndex = _filteredResolutions.Count - 1;
                }
            }
        }

        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResolutionIndex;
        _resolutionDropdown.RefreshShownValue();

        Debug.Log($"Resolution system initialized with {_filteredResolutions.Count} options");
    }

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= _filteredResolutions.Count) return;

        Resolution resolution = _filteredResolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);

        PlayerPrefs.SetInt("ResolutionWidth", resolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", resolution.height);
        PlayerPrefs.Save();

        Debug.Log($"Resolution changed to: {resolution.width} x {resolution.height}");
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"Fullscreen: {(isFullscreen ? "Enabled" : "Disabled")}");
    }

    #endregion

    #region Framerate System

    private void SetupFramerate()
    {
        if (_framerateDropdown == null) return;

        _framerateDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (int framerate in _framerateOptions)
        {
            options.Add($"{framerate} FPS");
        }

        options.Add("Unlimited");

        _framerateDropdown.AddOptions(options);

        _framerateDropdown.value = 1;
        _framerateDropdown.RefreshShownValue();

        Debug.Log("Framerate system initialized");
    }

    private void OnFramerateChanged(int index)
    {
        if (index < _framerateOptions.Length)
        {
            int targetFramerate = _framerateOptions[index];
            Application.targetFrameRate = targetFramerate;

            PlayerPrefs.SetInt("TargetFramerate", targetFramerate);
            Debug.Log($"Target framerate set to: {targetFramerate} FPS");
        }
        else
        {
            Application.targetFrameRate = -1;

            PlayerPrefs.SetInt("TargetFramerate", -1);
            Debug.Log("Target framerate set to: Unlimited");
        }

        PlayerPrefs.Save();
    }

    private void OnVSyncChanged(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"VSync: {(enabled ? "Enabled" : "Disabled")}");
    }

    #endregion

    #region Save/Load Settings

    private void LoadSettings()
    {
        
        if (_volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            _volumeSlider.value = savedVolume;
            SetMasterVolume(savedVolume);
        }

        _isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        
        if (_isMuted && _audioMixer != null)
        {
            _audioMixer.SetFloat("MasterVolume", -80f);
        }
        
        UpdateMuteButtonSprite();

        if (PlayerPrefs.HasKey("ResolutionWidth") && PlayerPrefs.HasKey("ResolutionHeight"))
        {
            int width = PlayerPrefs.GetInt("ResolutionWidth");
            int height = PlayerPrefs.GetInt("ResolutionHeight");
            
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            
            Screen.SetResolution(width, height, mode);
        }

        if (_fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            _fullscreenToggle.isOn = isFullscreen;
        }

        if (_framerateDropdown != null && PlayerPrefs.HasKey("TargetFramerate"))
        {
            int savedFramerate = PlayerPrefs.GetInt("TargetFramerate");
            Application.targetFrameRate = savedFramerate;
        }

        if (_vsyncToggle != null)
        {
            bool vsyncEnabled = PlayerPrefs.GetInt("VSync", 0) == 1;
            _vsyncToggle.isOn = vsyncEnabled;
            QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
        }

        Debug.Log($"Settings loaded - Volume: {(_volumeSlider != null ? _volumeSlider.value : 1f)}, Muted: {_isMuted}");
    }

    public void ResetToDefaults()
    {
        if (_volumeSlider != null)
            _volumeSlider.value = 1f;

        _isMuted = false;
        UpdateMuteButtonSprite();

        if (_resolutionDropdown != null && _filteredResolutions.Count > 0)
        {
            Resolution native = _filteredResolutions[_filteredResolutions.Count - 1];
            Screen.SetResolution(native.width, native.height, FullScreenMode.FullScreenWindow);
            _resolutionDropdown.value = _filteredResolutions.Count - 1;
        }

        if (_fullscreenToggle != null)
        {
            _fullscreenToggle.isOn = true;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }

        if (_framerateDropdown != null)
        {
            _framerateDropdown.value = 1;
            Application.targetFrameRate = 60;
        }

        if (_vsyncToggle != null)
        {
            _vsyncToggle.isOn = false;
            QualitySettings.vSyncCount = 0;
        }

        PlayerPrefs.DeleteKey("MasterVolume");
        PlayerPrefs.DeleteKey("IsMuted");
        PlayerPrefs.DeleteKey("ResolutionWidth");
        PlayerPrefs.DeleteKey("ResolutionHeight");
        PlayerPrefs.DeleteKey("Fullscreen");
        PlayerPrefs.DeleteKey("TargetFramerate");
        PlayerPrefs.DeleteKey("VSync");
        PlayerPrefs.Save();

        Debug.Log("Settings reset to defaults");
    }

    #endregion

    #region Public Getters

    public bool IsMuted() => _isMuted;
    public float GetVolume() => _volumeSlider != null ? _volumeSlider.value : 1f;

    #endregion

    private void OnDestroy()
    {
        if (_volumeSlider != null)
            _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (_muteButton != null)
            _muteButton.OnClicked -= OnMuteButtonClicked;

        if (_resolutionDropdown != null)
            _resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        if (_fullscreenToggle != null)
            _fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);

        if (_framerateDropdown != null)
            _framerateDropdown.onValueChanged.RemoveListener(OnFramerateChanged);

        if (_vsyncToggle != null)
            _vsyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
    }
}