using System.Collections;
using UnityEngine;

/// <summary>
/// Tutorial Manager - Handles the opening tutorial sequence for Case 0
/// Flow:
/// 1. Start with only Video panel visible (5 seconds)
/// 2. Show Email notification + shortcut (5 seconds wait)
/// 3. If not clicked, force open Email panel
/// 4. Wait for linkFile click, then show PhotoEditor & Form shortcuts
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private float _videoWatchDuration = 5f;
    [SerializeField] private float _emailNotificationWaitTime = 5f;

    [Header("Shortcuts to Show/Hide")]
    [SerializeField] private GameObject _emailShortcut;
    [SerializeField] private GameObject _photoEditorShortcut;
    [SerializeField] private GameObject _formShortcut;
    [SerializeField] private GameObject _videoShortcut;
    [SerializeField] private GameObject _settingsShortcut;
    [SerializeField] private GameObject _desktopButton;

    [Header("Panels")]
    [SerializeField] private GameObject _emailPanel;
    [SerializeField] private GameObject _videoPanel;

    private bool _tutorialCompleted = false;
    private bool _emailClicked = false;
    private bool _linkFileClicked = false;

    public static TutorialManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Check if this is the first case (tutorial)
        if (FormManager.Instance != null && FormManager.Instance.GetCurrentCaseIndex() == 0)
        {
            StartCoroutine(TutorialSequence());
        }
        else
        {
            // Not tutorial, enable everything normally
            EnableAllShortcuts();
            _tutorialCompleted = true;
        }
    }

    /// <summary>
    /// Main tutorial sequence coroutine
    /// </summary>
    private IEnumerator TutorialSequence()
    {
        Debug.Log("=== TUTORIAL STARTED ===");

        // STEP 1: Hide everything except Video panel & shortcut
        HideAllShortcuts();
        ShowOnlyVideoShortcut();

        if (DesktopManager.Instance != null)
        {
            DesktopManager.Instance.HideAllPanelsImmediate();
            DesktopManager.Instance.ShowPanel(_videoPanel);
        }

        Debug.Log("Step 1: Video playing for 5 seconds...");
        yield return new WaitForSeconds(_videoWatchDuration);

        // STEP 2: Show Email shortcut + notification
        ShowEmailShortcut();
        
        if (DesktopManager.Instance != null)
        {
            DesktopManager.Instance.ShowEmailNotification();
        }

        Debug.Log("Step 2: Email notification shown, waiting for click...");

        // Wait for email click OR timeout
        float waitTime = 0f;
        _emailClicked = false;

        while (waitTime < _emailNotificationWaitTime && !_emailClicked)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }

        // STEP 3: Force open email if not clicked
        if (!_emailClicked)
        {
            Debug.Log("Step 3: Email not clicked, forcing open...");
            
            if (DesktopManager.Instance != null)
            {
                DesktopManager.Instance.ShowPanel(_emailPanel);
            }
        }
        else
        {
            Debug.Log("Step 3: Email clicked by user");
        }

        // STEP 4: Wait for linkFile button click
        Debug.Log("Step 4: Waiting for linkFile button click...");
        
        while (!_linkFileClicked)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // STEP 5: Show PhotoEditor & Form shortcuts
        ShowPhotoEditorAndFormShortcuts();
        
        Debug.Log("Step 5: PhotoEditor & Form shortcuts shown");
        Debug.Log("=== TUTORIAL COMPLETED ===");

        _tutorialCompleted = true;
    }

    #region Shortcut Visibility Control

    /// <summary>
    /// Hide all shortcuts
    /// </summary>
    private void HideAllShortcuts()
    {
        if (_emailShortcut != null)
            _emailShortcut.SetActive(false);
        
        if (_photoEditorShortcut != null)
            _photoEditorShortcut.SetActive(false);
        
        if (_formShortcut != null)
            _formShortcut.SetActive(false);
        
        if (_videoShortcut != null)
            _videoShortcut.SetActive(false);
        
        if (_settingsShortcut != null)
            _settingsShortcut.SetActive(true);
        
        if (_desktopButton != null)
            _desktopButton.SetActive(true);

        Debug.Log("All shortcuts hidden");
    }

    /// <summary>
    /// Show only Video shortcut
    /// </summary>
    private void ShowOnlyVideoShortcut()
    {
        if (_videoShortcut != null)
            _videoShortcut.SetActive(true);

        Debug.Log("Video shortcut shown");
    }

    /// <summary>
    /// Show Email shortcut
    /// </summary>
    private void ShowEmailShortcut()
    {
        if (_emailShortcut != null)
            _emailShortcut.SetActive(true);

        Debug.Log("Email shortcut shown");
    }

    /// <summary>
    /// Show PhotoEditor and Form shortcuts
    /// </summary>
    private void ShowPhotoEditorAndFormShortcuts()
    {
        if (_photoEditorShortcut != null)
            _photoEditorShortcut.SetActive(true);
        
        if (_formShortcut != null)
            _formShortcut.SetActive(true);

        // Also show desktop button for navigation
        if (_desktopButton != null)
            _desktopButton.SetActive(true);

        Debug.Log("PhotoEditor & Form shortcuts shown");
    }

    /// <summary>
    /// Enable all shortcuts (for non-tutorial cases)
    /// </summary>
    private void EnableAllShortcuts()
    {
        if (_emailShortcut != null)
            _emailShortcut.SetActive(true);
        
        if (_photoEditorShortcut != null)
            _photoEditorShortcut.SetActive(true);
        
        if (_formShortcut != null)
            _formShortcut.SetActive(true);
        
        if (_videoShortcut != null)
            _videoShortcut.SetActive(true);
        
        if (_settingsShortcut != null)
            _settingsShortcut.SetActive(true);
        
        if (_desktopButton != null)
            _desktopButton.SetActive(true);

        Debug.Log("All shortcuts enabled");
    }

    #endregion

    #region Event Callbacks (Called from other managers)

    /// <summary>
    /// Called when email shortcut is clicked
    /// </summary>
    public void OnEmailShortcutClicked()
    {
        if (!_tutorialCompleted)
        {
            _emailClicked = true;
            Debug.Log("Tutorial: Email shortcut clicked");
        }
    }

    /// <summary>
    /// Called when linkFile button is clicked
    /// </summary>
    public void OnLinkFileClicked()
    {
        if (!_tutorialCompleted)
        {
            _linkFileClicked = true;
            Debug.Log("Tutorial: LinkFile clicked");
        }
    }

    /// <summary>
    /// Check if tutorial is completed
    /// </summary>
    public bool IsTutorialCompleted()
    {
        return _tutorialCompleted;
    }

    #endregion
}