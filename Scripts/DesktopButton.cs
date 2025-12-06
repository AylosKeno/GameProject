using UnityEngine;

[RequireComponent(typeof(MinigameButton))]
public class DesktopButton : MonoBehaviour
{
    private MinigameButton _button;

    private void Awake()
    {
        _button = GetComponent<MinigameButton>();
    }

    private void Start()
    {
        _button.OnClicked += OnDesktopButtonClicked;
    }

    private void OnDesktopButtonClicked()
    {
        CloseAllPanels();
    }

    public void CloseAllPanels()
    {
        if (DesktopManager.Instance == null)
        {
            Debug.LogError("DesktopManager instance not found!");
            return;
        }

        Debug.Log("Desktop button clicked - Closing all panels");

        DesktopManager.Instance.HideAllPanels();
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.OnClicked -= OnDesktopButtonClicked;
        }
    }
}