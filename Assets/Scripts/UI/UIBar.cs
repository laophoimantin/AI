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

    private float _lastValue;
    private bool _isInitialized = false;

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

        if (!_isInitialized)
        {
            _lastValue = current;
            _isInitialized = true;
        }
        else
        {
            float delta = current - _lastValue;

            if (Mathf.Abs(delta) > 0.1f)
            {
                ShowFloatingNumber(delta);
            }

            _lastValue = current; 
        }
        // ----------------------------------------------

        _barImage.size = new Vector2((current / maxValue) * _maxWidth, _maxHeight);
        _text.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(maxValue)}";
    }

    private void ShowFloatingNumber(float delta)
    {
        TextMeshPro popupText = Instantiate(_text, _text.transform.parent);

        popupText.text = delta > 0 ? $"+{Mathf.RoundToInt(delta)}" : $"{Mathf.RoundToInt(delta)}";
        popupText.color = delta > 0 ? Color.green : Color.red;

        popupText.fontStyle = FontStyles.Bold;
        popupText.fontSize = _text.fontSize * 1.5f;
        popupText.sortingOrder = 100;

        StartCoroutine(FloatAndFadeCoroutine(popupText));
    }

    private IEnumerator FloatAndFadeCoroutine(TextMeshPro popup)
    {
        float duration = 1.5f;
        float elapsed = 0f;

        Vector3 startPos = popup.transform.position;
        Vector3 endPos = startPos + new Vector3(Random.Range(-1.5f, 1.5f), 4.0f, 0f);

        Color startColor = popup.color;
        Vector3 originalScale = popup.transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; 

            float moveT = 1f - Mathf.Pow(1f - t, 3f);
            popup.transform.position = Vector3.Lerp(startPos, endPos, moveT);

            Color c = startColor;
            c.a = t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);
            popup.color = c;

            float scaleMultiplier = 1f + Mathf.Sin(t * Mathf.PI) * 0.8f;
            popup.transform.localScale = originalScale * scaleMultiplier;

            yield return null;
        }

        Destroy(popup.gameObject);
    }
}
