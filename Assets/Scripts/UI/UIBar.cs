using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI Bar for displaying health and mana.
public class UIBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _barImage;
    [SerializeField] private TextMeshProUGUI _text;
    private float _maxWidth;
    private float _maxHeight;

    void Start()
    {
        _maxWidth = _barImage.size.x;
        _maxHeight = _barImage.size.y;
    }
    public void UpdateBar(float maxValue, float currentValue)
    {

        _barImage.size = new Vector2( (currentValue / maxValue) * _maxWidth, _maxHeight);
        _text.text = $"{currentValue}/{maxValue}";
    }
}
