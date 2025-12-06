using UnityEngine;
using System.Collections.Generic;






public class DesktopManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _emailPanel;
    [SerializeField] private GameObject _photoEditorPanel;
    [SerializeField] private GameObject _formPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _videoPanel;

    [Header("Taskbar Shortcuts (Buttons)")]
    [SerializeField] private MinigameButton _emailShortcut;
    [SerializeField] private MinigameButton _photoEditorShortcut;
    [SerializeField] private MinigameButton _formShortcut;
    [SerializeField] private MinigameButton _settingsShortcut;
    [SerializeField] private MinigameButton _videoShortcut;

    [Header("Close Buttons - Auto Setup")]
    [SerializeField] private MinigameButton _emailCloseButton;
    [SerializeField] private MinigameButton _photoEditorCloseButton;
    [SerializeField] private MinigameButton _formCloseButton;
    [SerializeField] private MinigameButton _settingsCloseButton;
    [SerializeField] private MinigameButton _videoCloseButton;

    [Header("Notification System")]
    [SerializeField] private GameObject _emailNotificationSprite; 
    [SerializeField] private float _notificationDisplayTime = 0.5f; 

    [Header("Video Player Integration")]
    [SerializeField] private UnityEngine.Video.VideoPlayer _videoPlayer;

    
    private Dictionary<MinigameButton, GameObject> _shortcutToPanelMap;
    
    
    private Dictionary<MinigameButton, GameObject> _closeButtonToPanelMap;

    
    private GameObject _currentActivePanel;

    
    private Dictionary<GameObject, PanelAnimator> _panelAnimators;

    
    private bool _isNotificationActive = false;

    public static DesktopManager Instance { get; private set; }

    private void Awake()
    {
        
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        
        InitializeShortcutMapping();
        InitializeCloseButtonMapping();
        InitializePanelAnimators();
    }

    private void Start()
    {
        
        SubscribeToShortcuts();
        SubscribeToCloseButtons();

        
        HideAllPanelsImmediate();

        
        if (_emailNotificationSprite != null)
            _emailNotificationSprite.SetActive(false);

        Debug.Log("Desktop Manager initialized with auto close button system");
    }

    
    
    
    private void InitializeShortcutMapping()
    {
        _shortcutToPanelMap = new Dictionary<MinigameButton, GameObject>
        {
            { _emailShortcut, _emailPanel },
            { _photoEditorShortcut, _photoEditorPanel },
            { _formShortcut, _formPanel },
            { _settingsShortcut, _settingsPanel },
            { _videoShortcut, _videoPanel }
        };
    }

    
    
    
    private void InitializeCloseButtonMapping()
    {
        _closeButtonToPanelMap = new Dictionary<MinigameButton, GameObject>();

        
        if (_emailCloseButton != null) 
            _closeButtonToPanelMap[_emailCloseButton] = _emailPanel;
        
        if (_photoEditorCloseButton != null) 
            _closeButtonToPanelMap[_photoEditorCloseButton] = _photoEditorPanel;
        
        if (_formCloseButton != null) 
            _closeButtonToPanelMap[_formCloseButton] = _formPanel;
        
        if (_settingsCloseButton != null) 
            _closeButtonToPanelMap[_settingsCloseButton] = _settingsPanel;
        
        if (_videoCloseButton != null) 
            _closeButtonToPanelMap[_videoCloseButton] = _videoPanel;

        Debug.Log($"Close buttons mapped: {_closeButtonToPanelMap.Count} buttons");
    }

    
    
    
    private void InitializePanelAnimators()
    {
        _panelAnimators = new Dictionary<GameObject, PanelAnimator>();

        GameObject[] allPanels = new GameObject[]
        {
            _emailPanel,
            _photoEditorPanel,
            _formPanel,
            _settingsPanel,
            _videoPanel
        };

        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
            {
                PanelAnimator animator = panel.GetComponent<PanelAnimator>();
                if (animator != null)
                {
                    _panelAnimators[panel] = animator;
                }
            }
        }
    }

    
    
    
    private void SubscribeToShortcuts()
    {
        if (_emailShortcut != null)
            _emailShortcut.OnClicked += () => OnShortcutClicked(_emailShortcut);
        
        if (_photoEditorShortcut != null)
            _photoEditorShortcut.OnClicked += () => OnShortcutClicked(_photoEditorShortcut);
        
        if (_formShortcut != null)
            _formShortcut.OnClicked += () => OnShortcutClicked(_formShortcut);
        
        if (_settingsShortcut != null)
            _settingsShortcut.OnClicked += () => OnShortcutClicked(_settingsShortcut);
        
        if (_videoShortcut != null)
            _videoShortcut.OnClicked += () => OnShortcutClicked(_videoShortcut);

        
        if (_emailShortcut != null)
            _emailShortcut.OnClicked += HideEmailNotification;
    }

    
    
    
    private void SubscribeToCloseButtons()
    {
        foreach (var kvp in _closeButtonToPanelMap)
        {
            MinigameButton closeButton = kvp.Key;
            GameObject panel = kvp.Value;

            
            closeButton.OnClicked += () => OnCloseButtonClicked(panel);
        }

        Debug.Log("Close buttons subscribed");
    }

    
    
    
    private void OnCloseButtonClicked(GameObject panelToClose)
    {
        if (panelToClose == null)
        {
            Debug.LogError("Panel to close is null!");
            return;
        }

        Debug.Log($"Close button clicked for: {panelToClose.name}");
        HidePanel(panelToClose);
    }

    
    
    
    private void OnShortcutClicked(MinigameButton shortcut)
    {
        if (shortcut == _emailShortcut && TutorialManager.Instance != null)
        {
        TutorialManager.Instance.OnEmailShortcutClicked();
        }
        
        if (_shortcutToPanelMap.TryGetValue(shortcut, out GameObject panel))
        {
            ShowPanel(panel);
        }
        else
        {
            Debug.LogError($"Panel not found for shortcut: {shortcut.name}");
        }
    }

    
    
    
    
    
    public void ShowPanel(GameObject panelToShow)
    {
        if (panelToShow == null)
        {
            Debug.LogError("Panel to show is null!");
            return;
        }

        
        if (_currentActivePanel == panelToShow && panelToShow.activeSelf)
        {
            Debug.Log($"Panel {panelToShow.name} is already active");
            return;
        }

        
        if (_currentActivePanel != null && _currentActivePanel != panelToShow)
        {
            HidePanel(_currentActivePanel);
        }

        
        _currentActivePanel = panelToShow;

        if (_panelAnimators.TryGetValue(panelToShow, out PanelAnimator animator))
        {
            animator.Show();
        }
        else
        {
            
            panelToShow.SetActive(true);
        }

        
        if (panelToShow == _emailPanel)
        {
            HideEmailNotification();
        }

        
        if (panelToShow == _videoPanel)
        {
            PlayVideo();
        }

        Debug.Log($"Showing panel: {panelToShow.name}");
    }

    
    
    
    
    public void HidePanel(GameObject panel)
    {
        if (panel == null) return;

        
        if (panel == _videoPanel)
        {
            StopVideo();
        }

        if (_panelAnimators.TryGetValue(panel, out PanelAnimator animator))
        {
            animator.Hide(() => OnPanelClosed(panel));
        }
        else
        {
            panel.SetActive(false);
            OnPanelClosed(panel);
        }
    }

    
    
    
    public void HideAllPanels()
    {
        GameObject[] allPanels = new GameObject[]
        {
            _emailPanel,
            _photoEditorPanel,
            _formPanel,
            _settingsPanel,
            _videoPanel
        };

        foreach (GameObject panel in allPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                HidePanel(panel);
            }
        }

        _currentActivePanel = null;

        Debug.Log("All panels hidden");
    }

    
    
    
    public void HideAllPanelsImmediate()
    {
        GameObject[] allPanels = new GameObject[]
        {
            _emailPanel,
            _photoEditorPanel,
            _formPanel,
            _settingsPanel,
            _videoPanel
        };

        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
            {
                if (_panelAnimators.TryGetValue(panel, out PanelAnimator animator))
                {
                    animator.HideImmediate();
                }
                else
                {
                    panel.SetActive(false);
                }
            }
        }

        _currentActivePanel = null;
    }

    
    
    
    public void OnPanelClosed(GameObject panel)
    {
        if (_currentActivePanel == panel)
        {
            _currentActivePanel = null;
        }

        Debug.Log($"Panel closed: {panel.name}");
    }

    
    
    
    public GameObject GetActivePanel()
    {
        return _currentActivePanel;
    }

    
    
    
    public bool IsPanelActive()
    {
        return _currentActivePanel != null && _currentActivePanel.activeSelf;
    }

    #region Video Player Control

    
    
    
    private void PlayVideo()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Play();
            Debug.Log("Video player started");
        }
        else
        {
            Debug.LogWarning("Video player is not assigned!");
        }
    }

    
    
    
    private void StopVideo()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();
            Debug.Log("Video player stopped");
        }
    }

    #endregion

    #region Notification System

    
    
    
    
    public void ShowEmailNotification()
    {
        if (_emailNotificationSprite == null)
        {
            Debug.LogWarning("Email notification sprite is not assigned!");
            return;
        }

        
        if (_isNotificationActive)
        {
            Debug.Log("Notification already active");
            return;
        }

        _isNotificationActive = true;

        
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayNotificationSFX();
            Debug.Log("Notification sound played");
        }

        
        if (_currentActivePanel == _emailPanel && _emailPanel.activeSelf)
        {
            
            StartCoroutine(ShowNotificationBriefly());
        }
        else
        {
            
            _emailNotificationSprite.SetActive(true);
            Debug.Log("Email notification shown - waiting for email panel to open");
        }
    }

    
    
    
    private System.Collections.IEnumerator ShowNotificationBriefly()
    {
        _emailNotificationSprite.SetActive(true);
        Debug.Log($"Email notification shown briefly for {_notificationDisplayTime} seconds");
        
        yield return new WaitForSeconds(_notificationDisplayTime);
        
        HideEmailNotification();
    }

    
    
    
    private void HideEmailNotification()
    {
        if (_emailNotificationSprite != null && _emailNotificationSprite.activeSelf)
        {
            _emailNotificationSprite.SetActive(false);
            _isNotificationActive = false;
            Debug.Log("Email notification hidden");
        }
    }

    #endregion

    #region Photo Editor Integration

    
    
    
    
    public void OpenPhotoEditorFromEmail()
    {
        ShowPanel(_photoEditorPanel);
        
        
        
        Debug.Log("Photo Editor opened from email - Form ready for new case");
    }

    #endregion

    #region Ending System

    
    
    
    
    public void ShowEnding(bool isGoodEnding)
    {
        
        HideAllPanels();

        
        
        if (isGoodEnding)
        {
            Debug.Log("=== SHOWING GOOD ENDING PANEL ===");
            
        }
        else
        {
            Debug.Log("=== SHOWING BAD ENDING PANEL ===");
            
        }
    }

    #endregion

    private void OnDestroy()
    {
        
        if (_emailShortcut != null)
            _emailShortcut.OnClicked -= () => OnShortcutClicked(_emailShortcut);
        
        if (_photoEditorShortcut != null)
            _photoEditorShortcut.OnClicked -= () => OnShortcutClicked(_photoEditorShortcut);
        
        if (_formShortcut != null)
            _formShortcut.OnClicked -= () => OnShortcutClicked(_formShortcut);
        
        if (_settingsShortcut != null)
            _settingsShortcut.OnClicked -= () => OnShortcutClicked(_settingsShortcut);
        
        if (_videoShortcut != null)
            _videoShortcut.OnClicked -= () => OnShortcutClicked(_videoShortcut);

        
        if (_emailShortcut != null)
            _emailShortcut.OnClicked -= HideEmailNotification;

        
        foreach (var kvp in _closeButtonToPanelMap)
        {
            MinigameButton closeButton = kvp.Key;
            GameObject panel = kvp.Value;
            
            if (closeButton != null)
            {
                closeButton.OnClicked -= () => OnCloseButtonClicked(panel);
            }
        }
    }
}