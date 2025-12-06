using System.Collections;
using UnityEngine;
using TMPro;





public class FormManager : MonoBehaviour
{
    [Header("Input Fields - Drag from Inspector")]
    [SerializeField] private TMP_InputField _brightnessInputField;
    [SerializeField] private TMP_InputField _contrastInputField;
    [SerializeField] private TMP_Dropdown _anomalyTypeDropdown;

    [Header("Submit Button")]
    [SerializeField] private MinigameButton _submitButton;

    [Header("Submit Settings")]
    [SerializeField] private float _submitCooldown = 1f; 

    [Header("Validation Settings")]
    [SerializeField] private float _adjustmentTolerance = 5f;

    [Header("Game Settings")]
    [SerializeField] private int _pointsForCorrectAnomaly = 8;
    [SerializeField] private int _pointsForCorrectAdjustment = 7;
    [SerializeField] private int _pointsForEmailBlocked = 5;
    [SerializeField] private int _goodEndingThreshold = 138; 

    
    private CaseDataSO _currentCaseData;

    
    private int _currentCaseIndex = 0;
    private int _nightCurrentScore = 0;

    
    private bool _guessAnomaly = false;
    private bool _adjustAnomaly = false;
    private bool _emailBlocked = false;

    
    private bool _canSubmit = true;

    public static FormManager Instance { get; private set; }

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
        if (_submitButton != null)
            _submitButton.OnClicked += OnSubmitClicked;
            
        _currentCaseIndex = 0;
        _nightCurrentScore = 0;
        SetupDropdown();
        ResetForm();

