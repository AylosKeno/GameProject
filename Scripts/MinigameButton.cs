using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinigameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Color Transitions")]
    [SerializeField] private Color _onHoverColor = new Color(1f, 1f, 0f, 1f); 
    [SerializeField] private Color _onClickDownColor = new Color(1f, 0.5f, 0f, 1f); 
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private float _fadeDuration = 0.1f;

    [Header("Audio Settings")]
    [SerializeField] private bool _playHoverSound = true;
    [SerializeField] private bool _playClickSound = true;

    private Image _image;
    private bool _clickedDown;
    private Tween _exitMouseTween;

    
    public event Action OnClicked;

    private void Awake()
    {
        _image = GetComponent<Image>();
        
        if (_image == null)
        {
            Debug.LogError($"MinigameButton on {gameObject.name} requires an Image component!");
        }
        
        
        if (_image != null)
        {
            _image.color = _normalColor;
        }
    }

    #region Unity Event System Handlers

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_image == null) return;
        
        _image.DOColor(_onClickDownColor, _fadeDuration);
        _clickedDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_image == null) return;
        
        _image.DOColor(_normalColor, _fadeDuration);
        
        if (_clickedDown)
        {
            _clickedDown = false;

            
            if (_playClickSound && SettingsManager.Instance != null)
            {
                SettingsManager.Instance.PlayClickSFX();
            }

            OnClicked?.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_image == null) return;
        
        _exitMouseTween.Kill();
        _image.DOColor(_onHoverColor, _fadeDuration);

        
        if (_playHoverSound && SettingsManager.Instance != null)
        {
            SettingsManager.Instance.PlayHoverSFX();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_image == null) return;
        
        _clickedDown = false;
        _image.DOColor(_normalColor, _fadeDuration);
    }

    #endregion

    #region Public Control Methods

    
    
    
    public void SetHoverSound(bool enable)
    {
        _playHoverSound = enable;
    }

    
    
    
    public void SetClickSound(bool enable)
    {
        _playClickSound = enable;
    }

    #endregion

    #region Legacy Methods (Untuk backward compatibility)

    
    
    
    public void OnLeftClickDown()
    {
        OnPointerDown(null);
    }

    
    
    
    public void OnLeftClickUp()
    {
        OnPointerUp(null);
    }

    
    
    
    public void OnHover()
    {
        OnPointerEnter(null);
    }

    #endregion

    private void OnDestroy()
    {
        
        _exitMouseTween.Kill();
        _image?.DOKill();
    }
}