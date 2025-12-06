using UnityEngine;
using DG.Tweening;





public class PanelAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.3f;
    [SerializeField] private Ease _easeIn = Ease.OutBack;
    [SerializeField] private Ease _easeOut = Ease.InBack;

    [Header("Animation Type")]
    [SerializeField] private AnimationType _animationType = AnimationType.SlideFromBottom;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Vector2 _originalPosition;
    private bool _isAnimating;

    public enum AnimationType
    {
        SlideFromBottom,
        SlideFromTop,
        SlideFromLeft,
        SlideFromRight,
        Scale,
        Fade
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        
        _originalPosition = _rectTransform.anchoredPosition;
    }

    
    
    
    public void Show()
    {
        if (_isAnimating) return;

        gameObject.SetActive(true);
        _isAnimating = true;

        
        SetStartPosition();

        
        AnimateIn(() =>
        {
            _isAnimating = false;
        });
    }

    
    
    
    public void Hide(System.Action onComplete = null)
    {
        if (_isAnimating) return;

        _isAnimating = true;

        
        AnimateOut(() =>
        {
            gameObject.SetActive(false);
            _isAnimating = false;
            onComplete?.Invoke();
        });
    }

    
    
    
    public void ShowImmediate()
    {
        gameObject.SetActive(true);
        _rectTransform.anchoredPosition = _originalPosition;
        _canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
    }

    
    
    
    public void HideImmediate()
    {
        gameObject.SetActive(false);
    }

    private void SetStartPosition()
    {
        switch (_animationType)
        {
            case AnimationType.SlideFromBottom:
                _rectTransform.anchoredPosition = _originalPosition + new Vector2(0, -Screen.height);
                _canvasGroup.alpha = 1f;
                transform.localScale = Vector3.one;
                break;

            case AnimationType.SlideFromTop:
                _rectTransform.anchoredPosition = _originalPosition + new Vector2(0, Screen.height);
                _canvasGroup.alpha = 1f;
                transform.localScale = Vector3.one;
                break;

            case AnimationType.SlideFromLeft:
                _rectTransform.anchoredPosition = _originalPosition + new Vector2(-Screen.width, 0);
                _canvasGroup.alpha = 1f;
                transform.localScale = Vector3.one;
                break;

            case AnimationType.SlideFromRight:
                _rectTransform.anchoredPosition = _originalPosition + new Vector2(Screen.width, 0);
                _canvasGroup.alpha = 1f;
                transform.localScale = Vector3.one;
                break;

            case AnimationType.Scale:
                _rectTransform.anchoredPosition = _originalPosition;
                _canvasGroup.alpha = 1f;
                transform.localScale = Vector3.zero;
                break;

            case AnimationType.Fade:
                _rectTransform.anchoredPosition = _originalPosition;
                _canvasGroup.alpha = 0f;
                transform.localScale = Vector3.one;
                break;
        }
    }

    private void AnimateIn(System.Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        switch (_animationType)
        {
            case AnimationType.SlideFromBottom:
            case AnimationType.SlideFromTop:
            case AnimationType.SlideFromLeft:
            case AnimationType.SlideFromRight:
                sequence.Append(_rectTransform.DOAnchorPos(_originalPosition, _animationDuration).SetEase(_easeIn));
                break;

            case AnimationType.Scale:
                sequence.Append(transform.DOScale(Vector3.one, _animationDuration).SetEase(_easeIn));
                break;

            case AnimationType.Fade:
                sequence.Append(_canvasGroup.DOFade(1f, _animationDuration).SetEase(_easeIn));
                break;
        }

        sequence.OnComplete(() => onComplete?.Invoke());
    }

    private void AnimateOut(System.Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        switch (_animationType)
        {
            case AnimationType.SlideFromBottom:
                sequence.Append(_rectTransform.DOAnchorPos(_originalPosition + new Vector2(0, -Screen.height), _animationDuration).SetEase(_easeOut));
                break;

            case AnimationType.SlideFromTop:
                sequence.Append(_rectTransform.DOAnchorPos(_originalPosition + new Vector2(0, Screen.height), _animationDuration).SetEase(_easeOut));
                break;

            case AnimationType.SlideFromLeft:
                sequence.Append(_rectTransform.DOAnchorPos(_originalPosition + new Vector2(-Screen.width, 0), _animationDuration).SetEase(_easeOut));
                break;

            case AnimationType.SlideFromRight:
                sequence.Append(_rectTransform.DOAnchorPos(_originalPosition + new Vector2(Screen.width, 0), _animationDuration).SetEase(_easeOut));
                break;

            case AnimationType.Scale:
                sequence.Append(transform.DOScale(Vector3.zero, _animationDuration).SetEase(_easeOut));
                break;

            case AnimationType.Fade:
                sequence.Append(_canvasGroup.DOFade(0f, _animationDuration).SetEase(_easeOut));
                break;
        }

        sequence.OnComplete(() => onComplete?.Invoke());
    }

    private void OnDestroy()
    {
        
        _rectTransform?.DOKill();
        _canvasGroup?.DOKill();
        transform?.DOKill();
    }
}

