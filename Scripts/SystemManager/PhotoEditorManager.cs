using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Photo Editor Manager - Handles photo editing with brightness, contrast, saturation
/// NOW SUPPORTS: Loading images from CaseDataSO
/// </summary>
public class PhotoEditorManager : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider brightnessSlider;
    public Slider saturationSlider;
    public Slider contrastSlider;

    [Header("Photo Display")]
    public RawImage photoDisplay;

    [Header("Image Objects - Assign from hierarchy")]
    public RawImage photoOrigin;      // For caseImageOrigin
    public Image photoAnomaly;         // For caseImageAnomaly

    private Material runtimeMaterial;

    // Custom min/max mapping ranges
    private const float BRI_MIN = -0.8f;
    private const float BRI_MAX = 2.8f;

    private const float SAT_MIN = -30f;
    private const float SAT_MAX = 30f;

    private const float CON_MIN = -0.8f;
    private const float CON_MAX = 2.8f;

    // Track current case
    private CaseDataSO _currentCase;

    private void Start()
    {
        // Duplicate the material so we don't modify original asset
        if (photoDisplay != null && photoDisplay.material != null)
        {
            runtimeMaterial = Instantiate(photoDisplay.material);
            photoDisplay.material = runtimeMaterial;
        }

        // Setup slider events
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(_ => ApplyAdjustments());
        
        if (saturationSlider != null)
            saturationSlider.onValueChanged.AddListener(_ => ApplyAdjustments());
        
        if (contrastSlider != null)
            contrastSlider.onValueChanged.AddListener(_ => ApplyAdjustments());

        // Load current case from EmailManager
        LoadCurrentCase();

        ApplyAdjustments();
    }

    /// <summary>
    /// Load current case and update images
    /// Called when Photo Editor panel opens
    /// </summary>
    public void LoadCurrentCase()
    {
        if (EmailManager.Instance == null)
        {
            Debug.LogError("EmailManager instance not found!");
            return;
        }

        _currentCase = EmailManager.Instance.GetCurrentCase();

        if (_currentCase == null)
        {
            Debug.LogError("No case loaded in EmailManager!");
            return;
        }

        // Load images from case data
        LoadImages(_currentCase);

        Debug.Log($"PhotoEditor: Loaded case {_currentCase.caseIndex}");
    }

    /// <summary>
    /// Load images from CaseDataSO
    /// </summary>
    private void LoadImages(CaseDataSO caseData)
    {
        // Load origin image (no anomaly)
        if (photoOrigin != null && caseData.caseImageOrigin != null)
        {
            // Convert Sprite to Texture2D
            Texture2D originTexture = SpriteToTexture(caseData.caseImageOrigin);
            photoOrigin.texture = originTexture;
            
            Debug.Log($"PhotoOrigin loaded: {caseData.caseImageOrigin.name}");
        }
        else if (photoOrigin == null)
        {
            Debug.LogWarning("PhotoOrigin RawImage not assigned!");
        }
        else if (caseData.caseImageOrigin == null)
        {
            Debug.LogWarning("Case has no origin image!");
        }

        // Load anomaly image
        if (photoAnomaly != null && caseData.caseImageAnomaly != null)
        {
            photoAnomaly.sprite = caseData.caseImageAnomaly;
            photoAnomaly.gameObject.SetActive(true); // Show anomaly layer
            
            Debug.Log($"PhotoAnomaly loaded: {caseData.caseImageAnomaly.name}");
        }
        else if (photoAnomaly == null)
        {
            Debug.LogWarning("PhotoAnomaly Image not assigned!");
        }
        else if (caseData.caseImageAnomaly == null)
        {
            Debug.LogWarning("Case has no anomaly image!");
            if (photoAnomaly != null)
                photoAnomaly.gameObject.SetActive(false); // Hide if no anomaly
        }
    }

    /// <summary>
    /// Convert Sprite to Texture2D for RawImage
    /// </summary>
    private Texture2D SpriteToTexture(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            // Sprite is part of atlas, need to extract
            Texture2D newTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.rect.x,
                (int)sprite.rect.y,
                (int)sprite.rect.width,
                (int)sprite.rect.height
            );
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            return newTexture;
        }
        else
        {
            // Sprite is full texture
            return sprite.texture;
        }
    }

    /// <summary>
    /// Apply adjustments to photo material
    /// </summary>
    private void ApplyAdjustments()
    {
        if (runtimeMaterial == null) return;

        // Remap slider (-100 → 100) to custom min/max
        float bri = Mathf.Lerp(BRI_MIN, BRI_MAX, (brightnessSlider.value + 100f) / 200f);
        float sat = Mathf.Lerp(SAT_MIN, SAT_MAX, (saturationSlider.value + 100f) / 200f);
        float con = Mathf.Lerp(CON_MIN, CON_MAX, (contrastSlider.value + 100f) / 200f);

        // Get current HSVC vector
        Vector4 hsvc = runtimeMaterial.GetVector("_HSVC");

        hsvc.x = 0;    // hue stays 0
        hsvc.y = sat;  
        hsvc.z = bri;  
        hsvc.w = con;  

        runtimeMaterial.SetVector("_HSVC", hsvc);

        // Make sure FX blend is ON
        runtimeMaterial.SetFloat("_FxBlend", 1f);
    }

    /// <summary>
    /// Reset sliders to default (0)
    /// </summary>
    public void ResetAdjustments()
    {
        if (brightnessSlider != null)
            brightnessSlider.value = 0;
        
        if (saturationSlider != null)
            saturationSlider.value = 0;
        
        if (contrastSlider != null)
            contrastSlider.value = 0;

        Debug.Log("Photo adjustments reset");
    }

    /// <summary>
    /// Called when PhotoEditor panel opens (from DesktopManager or EmailManager)
    /// </summary>
    private void OnEnable()
    {
        // Reload case when panel opens
        LoadCurrentCase();
    }

    private void OnDestroy()
    {
        // Cleanup event listeners
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveAllListeners();
        
        if (saturationSlider != null)
            saturationSlider.onValueChanged.RemoveAllListeners();
        
        if (contrastSlider != null)
            contrastSlider.onValueChanged.RemoveAllListeners();
    }
}