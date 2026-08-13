using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class FantasyButtonText : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private float fontSize = 30f;

    [SerializeField] private Color textColor =
        new Color32(230, 194, 122, 255);

    [SerializeField] private Color outlineColor =
        new Color32(45, 25, 12, 255);

    private TextMeshProUGUI text;

    private void OnEnable()
    {
        ApplyStyle();
    }

    private void OnValidate()
    {
        ApplyStyle();
    }

    private void ApplyStyle()
    {
        text = GetComponent<TextMeshProUGUI>();

        if (text == null)
            return;

        // Boyut
        text.fontSize = fontSize;

        // Kalın görünüm
        text.fontStyle = FontStyles.Bold;

        // Renk
        text.color = textColor;

        // Ortala
        text.alignment = TextAlignmentOptions.Center;

        // Harf aralığı
        text.characterSpacing = 2f;

        // Outline
        text.outlineWidth = 0.18f;
        text.outlineColor = outlineColor;

        // Butonun tamamını kapla
        RectTransform rect = text.rectTransform;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}