using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Case_00", menuName = "Game Data/Case Data", order = 1)]
public class CaseDataSO : ScriptableObject
{
    [Header("Case Identification")]
    [Tooltip("Case number/index (0, 1, 2, ...)")]
    public int caseIndex;

    [Header("Email Information")]
    [Tooltip("Sender name (e.g., 'John Doe', 'AIC Official')")]
    public string caseSender;

    [Tooltip("Email subject line")]
    public string caseSubject;

    [Tooltip("Email body content")]
    [TextArea(5, 15)]
    public string caseBody;

    [Header("Image Data")]
    [Tooltip("Original image (no anomaly)")]
    public Sprite caseImageOrigin;

    [Tooltip("Image with anomaly")]
    public Sprite caseImageAnomaly;

    [Tooltip("Type of anomaly in this case (None if no anomaly)")]
    public AnomalyType anomalyType;

    [Header("Photo Editor Settings - Correct Values")]
    [Tooltip("Correct brightness value")]
    [Range(-100f, 100f)]
    public float caseBrightness = 0f;

    [Tooltip("Correct contrast value")]
    [Range(-100f, 100f)]
    public float caseContrast = 0f;

    [Header("Metadata")]
    [Tooltip("Is this case already completed?")]
    public bool isCompleted = false;

    public void ResetCase()
    {
        isCompleted = false;
    }

    public void CompleteCase()
    {
        isCompleted = true;
    }

    private void OnValidate()
    {
        if (caseSender.Length > 13)
            caseSender = caseSender.Substring(0, 13);

        if (caseSubject.Length > 30)
            caseSubject = caseSubject.Substring(0, 30);
    }

    public enum AnomalyType
    {
        None,
        ShadowFigure,
        Reflection,
        SpotEyes,
        TextGlitch,
        ShapeDistort,
        ColorDisplace,
        Presence
    }
}