        if (EmailManager.Instance != null)
        {
        _currentCaseData = EmailManager.Instance.GetCurrentCase();
        
        if (_currentCaseData != null)
            {
            Debug.Log($"FormManager: Initial case loaded - Case {_currentCaseData.caseIndex}");
            }
        else
            {
            Debug.LogWarning("FormManager: EmailManager has no case loaded yet");
            }
        }
        else
        {
        Debug.LogError("FormManager: EmailManager instance not found at start!");
        }
    }

    
    
    
    private void SetupDropdown()
    {
        if (_anomalyTypeDropdown == null) return;

        _anomalyTypeDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        
        foreach (CaseDataSO.AnomalyType type in System.Enum.GetValues(typeof(CaseDataSO.AnomalyType)))
        {
            options.Add(type.ToString());
        }
        
        _anomalyTypeDropdown.AddOptions(options);
    }

    
    
    
    public void UpdateCaseData(CaseDataSO caseData)
    {
        _currentCaseData = caseData;
        Debug.Log($"FormManager: Case data updated to index {caseData.caseIndex}");
    }

    
    
    
    public void SetEmailBlocked(bool blocked)
    {
        _emailBlocked = blocked;
        Debug.Log($"FormManager: Email blocked status set to {blocked}");
    }

    public int GetCurrentCaseIndex()
    {
        return _currentCaseIndex;
    }

    public int GetCurrentScore()
    {
        return _nightCurrentScore;
    }

    #region Submit & Validation

    private void OnSubmitClicked()
    {
        
        if (!_canSubmit)
        {
            Debug.LogWarning("Submit on cooldown!");
            return;
        }

        if (_currentCaseData == null)
        {
            Debug.LogError("No case data loaded in FormManager!");
            return;
        }

        
        _canSubmit = false;
        StartCoroutine(SubmitCooldownRoutine());

        
        _adjustAnomaly = ValidateAdjustments(_currentCaseData);
        
        
        _guessAnomaly = ValidateAnomalyGuess(_currentCaseData);

        
        int caseScore = CalculateCaseScore();
        _nightCurrentScore += caseScore;

        
        Debug.Log($"=== CASE {_currentCaseIndex} SUBMITTED ===");
        Debug.Log($"Guess Anomaly: {_guessAnomaly} (+{(_guessAnomaly ? _pointsForCorrectAnomaly : 0)})");
        Debug.Log($"Adjust Anomaly: {_adjustAnomaly} (+{(_adjustAnomaly ? _pointsForCorrectAdjustment : 0)})");
        Debug.Log($"Email Blocked: {_emailBlocked} (+{(_emailBlocked ? _pointsForEmailBlocked : 0)})");
        Debug.Log($"Case Score: {caseScore}/20");
        Debug.Log($"Total Score: {_nightCurrentScore}");

        
        if (EmailManager.Instance != null)
            EmailManager.Instance.HideClientEmailAndShowList();

        
        if (_currentCaseIndex >= 9)
        {
            EndNight();
        }
        else
        {
            
            _currentCaseIndex++;
            ResetCaseFlags();
            ResetForm();
            
            
            StartCoroutine(ShowNotificationAfterDelay());
        }
    }

    private IEnumerator SubmitCooldownRoutine()
    {
        yield return new WaitForSeconds(_submitCooldown);
        _canSubmit = true;
    }

    private IEnumerator ShowNotificationAfterDelay()
    {
        
        yield return new WaitForSeconds(5f);

        
        if (EmailManager.Instance != null)
            EmailManager.Instance.LoadNextCase();
        
        Debug.Log("Case Updated");

        
        if (DesktopManager.Instance != null)
            DesktopManager.Instance.ShowEmailNotification();
    }

    private int CalculateCaseScore()
    {
        int score = 0;
        if (_guessAnomaly) score += _pointsForCorrectAnomaly;
        if (_adjustAnomaly) score += _pointsForCorrectAdjustment;
        if (_emailBlocked) score += _pointsForEmailBlocked;
        return score; 
    }

    private void ResetCaseFlags()
    {
        _guessAnomaly = false;
        _adjustAnomaly = false;
        _emailBlocked = false;
    }

    #endregion

    #region Validation Logic

    private bool ValidateAdjustments(CaseDataSO caseData)
    {
        float inputBrightness = ParseFloat(_brightnessInputField != null ? _brightnessInputField.text : "0");
        float inputContrast = ParseFloat(_contrastInputField != null ? _contrastInputField.text : "0");

        bool brightnessCorrect = ValidateBrightness(inputBrightness, caseData.caseBrightness);
        bool contrastCorrect = ValidateContrast(inputContrast, caseData.caseContrast);

        Debug.Log($"Brightness: {inputBrightness} vs {caseData.caseBrightness} = {brightnessCorrect}");
        Debug.Log($"Contrast: {inputContrast} vs {caseData.caseContrast} = {contrastCorrect}");

        return brightnessCorrect && contrastCorrect;
    }

    private bool ValidateBrightness(float input, float correct)
    {
        
        
        
        if (correct > 0)
            return input > (correct - _adjustmentTolerance);
        else if (correct < 0)
            return input < (correct + _adjustmentTolerance);
        else
            return Mathf.Abs(input) <= _adjustmentTolerance;
    }

    private bool ValidateContrast(float input, float correct)
    {
        
        return Mathf.Abs(input - correct) <= _adjustmentTolerance;
    }

    private bool ValidateAnomalyGuess(CaseDataSO caseData)
    {
        if (_anomalyTypeDropdown == null) return false;

        string selectedType = _anomalyTypeDropdown.options[_anomalyTypeDropdown.value].text;
        bool correct = selectedType == caseData.anomalyType.ToString();

        Debug.Log($"Anomaly: {selectedType} vs {caseData.anomalyType} = {correct}");
        
        return correct;
    }

    #endregion

    #region Ending System

    private void EndNight()
    {
        
        ResetCaseFlags();

        
        bool isGoodEnding = _nightCurrentScore > _goodEndingThreshold;
        
        Debug.Log($"=== NIGHT COMPLETED ===");
        Debug.Log($"Final Score: {_nightCurrentScore}/200"); 
        Debug.Log($"Ending: {(isGoodEnding ? "GOOD ENDING" : "BAD ENDING")}");

        
        if (DesktopManager.Instance != null)
            DesktopManager.Instance.ShowEnding(isGoodEnding);
    }

    #endregion

    #region Helper Methods

    private float ParseFloat(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0f;
        if (float.TryParse(text, out float result))
            return result;
        return 0f;
    }

    private void ResetForm()
    {
        if (_brightnessInputField != null)
            _brightnessInputField.text = "0";
        
        if (_contrastInputField != null)
            _contrastInputField.text = "0";
        
        if (_anomalyTypeDropdown != null)
            _anomalyTypeDropdown.value = 0;
    }

    #endregion

    private void OnDestroy()
    {
        if (_submitButton != null)
            _submitButton.OnClicked -= OnSubmitClicked;
    }
}