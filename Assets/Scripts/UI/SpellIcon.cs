using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    /// <summary>
    /// Manages the visual presentation of a single spell 
    /// Handles the Spell Icon, Cooldown Overlay, and Turn Counter.
    /// </summary>
    public class SpellIcon : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _iconDisplay;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TextMeshProUGUI _cooldownText;

        // Sets the visual sprite
        public void UpdateIcon(Sprite spellIcon)
        {
            if (_iconDisplay.sprite != spellIcon)
                _iconDisplay.sprite = spellIcon;
        }

        // Updates the cooldown visuals
        public void UpdateCooldown(int maxCooldown, int currentCooldown)
        {
            if (currentCooldown <= 0)
            {
                _cooldownOverlay.fillAmount = 0;
                _cooldownText.text = string.Empty;
                return;
            }

            float fillRatio = (float)currentCooldown / maxCooldown;
            _cooldownOverlay.fillAmount = fillRatio;
            _cooldownText.text = currentCooldown.ToString();
        }
    }
}