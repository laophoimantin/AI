using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI Bar for displaying health and mana.
public class UIBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _barImage;
    [SerializeField] private TextMeshPro _text;
    private float _maxWidth;
    private float _maxHeight;

    void Awake()
    {
        _maxWidth = _barImage.size.x;
        _maxHeight = _barImage.size.y;
    }

    public void UpdateBar(float maxValue, float currentValue)
    {
        if (maxValue <= 0)
        {
            _barImage.size = new Vector2(0, _maxHeight);
            _text.text = $"0/0";
            return;
        }

        float current = Mathf.Clamp(currentValue, 0, maxValue);

        _barImage.size = new Vector2((current / maxValue) * _maxWidth, _maxHeight);
        _text.text = $"{current}/{maxValue}";
    }
}
