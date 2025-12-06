using UnityEngine;
using TMPro;
using System.Collections.Generic;





public class EmailManager : MonoBehaviour
{
    [Header("Email Panels")]
    [SerializeField] private GameObject _emailListPanel;
    [SerializeField] private GameObject _clientEmailPanel;
    [SerializeField] private GameObject _aicEmailPanel;

    [Header("Email List Buttons")]
    [SerializeField] private MinigameButton _inboxButton;
    [SerializeField] private MinigameButton _starredButton;
    [SerializeField] private MinigameButton _snoozedButton;
    [SerializeField] private MinigameButton _sentButton;
    [SerializeField] private MinigameButton _draftButton;
    [SerializeField] private MinigameButton _listButton;

    [Header("Email Item Buttons (in EmailListPanel)")]
    [SerializeField] private MinigameButton _aicEmailButton;
    [SerializeField] private MinigameButton _clientEmailButton;

    [Header("Block Buttons (in Email Detail Panels)")]
    [SerializeField] private MinigameButton _clientBlockButton;
    [SerializeField] private MinigameButton _aicBlockButton;

    [Header("Link File Button (in Client Email Panel)")]
    [SerializeField] private MinigameButton _linkFileButton;

    [Header("Client Email UI Elements")]
    [SerializeField] private TMP_Text _clientSenderText;
    [SerializeField] private TMP_Text _clientSubjectText;
    [SerializeField] private TMP_Text _clientBodyText;

    [Header("Case Data")]
    [SerializeField] private List<CaseDataSO> _allCases = new List<CaseDataSO>();

    
    private GameObject _currentActiveEmailPanel;
    private int _currentCaseIndex = 0;
    private CaseDataSO _currentCase;

    
    private bool _isClientBlocked = false;

    public static EmailManager Instance { get; private set; }

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
        
