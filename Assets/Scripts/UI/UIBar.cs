using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Bar for displaying health and mana.
/// </summary>
public class UIBar : MonoBehaviour
{
    [SerializeField] private Image _barImage;
    [SerializeField] private TextMeshProUGUI _text;

    public void UpdateBar(float maxValue, float currentValue)
    {
        _barImage.fillAmount = currentValue / maxValue;
        _text.text = $"{currentValue}/{maxValue}";
    }
}