        SubscribeToButtons();

        
        LoadCase(_currentCaseIndex);

        
        ShowEmailListPanel();
    }

    
    
    
    private void SubscribeToButtons()
    {
        
        if (_inboxButton != null)
            _inboxButton.OnClicked += () => OnCategoryButtonClicked("Inbox");
        
        if (_starredButton != null)
            _starredButton.OnClicked += () => OnCategoryButtonClicked("Starred");
        
        if (_snoozedButton != null)
            _snoozedButton.OnClicked += () => OnCategoryButtonClicked("Snoozed");
        
        if (_sentButton != null)
            _sentButton.OnClicked += () => OnCategoryButtonClicked("Sent");
        
        if (_draftButton != null)
            _draftButton.OnClicked += () => OnCategoryButtonClicked("Draft");
        
        if (_listButton != null)
            _listButton.OnClicked += () => OnCategoryButtonClicked("List");

        
        if (_aicEmailButton != null)
            _aicEmailButton.OnClicked += OnAICEmailClicked;
        
        if (_clientEmailButton != null)
            _clientEmailButton.OnClicked += OnClientEmailClicked;

        
        if (_clientBlockButton != null)
            _clientBlockButton.OnClicked += OnClientBlockClicked;
        
        if (_aicBlockButton != null)
            _aicBlockButton.OnClicked += OnAICBlockClicked;

        
        if (_linkFileButton != null)
            _linkFileButton.OnClicked += OnLinkFileClicked;
    }

    #region Category Buttons (Inbox, Starred, etc.)

    private void OnCategoryButtonClicked(string category)
    {
        Debug.Log($"Category clicked: {category}");
        
        
        ShowEmailListPanel();

        
    }

    #endregion

    #region Email List Panel

    
    
    
    public void ShowEmailListPanel()
    {
        HideAllEmailPanels();
        
        if (_emailListPanel != null)
        {
            _emailListPanel.SetActive(true);
            _currentActiveEmailPanel = _emailListPanel;
        }

        Debug.Log("Showing Email List Panel");
    }

    #endregion

    #region Email Detail Panels

    
    
    
    private void OnAICEmailClicked()
    {
        HideAllEmailPanels();
        
        if (_aicEmailPanel != null)
        {
            _aicEmailPanel.SetActive(true);
            _currentActiveEmailPanel = _aicEmailPanel;
        }

        Debug.Log("Showing AIC Email Panel");
    }

    
    
    
    private void OnClientEmailClicked()
    {
        if (_isClientBlocked)
        {
            Debug.Log("Client email is blocked!");
            return;
        }

        HideAllEmailPanels();
        
        if (_clientEmailPanel != null)
        {
            _clientEmailPanel.SetActive(true);
            _currentActiveEmailPanel = _clientEmailPanel;
        }

        
        LoadClientEmail();

        Debug.Log("Showing Client Email Panel");
    }

    #endregion

    #region Block Functionality

    
    
    
    private void OnClientBlockClicked()
    {
        _isClientBlocked = true;
        
        if (_clientEmailButton != null)
            _clientEmailButton.gameObject.SetActive(false);

        
        if (FormManager.Instance != null)
            FormManager.Instance.SetEmailBlocked(true);

        Debug.Log("Client email blocked and hidden");

        
        ShowEmailListPanel();
    }

    
    
    
    private void OnAICBlockClicked()
    {
        Debug.LogWarning("Error Occurred: Archive Integrity Commission are Watching");
        
        
        
    }

    #endregion

    #region Link File Button

    
    
    
    private void OnLinkFileClicked()
    {
        Debug.Log("Link file clicked - Opening Photo Editor");

        if (TutorialManager.Instance != null)
    {
        TutorialManager.Instance.OnLinkFileClicked();
    }

        if (DesktopManager.Instance != null)
            DesktopManager.Instance.OpenPhotoEditorFromEmail();
    }

    #endregion

    #region Load Email Content

    
    
    
    private void LoadClientEmail()
    {
        if (_currentCase == null)
        {
            Debug.LogError("No case loaded!");
            return;
        }

        
        if (_clientSenderText != null)
            _clientSenderText.text = _currentCase.caseSender;

        
        if (_clientSubjectText != null)
            _clientSubjectText.text = _currentCase.caseSubject;

        
        if (_clientBodyText != null)
            _clientBodyText.text = _currentCase.caseBody;

        Debug.Log($"Loaded email from case {_currentCase.caseIndex}: {_currentCase.caseSender}");
    }

    #endregion

    #region Case Management

    
    
    
    public void LoadCase(int caseIndex)
    {
        if (caseIndex < 0 || caseIndex >= _allCases.Count)
        {
            Debug.LogError($"Invalid case index: {caseIndex}");
            return;
        }

        _currentCaseIndex = caseIndex;
        _currentCase = _allCases[caseIndex];

        Debug.Log($"Loaded case {caseIndex}: {_currentCase.caseSender}");

        
        _isClientBlocked = false;
        if (_clientEmailButton != null)
            _clientEmailButton.gameObject.SetActive(true);

        
        if (FormManager.Instance != null)
            FormManager.Instance.UpdateCaseData(_currentCase);

        
        if (FormManager.Instance != null)
            FormManager.Instance.SetEmailBlocked(false);
    }

    
    
    
    public void LoadNextCase()
    {
        int nextIndex = _currentCaseIndex + 1;
        
        if (nextIndex < _allCases.Count)
        {
            LoadCase(nextIndex);
        }
        else
        {
            Debug.Log("No more cases available");
        }
    }

    
    
    
    public CaseDataSO GetCurrentCase()
    {
        return _currentCase;
    }

    #endregion

    #region Helper Methods

    
    
    
    private void HideAllEmailPanels()
    {
        if (_emailListPanel != null)
            _emailListPanel.SetActive(false);
        
        if (_clientEmailPanel != null)
            _clientEmailPanel.SetActive(false);
        
        if (_aicEmailPanel != null)
            _aicEmailPanel.SetActive(false);

        _currentActiveEmailPanel = null;
    }

    
    
    
    public GameObject GetActiveEmailPanel()
    {
        return _currentActiveEmailPanel;
    }

    
    
    
    
    public void HideClientEmailAndShowList()
    {
        
        if (_clientEmailButton != null)
            _clientEmailButton.gameObject.SetActive(false);

        
        ShowEmailListPanel();

        Debug.Log("Client email hidden, email list shown");
    }

    #endregion

    private void OnDestroy()
    {
        
        if (_inboxButton != null)
            _inboxButton.OnClicked -= () => OnCategoryButtonClicked("Inbox");
        
        if (_starredButton != null)
            _starredButton.OnClicked -= () => OnCategoryButtonClicked("Starred");
        
        if (_snoozedButton != null)
            _snoozedButton.OnClicked -= () => OnCategoryButtonClicked("Snoozed");
        
        if (_sentButton != null)
            _sentButton.OnClicked -= () => OnCategoryButtonClicked("Sent");
        
        if (_draftButton != null)
            _draftButton.OnClicked -= () => OnCategoryButtonClicked("Draft");

        if (_listButton != null)
            _listButton.OnClicked -= () => OnCategoryButtonClicked("List");

        if (_aicEmailButton != null)
            _aicEmailButton.OnClicked -= OnAICEmailClicked;
        
        if (_clientEmailButton != null)
            _clientEmailButton.OnClicked -= OnClientEmailClicked;

        if (_clientBlockButton != null)
            _clientBlockButton.OnClicked -= OnClientBlockClicked;
        
        if (_aicBlockButton != null)
            _aicBlockButton.OnClicked -= OnAICBlockClicked;

        if (_linkFileButton != null)
            _linkFileButton.OnClicked -= OnLinkFileClicked;
    }
}